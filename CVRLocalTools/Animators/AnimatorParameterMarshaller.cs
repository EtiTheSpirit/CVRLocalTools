using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.InputManagement;
using ABI_RC.Systems.MovementSystem;
using RTG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CVRLocalTools.Animators {

	public class AnimatorParameterMarshaller : MonoBehaviour {

		// private PlayerSetup _setupLocal;
		private bool _desiredLocality = false;
		private Camera _mainCamera = null;
		private float _lastCheckedCameraSince = float.PositiveInfinity;
		private bool _hasWarnedForMissingInputMgr = false;

		// This stuff would be way better with #nullable
		// All MutableAnimatorParameters are nullable.
		private MutableAnimatorParameter _isLocal;

		private MutableAnimatorParameter _velocityX;
		private MutableAnimatorParameter _velocityY;
		private MutableAnimatorParameter _velocityZ;

		private MutableAnimatorParameter _localVelocityX;
		private MutableAnimatorParameter _localVelocityY;
		private MutableAnimatorParameter _localVelocityZ;

		private MutableAnimatorParameter _rotVelocityX;
		private MutableAnimatorParameter _rotVelocityY;
		private MutableAnimatorParameter _rotVelocityZ;

		private MutableAnimatorParameter _positionX;
		private MutableAnimatorParameter _positionY;
		private MutableAnimatorParameter _positionZ;

		private MutableAnimatorParameter _rotationX;
		private MutableAnimatorParameter _rotationY;
		private MutableAnimatorParameter _rotationZ;

		private MutableAnimatorParameter _localRotationX;
		private MutableAnimatorParameter _localRotationZ;

		private MutableAnimatorParameter _upright;

		private MutableAnimatorParameter _lookX;
		private MutableAnimatorParameter _lookY;
		private MutableAnimatorParameter _lookZ;

		private MutableAnimatorParameter _fingerTrackingEnabled;

		private Vector3 _lastPosition;
		private Vector3 _lastRotation;

		/// <summary>
		/// Creates a new <see cref="MutableAnimatorParameter"/> with the provided name, granted the provided <paramref name="animator"/> has such a parameter. Returns <see langword="null"/> if no such parameter exists.
		/// </summary>
		/// <param name="animator">The animator that contains this parameter.</param>
		/// <param name="name">The name of the parameter. This must not begin with '#'.</param>
		/// <param name="allowReplicated">If true, the replicated parameter is searched for first (with no leading '#'). Iff that is not found, its local counterpart <c>#<paramref name="name"/></c> is searched for.</param>
		/// <param name="disallowLocal">If true, the fallback search (see info on the <paramref name="allowReplicated"/> parameter) is skipped.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">If the name starts with a hashtag, or if an impossible search scenario (replicated disallowed while local is also disallowed) is encountered.</exception>
		private MutableAnimatorParameter GetParameter(Animator animator, string name, ref UserFacingErrorFlags errors, bool allowReplicated = false, bool disallowLocal = false) {
			if (name.StartsWith("#")) {
				throw new ArgumentException("The name must not begin with '#' in code.", nameof(name));
			}
			if (!allowReplicated && disallowLocal) {
				throw new ArgumentException("Attempt to disallow both a replicated and a local parameter with this name.", $"{nameof(allowReplicated)}, {nameof(disallowLocal)}");
			}

			string localName = '#' + name;
			bool hasReplicatedParam = MutableAnimatorParameter.TryGetIDFromName(animator, name, out int replicatedParamID);
			bool hasLocalParam = MutableAnimatorParameter.TryGetIDFromName(animator, localName, out int localParamID);

			if (allowReplicated) {
				// A parameter named "name" is allowed...
				if (hasReplicatedParam) {
					// ... And exists, return it.
					// (But first, check this)
					if (hasLocalParam) {
						if (PrefsAndTools.WarnForLocalAndGlobal) {
							errors |= UserFacingErrorFlags.BothReplicatedAndLocal;
							PrefsAndTools.WarnForLocalAndReplicatedParameter(name);
						}
					}
					return new MutableAnimatorParameter(animator, name, replicatedParamID);
				} else if (!disallowLocal && hasLocalParam) {
					// ... But does not exist, though "#name" is also allowed and exists. Return that instead.
					return new MutableAnimatorParameter(animator, localName, localParamID);
				} else {
					// ... But either no local counterpart "#name" is allowed, or it was but was not found. Return nothing.
					return null;
				}
			} else {
				// A parameter named "name" is NOT allowed, only "#name"...
				if (hasReplicatedParam) {
					if (hasLocalParam) {
						if (PrefsAndTools.WarnForLocalAndGlobal) {
							errors |= UserFacingErrorFlags.BothReplicatedAndLocal;
							PrefsAndTools.WarnForLocalAndReplicatedParameter(name);
						}
					} else {
						if (PrefsAndTools.VerifyProperLocality) {
							errors |= UserFacingErrorFlags.ReplicatedInPlaceOfLocal;
							PrefsAndTools.WarnForReplicatedInPlaceOfLocal(name);
						}
					}
				}
				if (hasLocalParam) {
					return new MutableAnimatorParameter(animator, localName, localParamID);
				}
			}
			
			return null;
		}

		private void LateInitWithAnimator(Animator animator) {
			if (animator == null) throw new ArgumentNullException(nameof(animator));

			UserFacingErrorFlags errors = UserFacingErrorFlags.NoError;

			if (_desiredLocality) {
				LocalUtilsMain._log.Msg("Checking for Local Parameter Extender parameters...");
				LocalUtilsMain._log.WriteLine();
			}
			_isLocal = GetParameter(animator, "IsLocal", ref errors, false, false);

			//bool canDriveParameterNow = _desiredLocality || PrefsAndTools.DriveRemoteParameters;
			const bool canDriveParameterNow = true; // Don't check this here.
			// I still want to instantiate the parameter so that the config option can be changed
			// *without* requiring the remote player to reload their avatar.

			_velocityX = GetParameter(animator, "VelocityX", ref errors, canDriveParameterNow);
			_velocityY = GetParameter(animator, "VelocityY", ref errors, canDriveParameterNow);
			_velocityZ = GetParameter(animator, "VelocityZ", ref errors, canDriveParameterNow);

			_localVelocityX = GetParameter(animator, "RelativeVelocityX", ref errors, canDriveParameterNow);
			_localVelocityY = GetParameter(animator, "RelativeVelocityY", ref errors, canDriveParameterNow);
			_localVelocityZ = GetParameter(animator, "RelativeVelocityZ", ref errors, canDriveParameterNow);

			_rotVelocityX = GetParameter(animator, "RotVelocityX", ref errors, canDriveParameterNow);
			_rotVelocityY = GetParameter(animator, "RotVelocityY", ref errors, canDriveParameterNow);
			_rotVelocityZ = GetParameter(animator, "RotVelocityZ", ref errors, canDriveParameterNow);

			_positionX = GetParameter(animator, "PositionX", ref errors, canDriveParameterNow);
			_positionY = GetParameter(animator, "PositionY", ref errors, canDriveParameterNow);
			_positionZ = GetParameter(animator, "PositionZ", ref errors, canDriveParameterNow);

			_rotationX = GetParameter(animator, "RotationX", ref errors, canDriveParameterNow);
			_rotationY = GetParameter(animator, "RotationY", ref errors, canDriveParameterNow);
			_rotationZ = GetParameter(animator, "RotationZ", ref errors, canDriveParameterNow);

			_localRotationX = GetParameter(animator, "RelativePitch", ref errors, canDriveParameterNow);
			_localRotationZ = GetParameter(animator, "RelativeRoll", ref errors, canDriveParameterNow);

			_upright = GetParameter(animator, "Upright", ref errors, canDriveParameterNow);

			// Local-Only Parameters:
			_lookX = GetParameter(animator, "LookX", ref errors, _desiredLocality);
			_lookY = GetParameter(animator, "LookY", ref errors, _desiredLocality);
			_lookZ = GetParameter(animator, "LookZ", ref errors, _desiredLocality);
			_fingerTrackingEnabled = GetParameter(animator, "FingerTracking", ref errors, _desiredLocality);
			// Above: Use _desiredLocality instead of canDriveParameterNow. The remote player should never control the value.

			if (ViewManager.Instance != null && errors != UserFacingErrorFlags.NoError && _desiredLocality) {
				LocalUtilsMain._log.Error("CVRLocalToolsMod_MalformedParametersOnLocal :: There were issues with your avatar. This log entry will be picked up by the Mod Log Scanner.");
				ViewManager.Instance.TriggerPushNotification("Local Parameter Extender: Detected that your animator might have incorrect parameters. Check the log for more info.", 8f);
			}

			SetIfPresent(_isLocal, _desiredLocality);

			if (_desiredLocality) {
				LocalUtilsMain._log.WriteLine();
				LocalUtilsMain._log.Msg("Parameter updater ready for this avatar.");
			}

			_lastPosition = gameObject.transform.position;
			_lastRotation = gameObject.transform.eulerAngles;
		}

		/// <summary>
		/// Initialize the marshaller using the given animator and desired locality.
		/// </summary>
		/// <param name="animator">The animator to wrap around.</param>
		/// <param name="isLocal">Whether or not the avatar that the given animator is a part of represents the local player's avatar.</param>
		/// <exception cref="ArgumentNullException">If the input animator is null.</exception>
		public void Initialize(Animator animator, bool isLocal) {
			if (animator == null) throw new ArgumentNullException(nameof(animator));
			_desiredLocality = isLocal;
			LateInitWithAnimator(animator);
		}

		void FixedUpdate() {
			Transform myTransform = gameObject.transform;
			float fdt = Time.fixedDeltaTime;
			float invFdt = 1f / fdt;

			Vector3 position = myTransform.position;
			Vector3 rotation = myTransform.eulerAngles;
			Vector3 velocity = (position - _lastPosition) * invFdt;
			Vector3 rotationalVelocity = (rotation - _lastRotation) * invFdt;
			WrapVector3Angles(ref rotationalVelocity); // TODO: This creates a large, backwards delta when wrapping angles i.e. 359 => 1
													   // TODO: Why does the rigidbody not have its values set?

			bool canDriveParameterNow = _desiredLocality || PrefsAndTools.DriveRemoteParameters;
#region Replicated Values
			if (canDriveParameterNow) {
				// World values:
				SetVector3(_positionX, _positionY, _positionZ, position);
				SetVector3(_rotationX, _rotationY, _rotationZ, rotation);
				SetVector3(_velocityX, _velocityY, _velocityZ, velocity);
				SetVector3(_rotVelocityX, _rotVelocityY, _rotVelocityZ, rotationalVelocity);
				SetIfPresent(_upright, myTransform.up.Dot(Vector3.up));

				// Relative values:
				Vector3 localVelocity = myTransform.TransformVector(velocity);
				SetVector3(_localVelocityX, _localVelocityY, _localVelocityZ, localVelocity);
				SetIfPresent(_localRotationX, Pitch(myTransform));
				SetIfPresent(_localRotationZ, Roll(myTransform));
			}
#endregion

#region Client-Only Values
			// Client-only values: 
			if (_desiredLocality) {
				// Camera getter:
				_lastCheckedCameraSince += fdt;
				if (_lastCheckedCameraSince > 1) {
					_mainCamera = Camera.main;
					// According to Unity, referencing this repeatedly (i.e. every frame) is not a good idea, so I have this delay cycle to ensure the reference
					// is correct mostly all the time while caching what I can.
					_lastCheckedCameraSince = 0;
				}
				
				if (_mainCamera != null) {
					Transform trs = _mainCamera.gameObject.transform;
					SetVector3(_lookX, _lookY, _lookZ, trs.forward);
				}

				if (CVRInputManager.Instance != null) {
					if (_hasWarnedForMissingInputMgr) {
						LocalUtilsMain._log.Warning($"The input manager has been found after it was missing before. {_fingerTrackingEnabled.Name} will be updated properly.");
						_hasWarnedForMissingInputMgr = false;
					}
					SetIfPresent(_fingerTrackingEnabled, CVRInputManager.Instance.individualFingerTracking);
				} else {
					if (!_hasWarnedForMissingInputMgr) {
						LocalUtilsMain._log.Warning($"The input manager is missing! {_fingerTrackingEnabled.Name} will be set to false until it is present.");
						_hasWarnedForMissingInputMgr = true;
					}
					SetIfPresent(_fingerTrackingEnabled, false);
				}
			}
#endregion

			_lastPosition = position;
			_lastRotation = rotation;
		}

		private void SetIfPresent(MutableAnimatorParameter param, float value) {
			if (param.IsValid()) {
				param.Set(value);
			}
		}

		private void SetIfPresent(MutableAnimatorParameter param, bool value) {
			if (param.IsValid()) {
				param.Set(value);
			}
		}

		/// <summary>
		/// Sets one or more of an X, Y and Z parameter representing components of a <see cref="Vector3"/>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="vector"></param>
		private void SetVector3(MutableAnimatorParameter x, MutableAnimatorParameter y, MutableAnimatorParameter z, in Vector3 vector) {
			SetIfPresent(x, vector.x);
			SetIfPresent(y, vector.y);
			SetIfPresent(z, vector.z);
		}
		
		/// <summary>
		/// Wraps a <see cref="Vector3"/> within a range of -360 to +360.
		/// </summary>
		/// <param name="vec"></param>
		private void WrapVector3Angles(ref Vector3 vec) {
			if (vec.x > 360) vec.x -= 360;
			if (vec.y > 360) vec.y -= 360;
			if (vec.z > 360) vec.z -= 360;
			if (vec.x < -360) vec.x += 360;
			if (vec.y < -360) vec.y += 360;
			if (vec.z < -360) vec.z += 360;
		}

		// Pitch and Roll methods acquired via https://answers.unity.com/questions/1366142/get-pitch-and-roll-values-from-object.html

		private float Pitch(Transform trs) {
			Vector3 right = trs.right * Mathf.Sign(trs.up.y);
			right.y = 0;
			Vector3 fwd = Vector3.Cross(right, Vector3.up).normalized;
			return Vector3.Angle(fwd, trs.forward) * Mathf.Sign(trs.forward.y);
		}

		private float Roll(Transform trs) {
			Vector3 fwd = trs.forward * Mathf.Sign(trs.up.y);
			fwd.y = 0;
			Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
			return Vector3.Angle(right, trs.right) * Mathf.Sign(trs.right.y);
		}

		[Flags]
		private enum UserFacingErrorFlags {
			NoError,
			ReplicatedInPlaceOfLocal,
			BothReplicatedAndLocal
		}
	}
}
