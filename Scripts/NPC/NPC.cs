using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;

namespace Postcarbon {
  public class NPC: NetworkedBehaviour {
    [Header("Components")]
    public Pawn pawn;
    public NavMeshAgent agent;
    [Header("AI")]
    public float patrolSpeed = 2;
    public float combatSpeed = 5;
    public int patrolDistance = 30;
    [HideInInspector] public List<Pawn> enemy;
    [HideInInspector] public bool enemyLost = false;
    [HideInInspector] public Vector3 patrolPoint;
    [HideInInspector] public bool patrolling = false;

    public Vector3 GetNavPoint(Vector3 point) {
      Vector3 navPoint = point;
      NavMeshHit myNavHit;
      if (NavMesh.SamplePosition(point, out myNavHit, 100, -1)) {
        navPoint = myNavHit.position;
      }
      return navPoint;
    }

    public void SetPatrolPoint() {
      float x = pawn.spawnPoint.x + Random.Range(-patrolDistance, patrolDistance);
      float y = transform.position.y;
      float z = pawn.spawnPoint.z + Random.Range(-patrolDistance, patrolDistance);
      patrolPoint = new Vector3(x, y, z);
      patrolPoint = GetNavPoint(patrolPoint);
      patrolling = true;
    }

    public void MoveToPoint(Vector3 destination, float speed) {
      agent.isStopped = false;
      agent.speed = speed;
      agent.SetDestination(destination);
      if (agent.remainingDistance < 2) { patrolling = false; }
    }

    public void EngageEnemy(float speed) {
      if (enemy[0] && enemy[0].state.Value != (int)pS.Dead) {
        agent.isStopped = false;
        agent.speed = speed;
        agent.SetDestination(enemy[0].transform.position);
        if (pawn.equipment.weapon[0] is dGun) { RangedAttack(); }
        else { MeleeAttack(); }
      }
      else { enemy.Remove(enemy[0]); }
    }

    public void RangedAttack() {
      if (agent.remainingDistance < 20) {
        transform.LookAt(enemy[0].transform);
        pawn.Attack();
        if (agent.remainingDistance < 5) { agent.isStopped = true; }
      }
    }

    public void MeleeAttack() {
      if (agent.remainingDistance < 5) {
        transform.LookAt(enemy[0].transform);
        pawn.Attack();
        if (agent.remainingDistance < 1) { agent.isStopped = true; }
      }
    }

    public void LookAtEnemy() {
      if (enemy[0]) {
        Vector3 toLook = enemy[0].GetComponent<Collider>().bounds.center;
        pawn.spine.transform.LookAt(toLook, Vector3.right);
        float rx = 0;
        float ry = pawn.spine.transform.localEulerAngles.y;
        float rz = 0;
        pawn.spine.transform.localEulerAngles = new Vector3(rx, ry, rz);
      }
    }
  }
}
