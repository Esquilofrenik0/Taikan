using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Consumable", menuName = "SRPG/Consumable")]
  [System.Serializable]
  public class dConsumable : dItem {
    public float hRestore = 0;
    public float sRestore = 0;
    public float mRestore = 0;
  }
}