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

    void Update() {
      if (!IsServer) { return; }
      if (pawn.state == (int)pS.Dead) { agent.isStopped = true; return; }
      if (!enemy) {
        if (!LookForEnemy()) {
          if (patrolling) { MoveToPoint(patrolPoint, patrolSpeed); }
          else { SetPatrolPoint(); }
        }
      }
      else { EngageEnemy(combatSpeed); }
      pawn.anim.SetFloat("Vertical", agent.desiredVelocity.magnitude);
    }

    void LateUpdate() {
      if (!IsServer || pawn.state == (int)pS.Dead) { return; }
      LookAtEnemy();
    }
  }
}
