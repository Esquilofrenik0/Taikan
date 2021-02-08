using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace Postcarbon {
  public class A_Shoot: StateMachineBehaviour {
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Pawn pawn = animator.GetComponent<Pawn>();
      Gun gun = (Gun)pawn.equipment.weaponObject[0];
      gun.audioSource.PlayOneShot(gun.dGun.audioClip[0]);
      gun.fx.SetActive(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      // if (!animator.GetComponent<Pawn>().IsLocalPlayer) { return; }
      Pawn pawn = animator.GetComponent<Pawn>();
      Gun gun = (Gun)pawn.equipment.weaponObject[0];
      if (gun.fx != null) { gun.fx.SetActive(false); }
    }
  }
}
