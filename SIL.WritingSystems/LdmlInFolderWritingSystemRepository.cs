using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using SIL.WritingSystems.Migration;
using SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace SIL.WritingSystems
{
	/// <summary>
	/// A folder-based, LDML writing system repository.
	/// </summary>
	public class LdmlInFolderWritingSystemRepository : LdmlInFolderWritingSystemRepository<WritingSystemDefinition>
	{
		/// <summary>
		/// Returns an instance of an ldml in folder writing system reposistory.
		/// </summary>
		/// <param name="basePath">base location of the global writing system repository</param>
		/// <param name="customDataMappers">The custom data mappers.</param>
		/// <param name="globalRepository">The global repository.</param>
		/// <param name="migrationHandler">Callback if during the initialization any writing system id's are changed</param>
		/// <param name="loadProblemHandler">Callback if during the initialization any writing systems cannot be loaded</param>
		/// <returns></returns>
		public static LdmlInFolderWritingSystemRepository Initialize(
			string basePath,
			IEnumerable<ICustomDataMapper> customDataMappers,
			GlobalWritingSystemRepository globalRepository,
			LdmlVersion0MigrationStrategy.MigrationHandler migrationHandler,
			Action<IEnumerable<WritingSystemRepositoryProblem>> loadProblemHandler
		)
		{
			ICustomDataMapper[] customDataMappersArray = customDataMappers.ToArray();
			var migrator = new LdmlInFolderWritingSystemRepositoryMigrator(basePath, migrationHandler, customDataMappersArray);
			migrator.Migrate();

			var instance = new LdmlInFolderWritingSystemRepository(basePath, customDataMappersArray, globalRepository);
			instance.LoadAllDefinitions();

			// Call the loadProblemHandler with both migration problems and load problems
			var loadProblems = new List<WritingSystemRepositoryProblem>();
			loadProblems.AddRange(migrator.MigrationProblems);
			loadProblems.AddRange(instance.LoadProblems);
			if (loadProblems.Count > 0 && loadProblemHandler != null)
			{
				loadProblemHandler(loadProblems);
			}

			return instance;
		}

		protected internal LdmlInFolderWritingSystemRepository(string basePath)
			: base(basePath)
		{
		}

		protected internal LdmlInFolderWritingSystemRepository(string basePath, IList<ICustomDataMapper> customDataMappers, GlobalWritingSystemRepository globalRepository = null)
			: base(basePath, customDataMappers, globalRepository)
		{
		}

		protected override WritingSystemDefinition ConstructDefinition()
		{
			return new WritingSystemDefinition();
		}

		protected override WritingSystemDefinition ConstructDefinition(string ietfLanguageTag)
		{
			return new WritingSystemDefinition(ietfLanguageTag);
		}

		protected override WritingSystemDefinition CloneDefinition(WritingSystemDefinition ws)
		{
			return ws.Clone();
		}
	}

	/// <summary>
	/// A folder-based, LDML writing system repository.
	/// </summary>
	public abstract class LdmlInFolderWritingSystemRepository<T> : LocalWritingSystemRepositoryBase<T> where T : WritingSystemDefinition
	{
		private const string Extension = ".ldml";
		private string _path;
		private IEnumerable<T> _systemWritingSystemProvider;
		private readonly WritingSystemChangeLog _changeLog;
		private readonly IList<WritingSystemRepositoryProblem> _loadProblems = new List<WritingSystemRepositoryProblem>();
		private readonly IList<ICustomDataMapper> _customDataMappers;
		private readonly GlobalWritingSystemRepository<T> _globalRepository;

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		/// <param name="basePath"></param>
		protected internal LdmlInFolderWritingSystemRepository(string basePath) :
			this(basePath, new List<ICustomDataMapper>())
		{
		}

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		protected internal LdmlInFolderWritingSystemRepository(string basePath, IList<ICustomDataMapper> customDataMappers, GlobalWritingSystemRepository<T> globalRepository = null)
			: base(globalRepository)
		{
			_customDataMappers = customDataMappers;
			_globalRepository = globalRepository;
			PathToWritingSystems = basePath;
			_changeLog = new WritingSystemChangeLog(new WritingSystemChangeLogDataMapper(Path.Combine(PathToWritingSystems, "idchangelog.xml")));
			ReadGlobalWritingSystemsToIgnore();
		}

		/// <summary>
		/// Gets the load problems.
		/// </summary>
		public IList<WritingSystemRepositoryProblem> LoadProblems
		{
			get { return _loadProblems; }
		}

		/// <summary>
		/// Gets or sets the path to the writing systems folder.
		/// </summary>
		public string PathToWritingSystems
		{
			get { return _path; }
			set
			{
				_path = value;
				if (!Directory.Exists(_path))
				{
					string parent = Directory.GetParent(_path).FullName;
					if (!Directory.Exists(parent))
					{
						throw new ApplicationException(
							"The writing system repository cannot be created because its parent folder, " + parent +
							", does not exist.");
					}
					Directory.CreateDirectory(_path);
				}
				LoadAllDefinitions();
			}
		}

		public IEnumerable<ICustomDataMapper> CustomDataMappers
		{
			get { return _customDataMappers; }
		}

		///<summary>
		/// Returns the full path to the underlying store for this writing system.
		///</summary>
		public string GetFilePathFromIetfLanguageTag(string langTag)
		{
			return Path.Combine(PathToWritingSystems, GetFileNameFromIetfLanguageTag(langTag));
		}

		/// <summary>
		/// Gets the file name from the specified identifier.
		/// </summary>
		protected static string GetFileNameFromIetfLanguageTag(string langTag)
		{
			return langTag + Extension;
		}

		/// <summary>
		/// Loads all writing system definitions.
		/// </summary>
		protected void LoadAllDefinitions()
		{
			Clear();
			foreach (string filePath in Directory.GetFiles(_path, "*.ldml"))
				LoadDefinition(filePath);

			LoadChangedIdsFromExistingWritingSystems();
		}

		protected virtual void LoadDefinition(string filePath)
		{
			T wsFromFile;
			try
			{
				wsFromFile = CreateNew();
				var ldmlDataMapper = new LdmlDataMapper();
				if (File.Exists(filePath))
				{
					ldmlDataMapper.Read(filePath, wsFromFile);
					foreach (ICustomDataMapper customDataMapper in _customDataMappers)
						customDataMapper.Read(wsFromFile);
					wsFromFile.Id = Path.GetFileNameWithoutExtension(filePath);
				}
			}
			catch (Exception e)
			{
				// Add the exception to our list of problems and continue loading
				var problem = new WritingSystemRepositoryProblem
					{
						Consequence = WritingSystemRepositoryProblem.ConsequenceType.WSWillNotBeAvailable,
						Exception = e,
						FilePath = filePath
					};
				_loadProblems.Add(problem);
				return;
			}

			if (string.Compare(wsFromFile.Id, wsFromFile.IetfLanguageTag, StringComparison.OrdinalIgnoreCase) != 0)
			{
				bool badFileName = true;
				if (wsFromFile.Id != null && wsFromFile.Id.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
				{
					var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
					interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag(wsFromFile.Id);
					if (interpreter.Rfc5646Tag.Equals(wsFromFile.IetfLanguageTag, StringComparison.OrdinalIgnoreCase))
					{
						badFileName = false;
					}
				}
				if (badFileName)
				{// Add the exception to our list of problems and continue loading
					var problem = new WritingSystemRepositoryProblem
					{
						Consequence = WritingSystemRepositoryProblem.ConsequenceType.WSWillNotBeAvailable,
						Exception = new ApplicationException(
							String.Format(
								"The writing system file {0} seems to be named inconsistently. It contains the IETF language tag: '{1}'. The name should have been made consistent with its content upon migration of the writing systems.",
								filePath, wsFromFile.IetfLanguageTag)),
						FilePath = filePath
					};
					_loadProblems.Add(problem);
				}
			}
			try
			{
				Set(wsFromFile);
			}
			catch (Exception e)
			{
				// Add the exception to our list of problems and continue loading
				var problem = new WritingSystemRepositoryProblem
				{
					Consequence = WritingSystemRepositoryProblem.ConsequenceType.WSWillNotBeAvailable,
					Exception = e,
					FilePath = filePath
				};
				_loadProblems.Add(problem);
			}
		}

		private bool HaveMatchingDefinitionInTrash(string identifier)
		{
			string path = PathToWritingSystemTrash();
			path = Path.Combine(path, GetFileNameFromIetfLanguageTag(identifier));
			return File.Exists(path);
		}

		private void AddActiveOSLanguages()
		{
			foreach (T ws in _systemWritingSystemProvider)
			{
				if (null == FindAlreadyLoadedWritingSystem(ws.IetfLanguageTag))
				{
					if (!HaveMatchingDefinitionInTrash(ws.IetfLanguageTag))
					{
						Set(ws);
					}
				}
			}
		}

		/// <summary>
		/// Provides writing systems from a repository that comes, for example, with the OS
		/// </summary>
		public IEnumerable<T> SystemWritingSystemProvider
		{
			get{ return _systemWritingSystemProvider;}
			set
			{
				if(_systemWritingSystemProvider == value){ return;}
				_systemWritingSystemProvider = value;
				AddActiveOSLanguages();
			}
		}

		private T FindAlreadyLoadedWritingSystem(string wsID)
		{
			return AllWritingSystems.FirstOrDefault(ws => ws.IetfLanguageTag == wsID);
		}

		/// <summary>
		/// Saves a writing system definition.
		/// </summary>
		protected internal virtual void SaveDefinition(T ws)
		{
			Set(ws);

			string writingSystemFilePath = GetFilePathFromIetfLanguageTag(ws.IetfLanguageTag);
			if (!File.Exists(writingSystemFilePath) && !string.IsNullOrEmpty(ws.Template))
			{
				// this is a new writing system that was generated from a template, so copy the template over before saving
				File.Copy(ws.Template, writingSystemFilePath);
				ws.Template = null;
			}

			MemoryStream oldData = null;
			if (!ws.IsChanged && File.Exists(writingSystemFilePath))
				return; // no need to save (better to preserve the modified date)
			ws.DateModified = DateTime.UtcNow;
			if (File.Exists(writingSystemFilePath))
			{
				// load old data to preserve stuff in LDML that we don't use, but don't throw up an error if it fails
				try
				{
					oldData = new MemoryStream(File.ReadAllBytes(writingSystemFilePath), false);
				}
				catch {}
				// What to do?  Assume that the UI has already checked for existing, asked, and allowed the overwrite.
				File.Delete(writingSystemFilePath); //!!! Should this be move to trash?
			}
			var ldmlDataMapper = new LdmlDataMapper();
			ldmlDataMapper.Write(writingSystemFilePath, ws, oldData);
			foreach (ICustomDataMapper customDataMapper in _customDataMappers)
				customDataMapper.Write(ws);
			ws.AcceptChanges();

			if (ChangedIds.Any(p => p.Value == ws.Id))
			{
				// log this id change to the writing system change log
				KeyValuePair<string, string> pair = ChangedIds.First(p => p.Value == ws.Id);
				_changeLog.LogChange(pair.Key, pair.Value);
			}
			else
			{
				// log this addition
				_changeLog.LogAdd(ws.Id);
			}
		}

		public override T CreateNew(string ietfLanguageTag)
		{
			string templatePath = null;
			// check local repo for template
			T existingWS;
			if (TryGet(ietfLanguageTag, out existingWS))
			{
				templatePath = GetFilePathFromIetfLanguageTag(existingWS.IetfLanguageTag);
				if (!File.Exists(templatePath))
					templatePath = null;
			}

			// check global repo for template
			if (string.IsNullOrEmpty(templatePath) && _globalRepository != null && _globalRepository.TryGet(ietfLanguageTag, out existingWS))
			{
				templatePath = _globalRepository.GetFilePathFromIetfLanguageTag(existingWS.IetfLanguageTag);
				if (!File.Exists(templatePath))
					templatePath = null;
			}

			// check SLDR for template
			if (string.IsNullOrEmpty(templatePath))
			{
				string sldrCachePath = Path.Combine(Path.GetTempPath(), "SldrCache");
				Directory.CreateDirectory(sldrCachePath);
				templatePath = Path.Combine(sldrCachePath, ietfLanguageTag + ".ldml");
				if (!GetLdmlFromSldr(templatePath, ietfLanguageTag))
				{
					// check SLDR cache for template
					if (!File.Exists(templatePath))
						templatePath = null;
				}
			}

			// check template folder for template
			if (string.IsNullOrEmpty(templatePath) && !string.IsNullOrEmpty(TemplateFolder))
			{
				templatePath = Path.Combine(TemplateFolder, ietfLanguageTag + ".ldml");
				if (!File.Exists(templatePath))
					templatePath = null;
			}

			T ws;
			if (!string.IsNullOrEmpty(templatePath))
			{
				ws = ConstructDefinition();
				var loader = new LdmlDataMapper();
				loader.Read(templatePath, ws);
				ws.Template = templatePath;
			}
			else
			{
				ws = ConstructDefinition(ietfLanguageTag);
			}

			return ws;
		}

		/// <summary>
		/// Gets the a LDML file from the SLDR.
		/// </summary>
		protected virtual bool GetLdmlFromSldr(string path, string id)
		{
			try
			{
				Sldr.GetLdmlFile(path, id);
				return true;
			}
			catch (WebException)
			{
				return false;
			}
		}

		/// <summary>
		/// The folder in which the repository looks for template LDML files when a writing system is wanted
		/// that cannot be found in the local store, global store, or SLDR.
		/// </summary>
		public string TemplateFolder { get; set; }

		public override void Conflate(string wsToConflate, string wsToConflateWith)
		{
			//conflation involves deleting the old writing system. That deletion should not appear int he log. which is what the "_conflating" is used for
			base.Conflate(wsToConflate, wsToConflateWith);
			_changeLog.LogConflate(wsToConflate, wsToConflateWith);
		}

		public override void Remove(string id)
		{
			base.Remove(id);
			_changeLog.LogDelete(id);
		}

		protected override void RemoveDefinition(T ws)
		{
			int wsIgnoreCount = WritingSystemsToIgnore.Count;

			//we really need to get it in the trash, else, if was auto-provided,
			//it'll keep coming back!
			if (!File.Exists(GetFilePathFromIetfLanguageTag(ws.IetfLanguageTag)))
				SaveDefinition(ws);

			if (File.Exists(GetFilePathFromIetfLanguageTag(ws.IetfLanguageTag)))
			{
				Directory.CreateDirectory(PathToWritingSystemTrash());
				string destination = Path.Combine(PathToWritingSystemTrash(), GetFileNameFromIetfLanguageTag(ws.IetfLanguageTag));
				//clear out any old on already in the trash
				if (File.Exists(destination))
					File.Delete(destination);
				File.Move(GetFilePathFromIetfLanguageTag(ws.IetfLanguageTag), destination);
			}
			base.RemoveDefinition(ws);
			foreach (ICustomDataMapper customDataMapper in _customDataMappers)
				customDataMapper.Remove(ws.IetfLanguageTag);

			if (wsIgnoreCount != WritingSystemsToIgnore.Count)
				WriteGlobalWritingSystemsToIgnore();
		}

		private string PathToWritingSystemTrash()
		{
			return Path.Combine(_path, "trash");
		}

		/// <summary>
		/// Return true if it will be possible (absent someone changing permissions while we aren't looking)
		/// to save changes to the specified writing system.
		/// </summary>
		public override bool CanSave(T ws)
		{
			string filePath = GetFilePathFromIetfLanguageTag(ws.IetfLanguageTag);
			if (File.Exists(filePath))
			{
				try
				{
					using (FileStream stream = File.Open(filePath, FileMode.Open))
						stream.Close();
					// don't really want to change anything
				}
				catch (UnauthorizedAccessException)
				{
					return false;
				}
			}
			else if (Directory.Exists(PathToWritingSystems))
			{
				try
				{
					// See whether we're allowed to create the file (but if so, get rid of it).
					// Pathologically we might have create but not delete permission...if so,
					// we'll create an empty file and report we can't save. I don't see how to
					// do better.
					using (FileStream stream = File.Create(filePath))
						stream.Close();
					File.Delete(filePath);
				}
				catch (UnauthorizedAccessException)
				{
					return false;
				}
			}
			else
			{
				try
				{
					Directory.CreateDirectory(PathToWritingSystems);
					// Don't try to clean it up again. This is a vanishingly rare case,
					// I don't think it's even possible to create a writing system store without
					// the directory existing.
				}
				catch (UnauthorizedAccessException)
				{
					return false;
				}
			}
			return true;
		}

		public override void Save()
		{
			int wsIgnoreCount = WritingSystemsToIgnore.Count;

			//delete anything we're going to delete first, to prevent losing
			//a WS we want by having it deleted by an old WS we don't want
			//(but which has the same identifier)
			foreach (string id in AllWritingSystems.Where(ws => ws.MarkedForDeletion).Select(ws => ws.Id).ToArray())
				Remove(id);

			// make a copy and then go through that list - SaveDefinition calls Set which
			// may delete and then insert the same writing system - which would change WritingSystemDefinitions
			// and not be allowed in a foreach loop
			foreach (T ws in AllWritingSystems.Where(CanSet).ToArray())
			{
				SaveDefinition(ws);
				OnChangeNotifySharedStore(ws);
			}

			LoadChangedIdsFromExistingWritingSystems();

			if (wsIgnoreCount != WritingSystemsToIgnore.Count)
				WriteGlobalWritingSystemsToIgnore();

			base.Save();
		}

		public override void Set(T ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			string oldStoreId = ws.Id;
			base.Set(ws);
			//Renaming the file here is a bit ugly as the content has not yet been updated. Thus there
			//may be a mismatch between the filename and the contained rfc5646 tag. Doing it here however
			//helps us avoid having to deal with situations where a writing system id is changed to be
			//identical with the old id of another writing sytsem. This could otherwise lead to dataloss.
			//The inconsistency is resolved on Save()
			if (oldStoreId != ws.Id && File.Exists(GetFilePathFromIetfLanguageTag(oldStoreId)))
				File.Move(GetFilePathFromIetfLanguageTag(oldStoreId), GetFilePathFromIetfLanguageTag(ws.Id));
		}

		public override bool WritingSystemIdHasChanged(string id)
		{
			return _changeLog.HasChangeFor(id);
		}

		public override string WritingSystemIdHasChangedTo(string id)
		{
			return AllWritingSystems.Any(ws => ws.IetfLanguageTag.Equals(id)) ? id : _changeLog.GetChangeFor(id);
		}

		protected override void LastChecked(string identifier, DateTime dateModified)
		{
			base.LastChecked(identifier, dateModified);
			WriteGlobalWritingSystemsToIgnore();
		}

		private void WriteGlobalWritingSystemsToIgnore()
		{
			if (_globalRepository == null)
				return;

			string path = Path.Combine(PathToWritingSystems, "WritingSystemsToIgnore.xml");

			if (WritingSystemsToIgnore.Count == 0)
			{
				if (File.Exists(path))
					File.Delete(path);
			}

			else
			{
				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
					new XElement("WritingSystems",
						WritingSystemsToIgnore.Select(ignoredWs => new XElement("WritingSystem", new XAttribute("id", ignoredWs.Key), new XAttribute("dateModified", ignoredWs.Value.ToString("s"))))));
				doc.Save(path);
			}
		}

		private void ReadGlobalWritingSystemsToIgnore()
		{
			string path = Path.Combine(PathToWritingSystems, "WritingSystemsToIgnore.xml");
			if (_globalRepository == null || !File.Exists(path))
				return;

			XElement wssElem = XElement.Load(path);
			foreach (XElement wsElem in wssElem.Elements("WritingSystem"))
			{
				DateTime dateModified = DateTime.ParseExact((string) wsElem.Attribute("dateModified"), "s", null, DateTimeStyles.AdjustToUniversal);
				WritingSystemsToIgnore[(string)wsElem.Attribute("id")] = dateModified;
			}
		}
	}
}