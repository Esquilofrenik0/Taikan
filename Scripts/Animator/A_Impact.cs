using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_Impact: StateMachineBehaviour {
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      if (pawn.meleeRoutine != null) { pawn.StopCoroutine(pawn.meleeRoutine); }
      pawn.state.Value = (int)pS.Attack;
      pawn.attacking = false;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.state.Value = 0;
    }
  }
}
