using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_Punch: StateMachineBehaviour {
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.PunchActive(true);
      pawn.state.Value = (int)pS.Attack;
      pawn.resetAttack = true;
      pawn.attacking = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.PunchActive(false);
      pawn.state.Value = 0;
      pawn.attacking = false;
    }
  }
}
