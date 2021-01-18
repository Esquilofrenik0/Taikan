using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace SRPG {
  public class A_Melee: StateMachineBehaviour {
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.SetState((int)pS.Attack);
      pawn.attacking = true;
      pawn.resetAttack = true;
    }

    // public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //   if (stateInfo.normalizedTime > 0.1 && stateInfo.normalizedTime < 0.9) { pawn.attacking = true; }
    //   else { pawn.attacking = false; }
    // }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.SetState(0);
      pawn.attacking = false;
      pawn.resetAttack = true;
    }
  }
}
