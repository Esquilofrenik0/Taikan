using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_Shoot: StateMachineBehaviour {
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      Pawn pawn = animator.GetComponent<Pawn>();
      // if (pawn.equipment.weapon1.Value != 0) { if pawn.equipment.weapon1.Value.GetComponent<Weapon>().fx != null) { pawn.equipment.weapon1.Value.GetComponent<Weapon>().fx.SetActive(false); } }
    }
  }
}
