using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace SRPG {
  public class T_SetPatrolPoint : Conditional {
    public SharedVector3 patrolPoint;
    private NPC npc;

    public override void OnStart() {
      base.OnStart();
      npc = gameObject.GetComponent<NPC>();
    }

    public override TaskStatus OnUpdate() {
      float x = npc.spawnPos.x + Random.Range(-npc.patrolDistance,npc.patrolDistance);
      float y = npc.transform.position.y;
      float z = npc.spawnPos.z + Random.Range(-npc.patrolDistance,npc.patrolDistance);
      patrolPoint.Value = new Vector3(x,y,z);
      return TaskStatus.Success;
    }
  }
}