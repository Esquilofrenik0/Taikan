using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace SRPG {
  public class A_Impact: StateMachineBehaviour {
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.SetState((int)pS.Attack);
      pawn.attacking = false;
      pawn.resetAttack = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.SetState(0);
    }
  }
}
