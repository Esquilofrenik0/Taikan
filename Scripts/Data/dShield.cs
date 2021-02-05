using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Shield", menuName = "SRPG/Weapon/Shield")]
  [System.Serializable]
  public class dShield: dWeapon {
    public float defense = 1;
  }
}