using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace SRPG {
  public class T_CanSeeEnemy : Conditional {
    public SharedTransform enemy;
    private NPC npc;
    private RaycastHit[] possibleTargets;


    public override void OnStart() {
      base.OnStart();
      npc = gameObject.GetComponent<NPC>();
    }

    public override TaskStatus OnUpdate() {
      possibleTargets = Physics.SphereCastAll(transform.position,50,Vector3.forward,50);
      for(int i = 0;i < possibleTargets.Length;++i) {
        if(possibleTargets[i].transform.GetComponent<Pawn>() != null) {
          Pawn pawn = possibleTargets[i].transform.GetComponent<Pawn>();
          if(pawn.faction != npc.pawn.faction) {
            if(pawn.state != pS.Dead) {
              if(WithinSight(pawn.transform,npc.FoV)) {
                enemy.Value = possibleTargets[i].transform;
                return TaskStatus.Success;
              }
            }
          }
        }
      }
      enemy.Value = null;
      return TaskStatus.Failure;
    }

    public bool WithinSight(Transform targetTransform,float fieldOfViewAngle) {
      Vector3 direction = targetTransform.position - transform.position;
      if(Vector3.Distance(targetTransform.position,transform.position) < npc.viewDistance) {
        return Vector3.Angle(direction,transform.forward) < fieldOfViewAngle;
      }
      else { return false; }
    }
  }
}