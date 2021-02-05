using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.NetworkedVar;
using System.IO;

namespace Postcarbon {
  [System.Serializable]
  public class Weapon: Item {
    [HideInInspector] public dWeapon dWeapon;
    [HideInInspector] public Pawn pawn;
    [HideInInspector] public ulong ownerID = 0;
    [HideInInspector] public NetworkedVar<ulong> owner = new NetworkedVar<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);

    public override void NetworkStart() {
      base.NetworkStart();
      if (GetComponent<NetworkedObject>() && GetComponent<NetworkedObject>().IsSceneObject == true) {
        GetComponent<Collider>().isTrigger = false;
        return;
      }
      // if (IsServer) { owner.Value = ownerID; }
      owner.Value = ownerID;
      if (owner.Value != 0) { pawn = GetNetworkedObject(owner.Value).GetComponent<Pawn>(); }
      else if (GetComponentInParent<Pawn>()) {
        pawn = GetComponentInParent<Pawn>();
        owner.Value = pawn.NetworkId;
      }
      if (GetComponent<Collider>() && pawn) { Physics.IgnoreCollision(GetComponent<Collider>(), pawn.col); }
      gameObject.layer = 2;
    }
  }
}