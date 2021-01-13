using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace SRPG {
  public class T_IsAlive : Conditional {
    private NPC npc;
    public SharedBool isAlive;

    public override void OnStart() {
      base.OnStart();
      npc = gameObject.GetComponent<NPC>();
    }

    public override TaskStatus OnUpdate() {
      if(npc.pawn.state == pS.Dead){
        isAlive.Value = false;
        return TaskStatus.Failure;
      } 
      else {
        isAlive.Value = true;
        return TaskStatus.Success;
      }
    }
  }
}