using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace SRPG {
  public class Survivor: NPC {
    [Header("Human")]
    public Human human;
    public dItem[] equippedItems;

    public override void NetworkStart() {
      // if (!IsLocalPlayer) { return; }
      base.NetworkStart();
      human.initRagdoll();
      for (int i = 0; i < equippedItems.Length; i++) {
        if (equippedItems[i] != null) {
          human.equipment.EquipItem(equippedItems[i]);
        }
      }
    }

    void FixedUpdate() {
      // if (!IsLocalPlayer) { return; }
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
      // if (!IsLocalPlayer) { return; }
      human.anim.SetFloat("Vertical", agent.desiredVelocity.magnitude);
    }
  }
}
