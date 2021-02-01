using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class A_RotateShoulders: StateMachineBehaviour {
    public Vector3 rsRotation = new Vector3(-180,0,-90);
    public Vector3 lsRotation = new Vector3(0,180,-90);

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Transform rs = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
      animator.SetBoneLocalRotation(HumanBodyBones.RightShoulder, Quaternion.Euler(rs.localRotation.x + rsRotation.x, rs.localRotation.y + rsRotation.y, rs.localRotation.z + rsRotation.z));
      Transform ls = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
      animator.SetBoneLocalRotation(HumanBodyBones.LeftShoulder, Quaternion.Euler(ls.localRotation.x + lsRotation.x, ls.localRotation.y + lsRotation.y, ls.localRotation.z + lsRotation.z));
    }
  }
}