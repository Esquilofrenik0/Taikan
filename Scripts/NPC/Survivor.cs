using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class Survivor: NPC {
    [Header("Human")]
    public dItem[] equippedItems;

    public override void NetworkStart() {
      base.NetworkStart();
      pawn.spawnPoint = transform.position;
      pawn.initRagdoll();
      pawn.Respawn();
      for (int i = 0; i < equippedItems.Length; i++) {
        if (equippedItems[i] != null) {
          pawn.equipment.EquipItem(equippedItems[i]);
        }
      }
    }

    void FixedUpdate() {
      if (!IsServer) { return; }
      if (pawn.anim.GetInteger("State") != (int)pawn.state) { pawn.SetState((pS)(pawn.anim.GetInteger("State"))); }
      pawn.SetAnimatorLayer();
      pawn.ResetWeaponTrace();
    }

    void Update() {
      if (!IsServer) { return; }
      if (pawn.state == pS.Dead) { agent.isStopped = true; return; }
      if (CanSeeEnemy()) { EngageEnemy(5); }
      else {
        if (patrolling) { MoveToPoint(patrolPoint, 2); }
        else { SetPatrolPoint(); }
      }
      pawn.anim.SetFloat("Vertical", agent.desiredVelocity.magnitude);
    }
  }
}
