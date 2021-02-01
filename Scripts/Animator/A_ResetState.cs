using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_ResetState: StateMachineBehaviour {
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      animator.GetComponent<Pawn>().SetState(0);
    }
  }
}
