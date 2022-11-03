using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CVRLocalTools.Animators {
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
		public string Name { get; }

		public MutableAnimatorParameter(Animator animator, string name) {
			Animator = animator;
			Name = name;
		}

		public void Set(bool value) {
			Animator.SetBool(Name, value);
		}

		public void Set(float value) {
			Animator.SetFloat(Name, value);
		}
		
		public void Set(int value) {
			Animator.SetInteger(Name, value);
		}

		public void SetTrigger() {
			Animator.SetTrigger(Name);
		}

		public void ResetTrigger() {
			Animator.ResetTrigger(Name);
		}
	}
}
