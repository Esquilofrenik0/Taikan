using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_Shoot: StateMachineBehaviour {
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Pawn pawn = animator.GetComponent<Pawn>();
      if (pawn.equipment.weaponSlot[0].GetChild(0).GetComponent<Weapon>()) {
        Weapon gun = pawn.equipment.weaponSlot[0].GetChild(0).GetComponent<Weapon>();
        if (gun.fx != null) { gun.fx.SetActive(false); }
      }
    }
  }
}
