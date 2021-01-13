using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace SRPG {
  public class T_Attack : Action {
    public SharedTransform enemy;
    private NPC npc;

    public override void OnAwake() {
      npc = gameObject.GetComponent<NPC>();
    }

    public override TaskStatus OnUpdate() {
      if(npc.pawn.state == pS.Idle) {
        if(Vector3.Distance(transform.position,enemy.Value.position) < 1) {
          npc.pawn.Attack();
        }
        else { return TaskStatus.Success; }
      }
      return TaskStatus.Running;
    }
  }
}