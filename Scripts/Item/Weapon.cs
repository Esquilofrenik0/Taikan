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
    
    public void Start() {
      if (GetComponentInParent<Pawn>()) {
        pawn = GetComponentInParent<Pawn>();
        if (GetComponent<Collider>()) { Physics.IgnoreCollision(GetComponent<Collider>(), pawn.col); }
        gameObject.layer = 2;
      }
    }
  }
}