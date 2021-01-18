using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace SRPG {
  public class A_ResetState: StateMachineBehaviour {
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      animator.GetComponent<Pawn>().SetState(0);
    }
  }
}
