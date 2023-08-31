using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CVRLocalTools.Animators {

	/// <summary>
	/// This type wraps around an <see cref="AnimatorControllerParameter"/> and makes the parameter
	/// behave like a distinctive object in that it provides setter methods to modify the animator it is a part of.
	/// </summary>
	public class MutableAnimatorParameter {

		/// <summary>
		/// Whether or not the animator exists and can be used.
		/// </summary>
		public bool IsValid => Animator;

		/// <summary>
		/// The animator itself.
		/// </summary>
		public Animator Animator { get; }

		/// <summary>
		/// The name of the parameter
		/// </summary>
		[Obsolete("The ID should be preferred over the name.")]
		public string Name { get; }

		/// <summary>
		/// The ID of the parameter.
		/// </summary>
		public int ID { get; }

		/// <summary>
		/// True if this parameter is local, based on its name.
		/// </summary>
		public bool IsLocalParameter { get; }

#pragma warning disable CS0618 // Obsolescence
		public MutableAnimatorParameter(Animator animator, string name, int id) {
			Animator = animator;
			Name = name;
			ID = id;
			IsLocalParameter = name.StartsWith("#");
		}
#pragma warning restore CS0618

		/// <summary>
		/// Attempts to get the ID of a named animator parameter.
		/// </summary>
		/// <param name="animator">The animator to search.</param>
		/// <param name="name">The name of the parameter to get the ID of.</param>
		/// <param name="id">The resulting ID, or <see langword="default"/> if the ID is invalid.</param>
		/// <returns><see langword="true"/> if the parameter and ID were both found, <see langword="false"/> if not.</returns>
		public static bool TryGetIDFromName(Animator animator, string name, out int id) {
			int @params = animator.parameterCount;
			for (int index = 0; index < @params; index++) {
				AnimatorControllerParameter param = animator.GetParameter(index);
				if (param.name == name) {
					id = param.nameHash;
					return true;
				}
			}
			id = 0;
			return false;
		}

		public virtual void Set(bool value) {
			Animator.SetBool(ID, value);
		}

		public virtual void Set(float value) {
			Animator.SetFloat(ID, value);
		}

		public virtual void Set(int value) {
			Animator.SetInteger(ID, value);
		}

		public virtual void SetTrigger() {
			Animator.SetTrigger(ID);
		}

		public virtual void ResetTrigger() {
			Animator.ResetTrigger(ID);
		}

	}
}
