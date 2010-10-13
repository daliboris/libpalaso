using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Palaso.Keyboarding;
using Palaso.UI.WindowsForms.Keyboarding;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	internal class WindowsIMEAdaptor
	{
		public static void ActivateKeyboard(KeyboardDescriptor keyboard)
		{
			if (keyboard == null || ((keyboard.KeyboardingEngine != Engines.Windows) && (keyboard.KeyboardingEngine != Engines.Unknown)))
			{
				return;
			}

			try
			{
				InputLanguage inputLanguage = FindInputLanguage(keyboard);
				if (inputLanguage != null)
				{
					InputLanguage.CurrentInputLanguage = inputLanguage;
				}
				else
				{
					Palaso.Reporting.ProblemNotificationDialog.Show("The keyboard '" + keyboard.KeyboardName + "' could not be activated using windows ime.");
				}
			}
			catch (Exception )
			{
				Palaso.Reporting.ProblemNotificationDialog.Show("There was an error trying to access the windows ime.");
			}
		}

		public static void ActivateKeyboard(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				return;
			}

			try
			{
				InputLanguage inputLanguage = FindInputLanguageByName(name);
				if (inputLanguage != null)
				{
					InputLanguage.CurrentInputLanguage = inputLanguage;
				}
				else
				{
					Palaso.Reporting.ProblemNotificationDialog.Show("The keyboard '" + name + "' could not be activated using windows ime.");
				}
			}
			catch (Exception)
			{
				Palaso.Reporting.ProblemNotificationDialog.Show("There was an error trying to access the windows ime.");
			}
		}

		public static bool HasKeyboard(KeyboardDescriptor keyboard)
		{
			if ((keyboard.KeyboardingEngine != Engines.Windows) && (keyboard.KeyboardingEngine != Engines.Unknown)) return false;
			return (FindInputLanguage(keyboard) != null);
		}

		static private InputLanguage FindInputLanguage(KeyboardDescriptor keyboard)
		{
			bool idHasRightFormat = true;
			try
			{
				IntPtr inputLanguageHandle = new IntPtr(Convert.ToInt64(keyboard.Id));
				return FindInputLanguageByHandle(inputLanguageHandle);
			}
			catch(FormatException)
			{
				return FindInputLanguageByName(keyboard.KeyboardName);
			}
		}

		private static InputLanguage FindInputLanguageByHandle(IntPtr inputLanguageHandle)
		{
			if (InputLanguage.InstalledInputLanguages != null) // as is the case on Linux
			{
				foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
				{
					if (l.Handle == inputLanguageHandle)
					{
						return l;
					}
				}
			}
			return null;
		}

		[Obsolete("Using keyboard descriptors rather than keyboard names improve keyboard control across localized versions of windows.")]
		public static bool HasKeyboardNamed(string name)
		{
			return (null != FindInputLanguageByName(name));
		}

		static private InputLanguage FindInputLanguageByName(string name)
		{
			if (InputLanguage.InstalledInputLanguages != null) // as is the case on Linux
			{
				foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
				{
					if (l.LayoutName == name)
					{
						return l;
					}
				}
			}
			return null;
		}

		public static List<KeyboardDescriptor> KeyboardDescriptors
		{
			get
			{
				List<KeyboardDescriptor> descriptors = new List<KeyboardDescriptor>();
				try
				{
					foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
					{
						KeyboardDescriptor d = new KeyboardDescriptor(lang.LayoutName, Engines.Windows, lang.Handle.ToInt64().ToString());
						descriptors.Add(d);
					}
				}
				catch (Exception err)
				{
					Debug.Fail(err.Message);
				}
				return descriptors;
			}
		}

		public static bool EngineAvailable
		{
			get {
				return PlatformID.Win32NT == Environment.OSVersion.Platform
					   || PlatformID.Unix == Environment.OSVersion.Platform;
			}
		}

		static  public void Deactivate()
		{
			try
			{
				InputLanguage.CurrentInputLanguage = InputLanguage.DefaultInputLanguage;
			}
			catch (Exception )
			{
				Palaso.Reporting.ProblemNotificationDialog.Show("There was a problem deactivating windows ime.");
			}
		}

		public static KeyboardDescriptor GetActiveKeyboardDescriptor()
		{
			try
			{
				InputLanguage lang = InputLanguage.CurrentInputLanguage;
				if (null == lang)
					return null;
				else
					return new KeyboardDescriptor(lang.LayoutName, Engines.Windows, lang.Handle.ToInt64().ToString());
			}
			catch (Exception)
			{
				Palaso.Reporting.ProblemNotificationDialog.Show(
					"There was a problem retrieving the active keyboard in from windows ime.");
			}
			return null;
		}
	}
}