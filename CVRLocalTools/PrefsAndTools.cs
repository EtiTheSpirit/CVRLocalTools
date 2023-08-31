using CVRLocalTools.Animators;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
		/// If true, the system will raise a warning if it finds a parameter reserved by CVRLocalTools (i.e. <c>IsLocal</c>) declared as a replicated parameter instead of a clientside parameter (i.e. <c>#IsLocal</c>).
		/// </summary>
		internal static bool VerifyProperLocality => _verifyLocality.Value;

		/// <summary>
		/// If true, the system will raise a warning if it finds two parameters, one networked and one non-networked.
		/// </summary>
		internal static bool WarnForLocalAndGlobal => _warnForLocalAndGlobal.Value;

		/// <summary>
		/// If true, the system will manage parameters from this mod even if they are not #local.
		/// </summary>
		internal static bool DriveRemoteParameters => _driveRemoteParameters.Value;

		private static MelonPreferences_Entry<bool> _verifyLocality;
		private static MelonPreferences_Entry<bool> _warnForLocalAndGlobal;
		private static MelonPreferences_Entry<bool> _driveRemoteParameters;

		internal static void InitializePrefs() {
			_settingsCategory = MelonPreferences.CreateCategory("Local Parameter Extender");
			_verifyLocality = _settingsCategory.CreateEntry("WarnForNetworkedLocal", false, "Verify Locality", "If true, raise a warning in the console if the avatar declares any parameter managed by this mod without a leading #. If disabled, the mod will just do nothing without saying why it's doing nothing.");
			_warnForLocalAndGlobal = _settingsCategory.CreateEntry("WarnForLocalAndGlobal", false, "Warn For Duplicate Local/Networked Parameters", $"If true, and if {_verifyLocality.DisplayName} is true, the system will also raise a notice in the console when it finds both \"Param\" *and* \"#Param\" on an avatar (avatar authors doing this is anticipated to be intentional, but if accidental it can be confusing).");
			_driveRemoteParameters = _settingsCategory.CreateEntry("DriveRemoteParameters", false, "Smooth Others' Parameters", "If true, this mod's parameters that belong to other players (i.e. someone else's VelocityY parameter) will be managed by your client, granted they are not #local. This can be used to enforce a \"what you see\" behavior; the value you get in their animator is reflective of what you see their avatar doing, rather than what they say their avatar is doing over the network. This can reduce latency but will also create a difference between in what you both see.");
			_settingsCategory.SaveToFile();
		}

		internal static void WarnForLocalAndReplicatedParameter(string replicatedName) {
			LocalUtilsMain._log.Warning($"CVRLocalTools_LocalAndReplicatedParam :: {YELLOW}The avatar you just loaded defines both a parameter named {CYAN}{replicatedName}{YELLOW} {UNDERLINE_ON}and{UNDERLINE_OFF} a parameter named {GREEN}#{CYAN}{replicatedName}{YELLOW}). The mod will {UNDERLINE_ON}only{UNDERLINE_OFF} affect {CYAN}{replicatedName}{YELLOW}, and {GREEN}#{CYAN}{replicatedName}{YELLOW} will be ignored!");
		}

		internal static void WarnForReplicatedInPlaceOfLocal(string replicatedName) {
			LocalUtilsMain._log.Warning($"CVRLocalTools_ReplicatedNeedsToBeLocal :: {YELLOW}The avatar you just loaded defines a parameter named {CYAN}{replicatedName}{YELLOW} (when it should instead be {UNDERLINE_ON}{GREEN}#{UNDERLINE_OFF}{CYAN}{replicatedName}{YELLOW}). Without the leading {GREEN}#{YELLOW}, the value would be sent across the network, which you don't want. This parameter will {UNDERLINE_ON}not{UNDERLINE_OFF} be changed by this mod. You can turn off this warning in your mod settings.");
		}

		/// <summary>
		/// This extension method allows checking <see cref="MutableAnimatorParameter.IsValid"/> on a potentially <see langword="null"/> reference.
		/// </summary>
		/// <param name="nullableParameter"></param>
		/// <returns></returns>
		internal static bool IsValid(this MutableAnimatorParameter nullableParameter) {
			return nullableParameter != null && nullableParameter.IsValid;
		}
	}
}
