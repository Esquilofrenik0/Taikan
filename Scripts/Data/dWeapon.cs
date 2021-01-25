using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Weapon", menuName = "SRPG/Item/Weapon")]
  [System.Serializable]
  public class dWeapon: dItem {
    public GameObject resource;
    public wT wType = wT.Unarmed;
    public wS weaponSlot = wS.RightHand;
    public float damage = 1;
    public float defense = 0;
    public float durability = 100;
    public bool isRanged = false;
    public AudioClip[] audioClip;

    public void Awake() {
      this.type = iT.Weapon;
      this.stack = 1;
    }
  }
}