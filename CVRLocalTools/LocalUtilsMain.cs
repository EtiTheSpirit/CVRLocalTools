using System;
using System.Collections.Generic;
using System.Reflection;
using ABI_RC.Core;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Util.AssetFiltering;
using ABI_RC.Systems.MovementSystem;
using CVRLocalTools.Animators;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CVRLocalTools {
	public class LocalUtilsMain : MelonMod {

		public const string MODNAME = "Local Parameter Extender";

		internal static MelonLogger.Instance _log;

		public override void OnInitializeMelon() {
			PrefsAndTools.InitializePrefs();
			_log = LoggerInstance;

			MethodInfo orgCalibrate = typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.CalibrateAvatar), BindingFlags.Public | BindingFlags.Instance);
			MethodInfo orgCleanAvy = typeof(AssetFilter).GetMethod(nameof(AssetFilter.FilterAvatar), BindingFlags.Public | BindingFlags.Static);

			MethodInfo postCalibrateMtd = typeof(LocalUtilsMain).GetMethod(nameof(AfterCalibrateAvatar), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo postCleanAvy = typeof(LocalUtilsMain).GetMethod(nameof(PostCleanRemoteAvatar), BindingFlags.NonPublic | BindingFlags.Static);

			if (orgCalibrate == null) throw new ArgumentNullException($"CVRLocalToolsMod_MissingGameHookedMtd :: Failed to find {nameof(PlayerSetup.CalibrateAvatar)} method. {MODNAME} Mod will fail to function.");
			if (postCalibrateMtd == null) throw new ArgumentNullException($"CVRLocalToolsMod_MissingModPostfixMtd :: Failed to find {nameof(AfterCalibrateAvatar)} method. {MODNAME} Mod will fail to function.");
			if (orgCleanAvy == null) throw new ArgumentNullException($"CVRLocalToolsMod_MissingGameHookedMtd :: Failed to find {nameof(AssetFilter.FilterAvatar)} method. {MODNAME} Mod will fail to function.");
			if (postCleanAvy == null) throw new ArgumentNullException($"CVRLocalToolsMod_MissingModPostfixMtd :: Failed to find {nameof(PostCleanRemoteAvatar)} method. {MODNAME} Mod will fail to function.");

			// Would be nice if this had something like BepInEx with the On namespace
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
			GameObject targetObjectForMarshaller = avatar; // avatar.transform.parent.parent.gameObject;
			if (targetObjectForMarshaller == null) {
				_log.Error($"CVRLocalToolsMod_MissingRemoteAvatar :: The avatar of a remote player was destroyed or missing. {MODNAME} will not properly manage their nonreplicated parameters, if they have any.");
				return;
			}

			AnimatorParameterMarshaller existing = targetObjectForMarshaller.GetComponent<AnimatorParameterMarshaller>();
			if (existing != null) {
				UnityObject.Destroy(existing);
			}

			AnimatorParameterMarshaller marshaller = targetObjectForMarshaller.AddComponent<AnimatorParameterMarshaller>();
			Animator animator = avatar.GetComponent<Animator>();
			if (animator == null) {
				_log.Error($"CVRLocalToolsMod_MissingRemoteAnimator :: Failed to find an Animator on another player's avatar. {MODNAME} will not properly manage their nonreplicated parameters, if they have any.");
				return;
			}
			marshaller.Initialize(animator, false);
		}

		internal static void AfterCalibrateAvatar(PlayerSetup __instance) {
			GameObject targetObjectForMarshaller = __instance._avatar;// __instance.gameObject;
			if (targetObjectForMarshaller == null) {
				// If this condition is met, either something injected before me, or.. well, I'm not sure. It should have errored before this.
				_log.Error($"CVRLocalToolsMod_MissingLocalAvatar :: After calibration completed, your local avatar reference was destroyed or missing.");
				return;
			}
			AnimatorParameterMarshaller existing = targetObjectForMarshaller.GetComponent<AnimatorParameterMarshaller>();
			if (existing != null) {
				UnityObject.Destroy(existing);
			}

			AnimatorParameterMarshaller marshaller = targetObjectForMarshaller.AddComponent<AnimatorParameterMarshaller>();
			Animator animator = __instance._animator;
			if (animator == null) {
				_log.Error($"CVRLocalToolsMod_MissingLocalAnimator :: Your current loaded avatar has no animator. Thus, {MODNAME} cannot manage animator parameters (for obvious reasons).");
				return;
			}
			marshaller.Initialize(__instance._animator, true);
		}

	}
}
