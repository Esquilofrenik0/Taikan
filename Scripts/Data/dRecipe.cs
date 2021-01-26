using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Recipe", menuName = "SRPG/Item/Recipe")]
  [System.Serializable]
  public class dRecipe : dItem {
    public Slot result;
    public List<Slot> cost;

    public void Awake() {
      this.type = iT.Recipe;
    }
  }
}