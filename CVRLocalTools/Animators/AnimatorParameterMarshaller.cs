using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
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
using static MelonLoader.MelonLogger;

namespace CVRLocalTools.Animators {

	public class AnimatorParameterMarshaller : MonoBehaviour {

		// private PlayerSetup _setupLocal;
		private bool _desiredLocality = false;
		private Camera _mainCamera = null;
		private float _lastCheckedCameraSince = float.PositiveInfinity;

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

		/*
		private MutableAnimatorParameter _localLookX;
		private MutableAnimatorParameter _localLookY;
		private MutableAnimatorParameter _localLookZ;
		*/

		private Vector3 _lastPosition;
		private Vector3 _lastRotation;
		private readonly List<string> _names = new List<string>();

		private MutableAnimatorParameter GetParameter(Animator animator, string name) {
			bool isPresent = MutableAnimatorParameter.TryGetIDFromName(animator, "#" + name, out int id);
			if (isPresent) {
				_names.Add(name);
				return new MutableAnimatorParameter(animator, "#" + name, id);
			}
			return null;
		}

		private void LateInitWithAnimator(Animator animator) {
			if (animator == null) throw new ArgumentNullException(nameof(animator));
			_isLocal = GetParameter(animator, "IsLocal");

			_velocityX = GetParameter(animator, "VelocityX");
			_velocityY = GetParameter(animator, "VelocityY");
			_velocityZ = GetParameter(animator, "VelocityZ");

			_localVelocityX = GetParameter(animator, "RelativeVelocityX");
			_localVelocityY = GetParameter(animator, "RelativeVelocityY");
			_localVelocityZ = GetParameter(animator, "RelativeVelocityZ");

			_rotVelocityX = GetParameter(animator, "RotVelocityX");
			_rotVelocityY = GetParameter(animator, "RotVelocityY");
			_rotVelocityZ = GetParameter(animator, "RotVelocityZ");

			_positionX = GetParameter(animator, "PositionX");
			_positionY = GetParameter(animator, "PositionY");
			_positionZ = GetParameter(animator, "PositionZ");

			_rotationX = GetParameter(animator, "RotationX");
			_rotationY = GetParameter(animator, "RotationY");
			_rotationZ = GetParameter(animator, "RotationZ");

			_localRotationX = GetParameter(animator, "RelativePitch");
			_localRotationZ = GetParameter(animator, "RelativeRoll");

			_upright = GetParameter(animator, "Upright");

			_lookX = GetParameter(animator, "LookX");
			_lookY = GetParameter(animator, "LookY");
			_lookZ = GetParameter(animator, "LookZ");

			/*
			// TODO: This would benefit most from a hip tracker.
			_localLookX = GetParameter(animator, "RelativeLookX");
			_localLookY = GetParameter(animator, "RelativeLookY");
			_localLookZ = GetParameter(animator, "RelativeLookZ");
			*/

			// Now the warning:
			string[] names = _names.ToArray();
			bool ok = PrefsAndTools.AssertParametersAreLocal(animator, names);

#pragma warning disable IDE0031
			if (ViewManager.Instance != null && !ok) {
				ViewManager.Instance.TriggerPushNotification("CVRLocalTools found what might be a mistake with this avatar's parameters (check the log file). You can turn this warning off in your ML preferences.", 8f);
			}
#pragma warning restore IDE0031

			SetIfPresent(_isLocal, _desiredLocality);
			LocalUtilsMain._log.Msg("Parameter updater ready for this avatar.");

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
			}

			_lastPosition = position;
			_lastRotation = rotation;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetIfPresent(MutableAnimatorParameter param, float value) {
			if (param?.IsValid ?? false) {
				param.Set(value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetIfPresent(MutableAnimatorParameter param, bool value) {
			if (param?.IsValid ?? false) {
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
			Vector3 right = trs.right * Mathf.Sign(transform.up.y);
			right.y = 0;
			Vector3 fwd = Vector3.Cross(right, Vector3.up).normalized;
			return Vector3.Angle(fwd, transform.forward) * Mathf.Sign(transform.forward.y);
		}

		private float Roll(Transform trs) {
			Vector3 fwd = transform.forward * Mathf.Sign(transform.up.y);
			fwd.y = 0;
			Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
			return Vector3.Angle(right, transform.right) * Mathf.Sign(transform.right.y);
		}
	}
}
