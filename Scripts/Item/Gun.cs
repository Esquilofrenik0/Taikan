using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.NetworkedVar;
using System.IO;

namespace Postcarbon {
  [System.Serializable]
  public class Gun: Weapon {
    public GameObject fx;
    public AudioSource audioSource;
    [HideInInspector] public dGun dGun;

    public override void SetData(dItem data) {
      base.SetData(data);
      dGun = data as dGun;
    }

    void Awake() {
      dWeapon = dItem as dWeapon;
      dGun = dItem as dGun;
    }
  }
}
