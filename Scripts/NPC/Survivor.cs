using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class Survivor: NPC {
    public override void NetworkStart() {
      base.NetworkStart();
      pawn.spawnPoint = transform.position;
      pawn.initRagdoll();
      pawn.Respawn();
    }

    void Update() {
      if (!IsServer) { return; }
      if (pawn.state == (int)pS.Dead) { agent.isStopped = true; return; }
      if (CanSeeEnemy()) { EngageEnemy(5); }
      else {
        if (patrolling) { MoveToPoint(patrolPoint, 2); }
        else { SetPatrolPoint(); }
      }
      pawn.anim.SetFloat("Vertical", agent.desiredVelocity.magnitude);
    }

    void LateUpdate() {
      if (!IsServer) { return; }
      LookAtEnemy();
    }
  }
}
