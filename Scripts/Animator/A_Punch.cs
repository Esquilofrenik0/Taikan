using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_Punch: StateMachineBehaviour {
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.PunchActive(true);
      pawn.SetState((int)pS.Attack);
      pawn.resetAttack = true;
      pawn.attacking = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Pawn pawn = animator.GetComponent<Pawn>();
      pawn.PunchActive(false);
      pawn.SetState(0);
      pawn.attacking = false;
    }
  }
}
