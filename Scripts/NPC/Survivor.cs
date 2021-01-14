using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class Survivor: NPC {
    [Header("Human")]
    public Human human;
    public dItem[] equippedItems;

    public override void NetworkStart() {
      base.NetworkStart();
      human.initRagdoll();
      for (int i = 0; i < equippedItems.Length; i++) {
        if (equippedItems[i] != null) {
          human.equipment.EquipItem(equippedItems[i]);
        }
      }
    }

    void FixedUpdate() {
      if (!IsServer) { return; }
      if (human.state == pS.Dead) {
        behaviorTree.DisableBehavior();
        agent.isStopped = true;
      }
      else {
        behaviorTree.EnableBehavior();
        human.RefreshState();
      }
    }

    void Update() {
      if (!IsServer) { return; }
      human.anim.SetFloat("Vertical", agent.desiredVelocity.magnitude);
    }
  }
}
