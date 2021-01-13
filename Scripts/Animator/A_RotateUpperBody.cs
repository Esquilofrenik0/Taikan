using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class A_RotateUpperBody : StateMachineBehaviour {
    public Vector3 hipsRotation;

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Transform tHips = animator.GetBoneTransform(HumanBodyBones.Spine);
      animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(tHips.localRotation.x + hipsRotation.x, tHips.localRotation.y + hipsRotation.y, tHips.localRotation.z + hipsRotation.z));
    }
  }
}