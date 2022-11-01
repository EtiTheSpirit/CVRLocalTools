using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVRLocalTools {
	internal class Prefs {


		private static MelonPreferences_Category _settingsCategory { get; set; }

		internal static bool WarnForFaults => _settingsCategory.GetEntry<bool>("WarnForNetworkedLocal").Value;

		internal static void InitializePrefs() {
			_settingsCategory = MelonPreferences.CreateCategory("Local Data Utilities");
			_settingsCategory.CreateEntry("WarnForNetworkedLocal", true, "Warn if using IsLocal (instead of #IsLocal)", "If true, raise a warning in the console if the avatar declares IsLocal (instead of #IsLocal).");
			_settingsCategory.SaveToFile();
		}

	}
}
