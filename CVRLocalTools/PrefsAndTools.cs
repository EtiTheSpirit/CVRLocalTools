using MelonLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace CVRLocalTools {
	internal static class PrefsAndTools {

		private const string YELLOW = "\u001b[93m";
		private const string CYAN = "\u001b[96m";
		private const string GREEN = "\u001b[92m";
		private const string UNDERLINE_ON = "\u001b[4m";
		private const string UNDERLINE_OFF = "\u001b[24m";

		private static MelonPreferences_Category _settingsCategory;

		/// <summary>
		/// If true, <see cref="AssertParameterIsLocal(Animator, string)"/> will raise a warning if it finds a parameter reserved by CVRLocalTools (i.e. <c>IsLocal</c>) declared as a replicated parameter instead of a clientside parameter.
		/// </summary>
		internal static bool VerifyParameterNames => _verifyParameterNames.Value;

		/// <summary>
		/// If true, <see cref="AssertParameterIsLocal(Animator, string)"/> will raise a warning if it finds two parameters, one networked and one non-networked.
		/// </summary>
		internal static bool WarnForLocalAndGlobal => _warnForLocalAndGlobal.Value;

		private static MelonPreferences_Entry<bool> _verifyParameterNames;
		private static MelonPreferences_Entry<bool> _warnForLocalAndGlobal;

		internal static void InitializePrefs() {
			_settingsCategory = MelonPreferences.CreateCategory("Local Data Utilities");
			_verifyParameterNames = _settingsCategory.CreateEntry("WarnForNetworkedLocal", false, "Verify Parameter Names", "If true, raise a warning in the console if the avatar declares any parameter managed by this mod without a leading #. If disabled, the mod will just do nothing without saying why it's doing nothing.");
			_warnForLocalAndGlobal = _settingsCategory.CreateEntry("WarnForLocalAndGlobal", false, "Verbose Verification", $"If true, and if {_verifyParameterNames.DisplayName} is true, the system will also raise a notice in the console when it finds both \"Param\" *and* \"#Param\" on an avatar (avatar authors doing this is anticipated to be intentional, but if accidental it can be confusing).");
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
		[Obsolete("This is too expensive for repeated calls.", true)]
		internal static bool AssertParameterIsLocal(this Animator animator, string name) {
			if (animator == null) throw new ArgumentNullException(nameof(animator));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (name.StartsWith("#")) throw new ArgumentException($"The name input into {nameof(AssertParameterIsLocal)} starts with #. It should not start with #.", nameof(name));

			string localName = "#" + name;
			if (!VerifyParameterNames) {
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

		/// <summary>
		/// This method verifies that every name in the given array is associated with a local parameter in ChilloutVR. It has the additional duty
		/// of warning the user if a global parameter was defined in its place, or if both a local and a global parameter were defined.
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="names"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		internal static bool AssertParametersAreLocal(Animator animator, string[] names) {
			if (animator == null) throw new ArgumentNullException(nameof(animator));
			if (names == null) throw new ArgumentNullException(nameof(names));
			if (!VerifyParameterNames) return true;

			bool ok = true;
			IReadOnlyDictionary<string, AnimatorControllerParameter> nameLookup = animator.GetParameters();
			for (int index = 0; index < names.Length; index++) {
				string replicatedName = names[index];
				if (replicatedName.Length > 0 && replicatedName[0] == '#') throw new ArgumentException($"A name input into {nameof(AssertParametersAreLocal)}, {replicatedName}, starts with #. It should not start with #.", nameof(names));

				string localName = "#" + replicatedName;
				bool hasGlobal = nameLookup.ContainsKey(replicatedName);
				bool hasLocal = nameLookup.ContainsKey(localName);
				if (hasGlobal && !hasLocal) {
					LocalUtilsMain._log.Warning($"{YELLOW}The avatar you just loaded defines a parameter named {CYAN}{replicatedName}{YELLOW} (when it should instead be {UNDERLINE_ON}{GREEN}#{UNDERLINE_OFF}{CYAN}{replicatedName}{YELLOW}). Without the leading {GREEN}#{YELLOW}, the value would be sent across the network, which you don't want. This parameter will {UNDERLINE_ON}not{UNDERLINE_OFF} be changed by this mod. You can turn off this warning in your mod settings.");
					ok = false;
				} else if (hasGlobal && hasLocal) {
					if (WarnForLocalAndGlobal) {
						LocalUtilsMain._log.Warning($"{YELLOW}The avatar you just loaded defines both a parameter named {CYAN}{replicatedName}{YELLOW} {UNDERLINE_ON}and{UNDERLINE_OFF} a parameter named {GREEN}#{CYAN}{replicatedName}{YELLOW}). The mod will {UNDERLINE_ON}only{UNDERLINE_OFF} affect {GREEN}#{CYAN}{replicatedName}{YELLOW}, and {CYAN}{replicatedName}{YELLOW} will be ignored!");
					}
					ok = false;
				}
			}

			return ok;
		}

		internal static IReadOnlyDictionary<string, AnimatorControllerParameter> GetParameters(this Animator animator) {
			Dictionary<string, AnimatorControllerParameter> dict = new Dictionary<string, AnimatorControllerParameter>();
			int count = animator.parameterCount;
			for (int index = 0; index < count; index++) {
				AnimatorControllerParameter param = animator.GetParameter(index);
				dict[param.name] = param;
			}
			return dict;
		}

	}
}
