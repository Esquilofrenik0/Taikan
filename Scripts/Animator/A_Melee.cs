using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace SRPG {
  public class A_Melee: StateMachineBehaviour {
    Pawn pawn;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      pawn = animator.GetComponent<Pawn>();
      pawn.SetState((int)pS.Attack);
      pawn.resetAttack = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Debug.Log(stateInfo.normalizedTime);
      if (stateInfo.normalizedTime > 0.1 && stateInfo.normalizedTime < 0.9) { pawn.attacking = true; }
      else { pawn.attacking = false; }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      pawn.SetState(0);
      pawn.attacking = false;
      pawn.resetAttack = true;
    }
  }
}
