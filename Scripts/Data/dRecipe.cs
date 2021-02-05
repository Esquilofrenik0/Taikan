using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Recipe", menuName = "SRPG/Recipe")]
  [System.Serializable]
  public class dRecipe : dItem {
    public Slot result;
    public List<Slot> cost;
  }
}