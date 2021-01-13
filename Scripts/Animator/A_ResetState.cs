using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class A_ResetState : StateMachineBehaviour {
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      animator.SetInteger("State", 0);
    }
  }
}
