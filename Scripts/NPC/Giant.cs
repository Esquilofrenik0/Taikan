using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class Giant: NPC {
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

    void Update() {
      if (!IsServer) { return; }
      if (pawn.state == (int)pS.Dead) { agent.isStopped = true; return; }
      if (CanSeeEnemy()) { EngageEnemy(8); }
      else {
        if (patrolling) { MoveToPoint(patrolPoint, 4); }
        else { SetPatrolPoint(); }
      }
      pawn.anim.SetFloat("Vertical", agent.desiredVelocity.magnitude / 3);
    }

    void LateUpdate() {
      if (!IsServer) { return; }
      if (pawn.state == (int)pS.Dead) { agent.isStopped = true; return; }
      LookAtEnemy();
    }
  }
}
