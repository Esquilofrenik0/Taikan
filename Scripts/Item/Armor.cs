using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [System.Serializable]
  public class Armor: Item {
    [HideInInspector] public dArmor dArmor;

    public override void SetData(dItem data) {
      base.SetData(data);
      dArmor = data as dArmor;
    }

    void Awake() {
      dArmor = dItem as dArmor;
    }
  }
}