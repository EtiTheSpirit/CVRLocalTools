using System;
using System.Collections.Generic;
using System.Reflection;
using ABI_RC.Core;
using ABI_RC.Core.Player;
using ABI_RC.Core.Util.AssetFiltering;
using ABI_RC.Systems.MovementSystem;
using CVRLocalTools.Animators;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CVRLocalTools {
	public class LocalUtilsMain : MelonMod {

		internal static MelonLogger.Instance _log;

		public override void OnInitializeMelon() {
			PrefsAndTools.InitializePrefs();
			_log = LoggerInstance;

			MethodInfo orgCalibrate = typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.CalibrateAvatar), BindingFlags.Public | BindingFlags.Instance);
			MethodInfo orgCleanAvy = typeof(AssetFilter).GetMethod(nameof(AssetFilter.FilterAvatar), BindingFlags.Public | BindingFlags.Static);

			MethodInfo postCalibrateMtd = typeof(LocalUtilsMain).GetMethod(nameof(AfterCalibrateAvatar), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo postCleanAvy = typeof(LocalUtilsMain).GetMethod(nameof(PostCleanRemoteAvatar), BindingFlags.NonPublic | BindingFlags.Static);

			if (orgCalibrate == null) throw new ArgumentNullException($"Failed to find {nameof(PlayerSetup.CalibrateAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");
			if (postCalibrateMtd == null) throw new ArgumentNullException($"Failed to find {nameof(AfterCalibrateAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");
			if (orgCleanAvy == null) throw new ArgumentNullException($"Failed to find {nameof(AssetFilter.FilterAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");
			if (postCleanAvy == null) throw new ArgumentNullException($"Failed to find {nameof(PostCleanRemoteAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");

			// Would be nice if this had something like RoR2 with the On namespace
			// On.ABI_RC.Core.Player.PlayerSetup.CalibrateAvatar += override method here
			HarmonyInstance.Patch(
				original: orgCalibrate,
				postfix: new HarmonyMethod(postCalibrateMtd)
			);
			HarmonyInstance.Patch(
				original: orgCleanAvy,
				postfix: new HarmonyMethod(postCleanAvy)
			);
		}

		internal static void PostCleanRemoteAvatar(GameObject avatar) {
			AnimatorParameterMarshaller existing = avatar.transform.parent.parent.GetComponent<AnimatorParameterMarshaller>();
			if (existing != null) {
				GameObject.Destroy(existing);
			}

			AnimatorParameterMarshaller marshaller = avatar.transform.parent.parent.gameObject.AddComponent<AnimatorParameterMarshaller>();
			Animator animator = avatar.GetComponent<Animator>();
			if (animator == null) {
				_log.Error("Failed to find an Animator on another player's avatar. CVRLocalTools will not properly manage their nonreplicated parameters, if they have any.");
				return;
			}
			marshaller.Initialize(animator, false);
		}

		internal static void AfterCalibrateAvatar(PlayerSetup __instance) {
			AnimatorParameterMarshaller existing = __instance.gameObject.GetComponent<AnimatorParameterMarshaller>();
			if (existing != null) {
				GameObject.Destroy(existing);
			}

			AnimatorParameterMarshaller marshaller = __instance.gameObject.AddComponent<AnimatorParameterMarshaller>();
			Animator animator = __instance._animator;
			if (animator == null) {
				_log.Error("Your current loaded avatar has no animator. Thus, CVRLocalTools cannot manage animator parameters (for obvious reasons).");
				return;
			}
			marshaller.Initialize(__instance._animator, true);
		}

	}
}
