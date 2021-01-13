using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace SRPG {
  public class T_MoveToPoint : Action {
		public float speed = 2;
		public SharedVector3 patrolPoint;
		private NPC npc;

    public override void OnStart() {
			npc = gameObject.GetComponent<NPC>();
			npc.agent.SetDestination(patrolPoint.Value);
      npc.agent.speed = speed;
    }

    public override TaskStatus OnUpdate() {
			if(npc.agent.remainingDistance < 5){
				return TaskStatus.Failure;
			}
			npc.agent.isStopped = false;
			npc.agent.SetDestination(patrolPoint.Value);
			return TaskStatus.Success;
    }
  }
}