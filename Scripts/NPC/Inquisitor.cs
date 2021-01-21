using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SRPG {
  public class Inquisitor: NPC {
    public float patrolSpeed;
    public float combatSpeed;
    
    public override void NetworkStart() {
      if (!IsServer) { return; }
      pawn.spawnPoint = GetNavPoint(transform.position);
      pawn.initRagdoll();
      pawn.Respawn();
    }

    void FixedUpdate() {
      if (!IsServer) { return; }
      if (pawn.state == (int)pS.Dead) { agent.isStopped = true; return; }
      if (CanSeeEnemy()) { EngageEnemy(combatSpeed); }
      else {
        if (patrolling) { MoveToPoint(patrolPoint, patrolSpeed); }
        else { SetPatrolPoint(); }
      }
      pawn.anim.SetFloat("Vertical", agent.desiredVelocity.magnitude);
    }

    void LateUpdate() {
      if (!IsServer || pawn.state == (int)pS.Dead) { return; }
      LookAtEnemy();
    }
  }
}
