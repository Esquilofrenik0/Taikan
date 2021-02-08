using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [System.Serializable]
  public class dWeapon: dItem {
    public GameObject resource;
    public wT wType = wT.Unarmed;
    public wS weaponSlot = wS.RightHand;
    public float damage = 1;
    public float durability = 100;
  }
}