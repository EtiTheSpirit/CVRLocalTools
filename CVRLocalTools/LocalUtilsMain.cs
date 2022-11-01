using System;
using System.Linq;
using System.Reflection;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CVRLocalTools {
	public class LocalUtilsMain : MelonMod {

		private static MelonLogger.Instance _LOG;

		public override void OnInitializeMelon() {
			Prefs.InitializePrefs();
			_LOG = LoggerInstance;
			MethodInfo org = typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.CalibrateAvatar), BindingFlags.Public | BindingFlags.Instance);
			MethodInfo postMtd = typeof(LocalUtilsMain).GetMethod(nameof(AfterCalibrateAvatar), BindingFlags.Public | BindingFlags.Static);

			if (org == null) throw new ArgumentNullException($"Failed to find {nameof(PlayerSetup.CalibrateAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");
			if (postMtd == null) throw new ArgumentNullException($"Failed to find {nameof(AfterCalibrateAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");

			HarmonyInstance.Patch(
				original: org,
				postfix: new HarmonyMethod(postMtd)
			);
		}

		public static void AfterCalibrateAvatar(PlayerSetup __instance) {
			if (Prefs.WarnForFaults) {
				AnimatorControllerParameter local = __instance._animator.parameters.FirstOrDefault(param => param.name == "#IsLocal");
				if (local == null) {
					local = __instance._animator.parameters.FirstOrDefault(param => param.name == "IsLocal");
					if (local != null) {
						if (ViewManager.Instance != null) {
							ViewManager.Instance.TriggerPushNotification("This avatar might not be made correctly (it has a replicated IsLocal parameter). Check the ML console for more information. You can turn this warning off in your ML preferences.", 8f);
						}
						_LOG.Warning("\u001b[93mThe avatar you just loaded defines a parameter named \u001b[96mIsLocal\u001b[93m (when it should instead be \u001b[4m\u001b[92m#\u001b[96m\u001b[24mIsLocal\u001b[93m). Without the leading #, the value would be sent across the network, which you don't want. This parameter will \u001b[4mnot\u001b[24m be changed by this mod. You can turn off this warning in your mod settings.");
					}
				}
			}

			// This actually works for all four types!
			// For boolean, it sets to true. For int and float it sets to 1. For trigger, it sets the trigger.
			__instance.changeAnimatorParam("#IsLocal", 1);
		}

	}
}
