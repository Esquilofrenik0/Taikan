using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Postcarbon {
  public class Inquisitor: NPC {
    public override void NetworkStart() {
      if (!IsServer) { return; }
      enemy = new List<Pawn>();
      pawn.spawnPoint = GetNavPoint(transform.position);
      pawn.initRagdoll();
      pawn.Respawn();
    }

    void Update() {
      if (!IsServer) { return; }
      if (pawn.state.Value == (int)pS.Dead) { agent.isStopped = true; return; }
      if (enemy.Count == 0) {
        if (patrolling) { MoveToPoint(patrolPoint, patrolSpeed); }
        else { SetPatrolPoint(); }
      }
      else {
        LookAtEnemy();
        EngageEnemy(combatSpeed);
      }
      pawn.anim.SetFloat("Vertical", agent.velocity.magnitude);
    }
  }
}
