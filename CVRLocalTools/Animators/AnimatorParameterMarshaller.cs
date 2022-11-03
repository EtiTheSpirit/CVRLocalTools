using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using RTG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace CVRLocalTools.Animators {

	public class AnimatorParameterMarshaller : MonoBehaviour {

		// private Rigidbody _rigidBody;
		private PlayerSetup _setupLocal;
		private MutableAnimatorParameter _isLocal;
		private bool _desiredLocality = false;


		private MutableAnimatorParameter _velocityX;
		private MutableAnimatorParameter _velocityY;
		private MutableAnimatorParameter _velocityZ;

		private MutableAnimatorParameter _rotVelocityX;
		private MutableAnimatorParameter _rotVelocityY;
		private MutableAnimatorParameter _rotVelocityZ;

		private MutableAnimatorParameter _positionX;
		private MutableAnimatorParameter _positionY;
		private MutableAnimatorParameter _positionZ;

		private MutableAnimatorParameter _rotationX;
		private MutableAnimatorParameter _rotationY;
		private MutableAnimatorParameter _rotationZ;
		private bool _displayWarning = false;

		private Vector3 _lastPosition;
		private Vector3 _lastRotation;

		private MutableAnimatorParameter GetParameter(Animator animator, string name) {
			_displayWarning = _displayWarning || !PrefsAndTools.AssertParameterIsLocal(animator, name);
			return new MutableAnimatorParameter(animator, "#" + name);
		}
		private void LateInitWithAnimator(Animator animator) {
			_isLocal = GetParameter(animator, "IsLocal");

			_velocityX = GetParameter(animator, "VelocityX");
			_velocityY = GetParameter(animator, "VelocityY");
			_velocityZ = GetParameter(animator, "VelocityZ");

			_rotVelocityX = GetParameter(animator, "RotVelocityX");
			_rotVelocityY = GetParameter(animator, "RotVelocityY");
			_rotVelocityZ = GetParameter(animator, "RotVelocityZ");

			_positionX = GetParameter(animator, "PositionX");
			_positionY = GetParameter(animator, "PositionY");
			_positionZ = GetParameter(animator, "PositionZ");

			_rotationX = GetParameter(animator, "RotationX");
			_rotationY = GetParameter(animator, "RotationY");
			_rotationZ = GetParameter(animator, "RotationZ");

			if (_displayWarning && ViewManager.Instance != null) {
				ViewManager.Instance.TriggerPushNotification("CVRLocalTools detected what might be a mistake with this avatar's parameters. Check the ML console for more information. You can turn this warning off in your ML preferences.", 8f);
			}

			SetIfPresent(_isLocal, _desiredLocality);
			LocalUtilsMain._log.Msg("Parameter updater ready for this avatar.");

			_lastPosition = gameObject.transform.position;
			_lastRotation = gameObject.transform.eulerAngles;
		}

		public void Initialize(Animator animator, bool isLocal) {
			if (animator == null) throw new MissingComponentException("Failed to find an animator.");
			_desiredLocality = isLocal;
			LateInitWithAnimator(animator);
		}

		void FixedUpdate() {
			Vector3 position = gameObject.transform.position;
			Vector3 rotation = gameObject.transform.eulerAngles;
			Vector3 velocity = (position - _lastPosition) / Time.fixedDeltaTime;
			Vector3 rotationalVelocity = (rotation - _lastRotation) / Time.fixedDeltaTime;
			WrapVector3(ref rotationalVelocity);

			SetVector3(_positionX, _positionY, _positionZ, ref position);
			SetVector3(_rotationX, _rotationY, _rotationZ, ref rotation);
			SetVector3(_velocityX, _velocityY, _velocityZ, ref velocity);
			SetVector3(_rotVelocityX, _rotVelocityY, _rotVelocityZ, ref rotationalVelocity);
			_lastPosition = position;
			_lastRotation = rotation;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetIfPresent(MutableAnimatorParameter param, float value) {
			if (param?.IsValid ?? false) {
				/*
				if (_setupLocal) {
					_setupLocal.changeAnimatorParam(param.Name, value);
				} else {
					param.Set(value);
				}
				*/
				param.Set(value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetIfPresent(MutableAnimatorParameter param, bool value) {
			if (param?.IsValid ?? false) {
				/*
				if (_setupLocal) {
					_setupLocal.changeAnimatorParam(param.Name, value ? 1f : 0f);
				} else {
					param.Set(value);
				}
				*/
				param.Set(value);
			}
		}

		private void SetVector3(MutableAnimatorParameter x, MutableAnimatorParameter y, MutableAnimatorParameter z, ref Vector3 vector) {
			SetIfPresent(x, vector.x);
			SetIfPresent(y, vector.y);
			SetIfPresent(z, vector.z);
		}

		private void WrapVector3(ref Vector3 vec) {
			if (vec.x > 360) vec.x -= 360;
			if (vec.y > 360) vec.y -= 360;
			if (vec.z > 360) vec.z -= 360;
			if (vec.x < -360) vec.x += 360;
			if (vec.y < -360) vec.y += 360;
			if (vec.z < -360) vec.z += 360;
		}
	}
}
