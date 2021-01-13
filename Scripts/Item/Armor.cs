using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  [System.Serializable]
  public class Armor : Item {
    [HideInInspector] public dArmor dArmor;

    void Awake() {
      dArmor = dItem as dArmor;
    }
  }
}