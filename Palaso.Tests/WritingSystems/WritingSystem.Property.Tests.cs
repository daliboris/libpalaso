using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemPropertyTests
	{


		[Test]
		public void DisplayLabelWhenUnknown()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.AreEqual("???", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabel_NoAbbreviation_UsesRFC5646()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "en";
			ws.Variant = "1901";
			Assert.AreEqual("en-1901", ws.DisplayLabel);
		}

		[Test]
		public void DisplayLabel_LanguageTagIsDefaultHasAbbreviation_ShowsAbbreviation()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.Abbreviation = "xyz";
			Assert.AreEqual("xyz", ws.DisplayLabel);
		}

		[Test]
		public void Variant_ConsistsOnlyOfRfc5646Variant_VariantIsSetCorrectly()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa";
			Assert.AreEqual("fonipa", ws.Variant);
		}

		[Test]
		public void Variant_ConsistsOnlyOfRfc5646PrivateUse_VariantIsSetCorrectly()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "x-etic";
			Assert.AreEqual("x-etic", ws.Variant);
		}

		[Test]
		public void Variant_ConsistsOfBothRfc5646VariantandprivateUse_VariantIsSetCorrectly()
		{
			var ws = new WritingSystemDefinition();
			ws.Variant = "fonipa-x-etic";
			Assert.AreEqual("fonipa-x-etic", ws.Variant);
		}

		[Test]
		public void DisplayLabel_OnlyHasLanguageName_UsesFirstPartOfLanguageName()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.LanguageName = "abcdefghijk";
			Assert.AreEqual("abcd", ws.DisplayLabel);
		}

		[Test]
		public void Rfc5646_HasOnlyAbbreviation_ReturnsQaa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition(){Abbreviation = "hello"};
			Assert.AreEqual("qaa", ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en","","","","", false);
			Assert.AreEqual("en", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoAndScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Zxxx", "", "", "", false);
			Assert.AreEqual("en-Zxxx", ws.RFC5646);
		}

		[Test]
		public void Rfc5646WhenIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "", "US", "", "", false);
			Assert.AreEqual("en-US", ws.RFC5646);
		}
		[Test]
		public void Rfc5646WhenIsoScriptRegionVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Zxxx", "US", "1901", "", false);
			Assert.AreEqual("en-Zxxx-US-1901", ws.RFC5646);
		}

		[Test]
		public void ReadsScriptRegistry()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Greater(WritingSystemDefinition.ScriptOptions.Count, 4);
		}


		[Test]
		public void ReadsISORegistry()
		{
			Assert.Greater(WritingSystemDefinition.ValidIso639LanguageCodes.Count, 100);
		}

		[Test]
		public void VerboseDescriptionWhenNoSubtagsSet()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("", "", "", "", "", false);
			Assert.AreEqual("Unknown language. (qaa)", ws.VerboseDescription);
		}

		[Test]
		public void VerboseDescriptionWhenJustISO()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "", "", "", "", false);
			Assert.AreEqual("English. (en)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoAndScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Kore", "", "", "", false);
			Assert.AreEqual("English written in Korean script. (en-Kore)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenOnlyScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("", "Kore", "", "", "", false);
			Assert.AreEqual("Unknown language written in Korean script. (qaa-Kore)", ws.VerboseDescription);
		}

		[Test]
		public void VerboseDescriptionWhenIsoAndRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "", "US", "", "", false);
			Assert.AreEqual("English in US. (en-US)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoScriptRegionVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Kore", "US", "1901", "", false);
			Assert.AreEqual("English in US written in Korean script. (en-Kore-US-1901)", ws.VerboseDescription);
		}
		[Test]
		public void VerboseDescriptionWhenIsoIsUnsetButLanguageNameIs()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("", "Kore", "US", "1901", "", false);
			ws.LanguageName = "Eastern lawa";
			Assert.AreEqual("Eastern lawa in US written in Korean script. (qaa-Kore-US-1901)", ws.VerboseDescription);
		}

		[Test]
		public void HasLotsOfScriptOptions()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Greater(WritingSystemDefinition.ScriptOptions.Count, 40);
		}


		[Test]
		public void CurrentScriptOptionReturnCorrectScript()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition("en", "Kore", "", "", "", false);
			Assert.AreEqual("Korean", ws.Iso15924Script.Label);
		}

		[Test]
		public void ModifyingDefinitionSetsModifiedFlag()
		{
			// Put any properties to ignore in this string surrounded by "|"
			const string ignoreProperties = "|Modified|MarkedForDeletion|StoreID|DateModified|Rfc5646TagOnLoad|";
			// special test values to use for properties that are particular
			Dictionary<string, object> firstValueSpecial = new Dictionary<string, object>();
			Dictionary<string, object> secondValueSpecial = new Dictionary<string, object>();
			firstValueSpecial.Add("Variant", "1901");
			secondValueSpecial.Add("Variant", "biske");
			firstValueSpecial.Add("Region", "US");
			secondValueSpecial.Add("Region", "GB");
			firstValueSpecial.Add("ISO", "en");
			secondValueSpecial.Add("ISO", "de");
			firstValueSpecial.Add("Script", "Zxxx");
			secondValueSpecial.Add("Script", "Latn");
			//firstValueSpecial.Add("SortUsing", "CustomSimple");
			//secondValueSpecial.Add("SortUsing", "CustomICU");
			// test values to use based on type
			Dictionary<Type, object> firstValueToSet = new Dictionary<Type, object>();
			Dictionary<Type, object> secondValueToSet = new Dictionary<Type, object>();
			firstValueToSet.Add(typeof (float), 2.18281828459045f);
			secondValueToSet.Add(typeof (float), 3.141592653589f);
			firstValueToSet.Add(typeof (bool), true);
			secondValueToSet.Add(typeof (bool), false);
			firstValueToSet.Add(typeof (string), "X");
			secondValueToSet.Add(typeof (string), "Y");
			firstValueToSet.Add(typeof (DateTime), new DateTime(2007, 12, 31));
			secondValueToSet.Add(typeof (DateTime), new DateTime(2008, 1, 1));
			firstValueToSet.Add(typeof(WritingSystemDefinition.SortRulesType), WritingSystemDefinition.SortRulesType.CustomICU);
			secondValueToSet.Add(typeof(WritingSystemDefinition.SortRulesType), WritingSystemDefinition.SortRulesType.CustomSimple);
			firstValueToSet.Add(typeof(RFC5646Tag), new RFC5646Tag("de", "Latn", "", "1901","x-audio"));

			firstValueToSet.Add(typeof(IpaStatusChoices), IpaStatusChoices.IpaPhonemic);
			secondValueToSet.Add(typeof(IpaStatusChoices), IpaStatusChoices.NotIpa);

			foreach (PropertyInfo propertyInfo in typeof(WritingSystemDefinition).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				// skip read-only or ones in the ignore list
				if (!propertyInfo.CanWrite || ignoreProperties.Contains("|" + propertyInfo.Name + "|"))
				{
					continue;
				}
				WritingSystemDefinition ws = new WritingSystemDefinition();
				ws.Modified = false;
				// We need to ensure that all values we are setting are actually different than the current values.
				// This could be accomplished by comparing with the current value or by setting twice with different values.
				// We use the setting twice method so we don't require a getter on the property.
				try
				{
					if (firstValueSpecial.ContainsKey(propertyInfo.Name) && secondValueSpecial.ContainsKey(propertyInfo.Name))
					{
						propertyInfo.SetValue(ws, firstValueSpecial[propertyInfo.Name], null);
						propertyInfo.SetValue(ws, secondValueSpecial[propertyInfo.Name], null);
					}
					else if (firstValueToSet.ContainsKey(propertyInfo.PropertyType) && secondValueToSet.ContainsKey(propertyInfo.PropertyType))
					{
						propertyInfo.SetValue(ws, firstValueToSet[propertyInfo.PropertyType], null);
						propertyInfo.SetValue(ws, secondValueToSet[propertyInfo.PropertyType], null);
					}
					else
					{
						Assert.Fail("Unhandled property type - please update the test to handle type {0}",
									propertyInfo.PropertyType.Name);
					}
				}
				catch(Exception error)
				{
					Assert.Fail("Error setting property WritingSystemDefinition.{0},{1}", propertyInfo.Name, error.ToString());
				}
				Assert.IsTrue(ws.Modified, "Modifying WritingSystemDefinition.{0} did not change modified flag.", propertyInfo.Name);
			}
		}

		[Test]
		public void CloneCopiesAllNeededMembers()
		{
			// Put any fields to ignore in this string surrounded by "|"
			const string ignoreFields = "|Modified|MarkedForDeletion|StoreID|_collator|";
			// values to use for testing different types
			var valuesToSet = new Dictionary<Type, object>
			{
				{typeof (float), 3.14f},
				{typeof (bool), true},
				{typeof (string), "Foo"},
				{typeof (DateTime), DateTime.Now},
				{typeof (WritingSystemDefinition.SortRulesType), WritingSystemDefinition.SortRulesType.CustomICU}
			};
			foreach (var fieldInfo in typeof(WritingSystemDefinition).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				var fieldName = fieldInfo.Name;
				if (fieldInfo.Name.Contains("<"))
				{
					var splitResult = fieldInfo.Name.Split(new[] {'<', '>'});
					fieldName = splitResult[1];
				}
				if (ignoreFields.Contains("|" + fieldName + "|"))
				{
					continue;
				}
				var ws = new WritingSystemDefinition();
				if (valuesToSet.ContainsKey(fieldInfo.FieldType))
				{
					fieldInfo.SetValue(ws, valuesToSet[fieldInfo.FieldType]);
				}
				else
				{
					Assert.Fail("Unhandled field type - please update the test to handle type {0}", fieldInfo.FieldType.Name);
				}
				var theClone = ws.Clone();
				Assert.AreEqual(valuesToSet[fieldInfo.FieldType], fieldInfo.GetValue(theClone), "Field {0} not copied on WritingSystemDefinition.Clone()", fieldInfo.Name);
			}
		}

		[Test]
		public void SortUsingDefaultOrdering_ValidateSortRulesWhenEmpty_IsTrue()
		{
			var ws = new WritingSystemDefinition();
			string message;
			Assert.IsTrue(ws.ValidateCollationRules(out message));
		}

		[Test]
		public void SortUsingDefaultOrdering_ValidateSortRulesWhenNotEmpty_IsFalse()
		{
			var ws = new WritingSystemDefinition();
			ws.SortRules = "abcd";
			string message;
			Assert.IsFalse(ws.ValidateCollationRules(out message));
		}

		[Test]
		public void SetIsVoice_SetToTrue_SetsScriptRegionAndVariantCorrectly()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 Script = "Latn",
												 Region = "US",
												 Variant = "1901"
											 };
			ws.SetIsVoice(true);
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.AreEqual("US", ws.Region);
			Assert.AreEqual("1901-x-audio", ws.Variant);
			Assert.AreEqual("qaa-Zxxx-US-1901-x-audio", ws.RFC5646);
		}

		[Test]
		public void IsVoice_SetToTrue_LeavesIsoCodeAlone()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 ISO = "en-GB"
											 };
			ws.IsVoice = true;
			Assert.AreEqual("en-GB", ws.ISO);
		}

		[Test]
		public void IsVoice_SetToFalseFromTrue_ScriptStaysZxxx()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true
			};
			ws.IsVoice = false;
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.AreEqual("", ws.Region);
			Assert.AreEqual("", ws.Variant);
		}

		[Test]
		public void Script_ChangedToSomethingOtherThanZxxxWhileIsVoiceIsTrue_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 IsVoice = true
											 };
			Assert.Throws<ArgumentException>(() => ws.Script = "change!");
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_ScriptSetToZxxxAndVariantSetToXDashAudio_SetsIsVoiceToTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("",WellKnownSubTags.Audio.Script,"",WellKnownSubTags.Audio.PrivateUseSubtag);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void SetAllRfc5646LanguageTagComponents_ScriptSetToZxXxAndVariantSetToXDashAuDiO_SetsIsVoiceToTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", "ZxXx", "", "X-AuDiO");
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Variant_ChangedToSomethingOtherThanXDashAudioWhileIsVoiceIsTrue_IsVoiceIsChangedToFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true
			};
			ws.Variant = "change!";
			Assert.AreEqual("change!", ws.Variant);
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Iso_SetToSmthWithDashesWhileIsVoiceIsTrue_IsoIsSet()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
											 {
												 IsVoice = true,
											 };
			ws.ISO = "iso-script-region-variant";
			Assert.AreEqual("iso-script-region-variant", ws.ISO);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void Iso_SetToSmthContainingZxxxDashxDashaudioWhileIsVoiceIsTrue_DontKnowWhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				IsVoice = true,
			};
			ws.ISO = "iso-Zxxx-x-audio";
			Assert.AreEqual("en", ws.ISO);
			Assert.AreEqual(WellKnownSubTags.Audio.PrivateUseSubtag, ws.Variant);
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
			Assert.IsTrue(ws.IsVoice);
			throw new NotImplementedException();
		}

		[Test]
		public void IsVoice_ToggledAfterVariantHasBeenSet_DoesNotRemoveVariant()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Variant = "variant"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual("variant", ws.Variant);
		}

		[Test]
		public void IsVoice_ToggledAfterRegionHasBeenSet_DoesNotRemoveRegion()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Region = "Region"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual("Region", ws.Region);
		}

		[Test]
		public void IsVoice_ToggledAfterScriptHasBeenSet_ScriptIsChangedToZxxx()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition()
			{
				Script = "Script"
			};
			ws.IsVoice = true;
			ws.IsVoice = false;
			Assert.AreEqual(WellKnownSubTags.Audio.Script, ws.Script);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IpaStatusIsSetToNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpaPhontetic_IpaStatusIsSetToNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonetic);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpaPhonemic_IpaStatusIsSetToNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonemic);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void Variant_IsSetWithDuplicateTags_DontKnowWhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition(){Variant = "duplicate-duplicate"};
			throw new NotImplementedException();
		}

		[Test]
		public void Variant_SetToXDashAudioWhileScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag);
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsXDashAudio_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag);
			Assert.Throws<ArgumentException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void Variant_SetToCapitalXDASHAUDIOWhileScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag.ToUpper());
		}

		[Test]
		public void Script_SetToOtherThanZxxxWhileVariantIsCapitalXDASHAUDIO_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag.ToUpper());
			Assert.Throws<ArgumentException>(() => ws.Script = "Ltn");
		}

		[Test]
		public void IsVoice_VariantIsPrefixXDashAudioPostFix_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition ();
			ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", "Prefixx-audioPostfix");
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void Variant_ContainsXDashAudioAndFonipa_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				()=>ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.IpaVariantSubtag));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhoneticMarker_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.IpaPhoneticPrivateUseSubtag));
		}

		[Test]
		public void Variant_ContainsXDashAudioAndPhonemicMarker_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(
				() => ws.SetAllRfc5646LanguageTagComponents("", WellKnownSubTags.Audio.Script, "", WellKnownSubTags.Audio.PrivateUseSubtag + "-" + WellKnownSubTags.Ipa.IpaPhonemicPrivateUseSubtag));
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IsVoiceIsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(true);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsIpa_IpaStatusIsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToFalseWhileIpaStatusIsIpa_IsVoiceIsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(false);
			Assert.IsFalse(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToFalseWhileIpaStatusIsIpa_IpaStatusIsIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.Ipa);
			ws.SetIsVoice(false);
			Assert.AreEqual(IpaStatusChoices.Ipa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonetic_IsVoiceIsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonetic);
			ws.SetIsVoice(true);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonetic_IpaStatusIsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonetic);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonemic_IsVoiceIsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonemic);
			ws.SetIsVoice(true);
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsVoice_SetToTrueWhileIpaStatusIsPhonemic_IpaStatusIsNotIpa()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.SetIpaStatus(IpaStatusChoices.IpaPhonemic);
			ws.SetIsVoice(true);
			Assert.AreEqual(IpaStatusChoices.NotIpa, ws.IpaStatus);
		}

		[Test]
		public void Iso_IsEmpty_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(()=>ws.ISO = String.Empty);
		}

		[Test]
		public void Variant_ContainsUnderscore_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			Assert.Throws<ArgumentException>(() => ws.Variant = "x_audio");
		}

		[Test]
		public void Variant_ContainsCapitalXDashAUDIOAndScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "bogus";
			Assert.Throws<ArgumentException>(() => ws.Variant = "X-AUDIO");
		}

		[Test]
		public void Variant_IndicatesThatWsIsAudioAndScriptIsCapitalZXXX_ReturnsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "ZXXX";
			ws.Variant = WellKnownSubTags.Audio.PrivateUseSubtag;
			Assert.IsTrue(ws.IsVoice);
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioButContainsotherThanJustTheNecassaryXDashAudioTagAndScriptIsNotZxxx_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "ltn";
			Assert.Throws<ArgumentException>(()=>ws.Variant = "x-private-x-audio");
		}

		[Test]
		public void LanguageSubtag_ContainsXDashAudio_WhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO = "de-x-audio");
			throw new NotImplementedException();
		}

		[Test]
		public void Language_ContainsZxxx_WhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de-Zxxx";
			throw new NotImplementedException();
		}

		[Test]
		public void LanguageSubtag_ContainsCapitalXDashAudio_WhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(() => ws.ISO = "de-X-AuDiO");
			throw new NotImplementedException();
		}

		[Test]
		public void LanguageSubtag_ContainsCapitalZxxx_WhatToDo()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de-ZXXX";
			throw new NotImplementedException();
		}

		[Test]
		public void Language_SetWithInvalidLanguageTag_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Script_SetWithInvalidScriptTag_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Region_SetWithInvalidRegionTag_Throws()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Variant_SetPrivateUseTag_VariantisSet()
		{
			throw new NotImplementedException();
		}
	}
}