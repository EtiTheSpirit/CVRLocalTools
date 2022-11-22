using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CVRLocalTools {
	internal static class PrefsAndTools {

		private const string YELLOW = "\u001b[93m";
		private const string CYAN = "\u001b[96m";
		private const string GREEN = "\u001b[92m";
		private const string UNDERLINE_ON = "\u001b[4m";
		private const string UNDERLINE_OFF = "\u001b[24m";
		
		private static MelonPreferences_Category _settingsCategory { get; set; }

		/// <summary>
		/// If true, <see cref="AssertParameterIsLocal(Animator, string)"/> will raise a warning as intended. If false, it will do nothing.
		/// </summary>
		internal static bool WarnForFaults => _settingsCategory.GetEntry<bool>("WarnForNetworkedLocal").Value;

		internal static void InitializePrefs() {
			_settingsCategory = MelonPreferences.CreateCategory("Local Data Utilities");
			_settingsCategory.CreateEntry("WarnForNetworkedLocal", false, "Warn if using networked parameters", "If true, raise a warning in the console if the avatar declares any parameter managed by this mod without a leading #. If disabled, the mod will just do nothing without saying why it's doing nothing.");
			_settingsCategory.SaveToFile();
		}

		/// <summary>
		/// This method serves multiple purposes. Its primary purpose is to acquire a reference to a <em>local</em> parameter with the given <paramref name="name"/>.
		/// It serves the additional function of warning the user if a global parameter is present (rather than a local), to prevent confusion.<para/>
		/// If the former exists but not the latter, this raises a warning to remind the user that the mod will not affect it.<br/>
		/// If both exist, this raises a warning to remind the user that only the local (#) parameter will be affected.
		/// </summary>
		/// <param name="name">The global name of the parameter. The # is automatically prepended and should not be defined by the developer.</param>
		/// <param name="localParamIfPresent">If present, this is a reference to the local parameter.</param>
		/// <exception cref="ArgumentException">If <paramref name="name"/> includes a leading #.</exception>
		/// <returns>True if everything is OK, false if a warning was raised.</returns>
		internal static bool AssertParameterIsLocal(this Animator animator, string name) {
			if (animator == null) throw new ArgumentNullException(nameof(animator));
			if (name == null) throw new ArgumentNullException(nameof(animator));
			if (name.StartsWith("#")) throw new ArgumentException($"The name input into {nameof(AssertParameterIsLocal)} starts with #. It should not start with #.", nameof(name));

			string localName = "#" + name;
			if (!WarnForFaults) {
				return true;
			}

			bool hasLocal = false;
			bool hasGlobal = false;
			foreach (AnimatorControllerParameter parameter in animator.parameters) {
				if (parameter.name == name) {
					hasGlobal = true;
				} else if (parameter.name == localName) {
					hasLocal = true;
				}
			}
			if (hasGlobal && !hasLocal) {
				LocalUtilsMain._log.Warning($"{YELLOW}The avatar you just loaded defines a parameter named {CYAN}{name}{YELLOW} (when it should instead be {UNDERLINE_ON}{GREEN}#{UNDERLINE_OFF}{CYAN}{name}{YELLOW}). Without the leading {GREEN}#{YELLOW}, the value would be sent across the network, which you don't want. This parameter will {UNDERLINE_ON}not{UNDERLINE_OFF} be changed by this mod. You can turn off this warning in your mod settings.");
				return false;
			} else if (hasGlobal && hasLocal) {
				LocalUtilsMain._log.Warning($"{YELLOW}The avatar you just loaded defines both a parameter named {CYAN}{name}{YELLOW} {UNDERLINE_ON}and{UNDERLINE_OFF} a parameter named {GREEN}#{CYAN}{name}{YELLOW}). The mod will {UNDERLINE_ON}only{UNDERLINE_OFF} affect {GREEN}#{CYAN}{name}{YELLOW}. The networked one will not be affected.");
				return false;
			}
			return true;
		}

	}
}
