using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CVRLocalTools.Animators {

	/// <summary>
	/// A wrapper for a <see cref="AnimatorControllerParameter"/> that provides setters.
	/// </summary>
	public class MutableAnimatorParameter {

		/// <summary>
		/// Whether or not the animator exists and can be used.
		/// </summary>
		public bool IsValid => Animator != null;

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

#pragma warning disable CS0618 // Obsolescence
		public MutableAnimatorParameter(Animator animator, string name, int id) {
			Animator = animator;
			Name = name;
			ID = id;
		}
#pragma warning restore CS0618

		/// <summary>
		/// Attempts to get the ID of a named animator parameter.
		/// </summary>
		/// <param name="animator">The animator to search.</param>
		/// <param name="name">The name of the parameter to get the ID of.</param>
		/// <param name="id">The resulting ID, or -1 if the ID is invalid.</param>
		/// <returns>True if the parameter and ID were both found, false if not.</returns>
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

		public void Set(bool value) {
			Animator.SetBool(ID, value);
		}

		public void Set(float value) {
			Animator.SetFloat(ID, value);
		}
		
		public void Set(int value) {
			Animator.SetInteger(ID, value);
		}

		public void SetTrigger() {
			Animator.SetTrigger(ID);
		}

		public void ResetTrigger() {
			Animator.ResetTrigger(ID);
		}
	}
}
