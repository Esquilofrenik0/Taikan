using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  [CreateAssetMenu(fileName = "Consumable", menuName = "SRPG/Item/Consumable")]
  [System.Serializable]
  public class dConsumable : dItem {
    public float hRestore = 0;
    public float sRestore = 0;
    public float mRestore = 0;

    public void Awake() {
      this.type = iT.Consumable;
    }
  }
}