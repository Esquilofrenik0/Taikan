using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_Melee: StateMachineBehaviour {
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.SetState((int)pS.Attack);
      pawn.resetAttack = true;
      pawn.attacking = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.SetState(0);
      // pawn.resetAttack = true;
      pawn.attacking = false;
    }
  }
}
