using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;

namespace Postcarbon {
  [System.Serializable]
  public class Weapon: Item {
    [HideInInspector] public dWeapon dWeapon;
    [HideInInspector] public Pawn pawn;
    // [HideInInspector] public ulong ownerID = 0;
    // [HideInInspector] public NetworkedVar<ulong> owner = new NetworkedVar<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);

    public void Start() {
      if (GetComponentInParent<Pawn>()) {
        pawn = GetComponentInParent<Pawn>();
        if (GetComponent<Collider>()) { Physics.IgnoreCollision(GetComponent<Collider>(), pawn.col); }
        gameObject.layer = 2;
      }
    }

    // public override void NetworkStart() {
    //   base.NetworkStart();
    //   if (GetComponent<NetworkedObject>()) {
    //     if (GetComponent<NetworkedObject>().IsSceneObject == true) {
    //       GetComponent<Collider>().isTrigger = false;
    //       return;
    //     }
    //   }
    //   if (ownerID != 0) { pawn = GetNetworkedObject(ownerID).GetComponent<Pawn>(); }
    //   else if (GetComponentInParent<Pawn>()) { pawn = GetComponentInParent<Pawn>(); }
    //   if (GetComponent<Collider>() && pawn) { Physics.IgnoreCollision(GetComponent<Collider>(), pawn.col); }
    //   gameObject.layer = 2;
    // }
  }
}