using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class A_RotateUpperChest: StateMachineBehaviour {
    public Vector3 toRotate = new Vector3(0,0,0);

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Transform bone = animator.GetBoneTransform(HumanBodyBones.Chest);
      animator.SetBoneLocalRotation(HumanBodyBones.Chest, Quaternion.Euler(bone.localRotation.x + toRotate.x, bone.localRotation.y + toRotate.y, bone.localRotation.z + toRotate.z));
    }
  }
}