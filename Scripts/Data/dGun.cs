using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Ranged", menuName = "SRPG/Weapon/Gun")]
  [System.Serializable]
  public class dGun: dWeapon {
    public int clipSize = 0;
    public AudioClip[] audioClip;
  }
}