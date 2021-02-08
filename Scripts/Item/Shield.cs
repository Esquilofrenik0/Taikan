using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.NetworkedVar;
using System.IO;

namespace Postcarbon {
  [System.Serializable]
  public class Shield: Weapon {
    [HideInInspector] public dShield dShield;
    
    public override void SetData(dItem data) {
      base.SetData(data);
      dShield = data as dShield;
    }
    void Awake() { 
      dWeapon = dItem as dWeapon;
      dShield = dItem as dShield; 
    }
  }
}