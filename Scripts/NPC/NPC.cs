using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;

namespace SRPG {
  public class NPC: NetworkedBehaviour {
    [Header("Components")]
    public Pawn pawn;
    public NavMeshAgent agent;
    [Header("AI")]
    public float FoV = 90;
    public float viewDistance = 10;
    public int patrolDistance = 30;
    [HideInInspector] public bool patrolling = false;
    [HideInInspector] public Transform enemy;
    [HideInInspector] public Vector3 patrolPoint;
    [HideInInspector] public RaycastHit[] possibleTargets;

    public void SetPatrolPoint() {
      float x = pawn.spawnPoint.x + Random.Range(-patrolDistance, patrolDistance);
      float y = transform.position.y;
      float z = pawn.spawnPoint.z + Random.Range(-patrolDistance, patrolDistance);
      patrolPoint = new Vector3(x, y, z);
      patrolling = true;
    }

    public void MoveToPoint(Vector3 destination, float speed) {
      agent.isStopped = false;
      agent.speed = speed;
      agent.SetDestination(destination);
      if (agent.remainingDistance < 5) {
        patrolling = false;
        SetPatrolPoint();
      }
    }

    public bool WithinSight(Transform targetTransform,float fieldOfViewAngle) {
      Vector3 direction = targetTransform.position - transform.position;
      if(Vector3.Distance(targetTransform.position,transform.position) < viewDistance) {
        return Vector3.Angle(direction,transform.forward) < fieldOfViewAngle;
      }
      else { return false; }
    }

    public bool CanSeeEnemy(){
      possibleTargets = Physics.SphereCastAll(transform.position,50,Vector3.forward,50);
      for(int i = 0;i < possibleTargets.Length;++i) {
        if(possibleTargets[i].transform.GetComponent<Pawn>() != null) {
          Pawn target = possibleTargets[i].transform.GetComponent<Pawn>();
          if(pawn.faction != target.faction) {
            if(target.state != pS.Dead) {
              if(WithinSight(target.transform,FoV)) {
                enemy = target.transform;
                return true;
              }
            }
          }
        }
      }
      enemy = null;
      return false;
    }

    public void EngageEnemy(int speed){
      agent.isStopped = false;
      agent.speed = speed;
      agent.SetDestination(enemy.position);
      MeleeAttack();
    }

    public void MeleeAttack(){
      if(agent.remainingDistance < 1){
        pawn.Attack();
      }
    }

  }
}
