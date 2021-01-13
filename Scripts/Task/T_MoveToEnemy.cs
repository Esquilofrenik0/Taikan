using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace SRPG {
  public class T_MoveToEnemy : Action {
    public float speed = 5;
    public SharedTransform enemy;
    private NPC npc;
    private Vector3 stopPosition;

    public override void OnStart() {
      npc = gameObject.GetComponent<NPC>();
      npc.agent.SetDestination(enemy.Value.position);
      npc.agent.speed = speed;
    }

    public override TaskStatus OnUpdate() {
      if (npc.agent.remainingDistance < 1) {
        npc.agent.isStopped = true;
        npc.agent.velocity = Vector3.zero;
        return TaskStatus.Success;
      }
      npc.agent.isStopped = false;
      npc.agent.SetDestination(enemy.Value.position);
      return TaskStatus.Running;
    }
  }
}