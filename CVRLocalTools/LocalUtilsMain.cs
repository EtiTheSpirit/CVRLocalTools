using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ABI_RC.Core;
using ABI_RC.Core.EventSystem;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using CVRLocalTools.Animators;
using HarmonyLib;
using MelonLoader;
using MelonLoader.ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace CVRLocalTools {
	public class LocalUtilsMain : MelonMod {

		internal static MelonLogger.Instance _log;
		private static AnimatorControllerParameter _velocityX;
		private static AnimatorControllerParameter _velocityY;
		private static AnimatorControllerParameter _velocityZ;
		private static AnimatorControllerParameter _positionX;
		private static AnimatorControllerParameter _positionY;
		private static AnimatorControllerParameter _positionZ;
		private static AnimatorControllerParameter _rotationX;
		private static AnimatorControllerParameter _rotationY;
		private static AnimatorControllerParameter _rotationZ;
		private static Animator _animator;
		private static List<Animator> _remoteAnimators = new List<Animator>();
		private static Transform _animatorTrs;
		private static MovementSystem _movementSystem;
		private static bool _isReady = false;

		public override void OnApplicationStart() {
			PrefsAndTools.InitializePrefs();
		}

		public override void OnInitializeMelon() {
			_log = LoggerInstance;

			MethodInfo orgCalibrate = typeof(PlayerSetup).GetMethod(nameof(PlayerSetup.CalibrateAvatar), BindingFlags.Public | BindingFlags.Instance);
			MethodInfo orgCleanAvy = typeof(CVRTools).GetMethod(nameof(CVRTools.CleanAvatarGameObjectNetwork), BindingFlags.Public | BindingFlags.Static);

			MethodInfo postCalibrateMtd = typeof(LocalUtilsMain).GetMethod(nameof(AfterCalibrateAvatar), BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo postCleanAvy = typeof(LocalUtilsMain).GetMethod(nameof(PostCleanRemoteAvatar), BindingFlags.NonPublic | BindingFlags.Static);

			if (orgCalibrate == null) throw new ArgumentNullException($"Failed to find {nameof(PlayerSetup.CalibrateAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");
			if (postCalibrateMtd == null) throw new ArgumentNullException($"Failed to find {nameof(AfterCalibrateAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");
			if (orgCleanAvy == null) throw new ArgumentNullException($"Failed to find {nameof(CVRTools.CleanAvatarGameObjectNetwork)} method. {nameof(CVRLocalTools)} Mod will fail to function.");
			if (postCleanAvy == null) throw new ArgumentNullException($"Failed to find {nameof(PostCleanRemoteAvatar)} method. {nameof(CVRLocalTools)} Mod will fail to function.");

			HarmonyInstance.Patch(
				original: orgCalibrate,
				postfix: new HarmonyMethod(postCalibrateMtd)
			);
			HarmonyInstance.Patch(
				original: orgCleanAvy,
				postfix: new HarmonyMethod(postCleanAvy)
			);
		}

		internal static void PostCleanRemoteAvatar(GameObject avatar, bool isFriend, AssetManagement.AvatarTags tags, bool forceShow, bool forceBlock) {
			AnimatorParameterMarshaller existing = avatar.transform.parent.parent.GetComponent<AnimatorParameterMarshaller>();
			if (existing != null) {
				GameObject.Destroy(existing);
			}

			AnimatorParameterMarshaller marshaller = avatar.transform.parent.parent.gameObject.AddComponent<AnimatorParameterMarshaller>();
			Animator animator = avatar.GetComponent<Animator>();
			if (animator == null) throw new MissingComponentException("Failed to find an Animator on another player's avatar.");
			marshaller.Initialize(animator, false);
		}

		internal static void AfterCalibrateAvatar(PlayerSetup __instance) {
			AnimatorParameterMarshaller marshaller = __instance.gameObject.AddComponent<AnimatorParameterMarshaller>();
			Animator animator = __instance._animator;
			if (animator == null) throw new MissingComponentException("Failed to find an Animator on another player's avatar.");
			marshaller.Initialize(__instance._animator, true);
		}

	}
}
