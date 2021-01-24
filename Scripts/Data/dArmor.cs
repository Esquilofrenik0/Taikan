using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Armor", menuName = "SRPG/Item/Armor")]
  [System.Serializable]
  public class dArmor : dItem {
    public aT aType = aT.Light;
    public aS armorSlot = aS.Helmet;
    public float defense = 1;
    public float durability = 100;

    public void Awake() {
      this.type = iT.Armor;
      this.stack = 1;
    }
  }
}