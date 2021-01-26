using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;

namespace Postcarbon {
  public class Database: NetworkedBehaviour {
    public List<dItem> database;
    public List<dRecipe> recipes;
    
    public dItem GetItem(string name) {
      dItem dItem = null;
      for (int i = 0; i < database.Count; i++) {
        if (database[i].name == name) {
          dItem = database[i];
          break;
        }
      }
      return dItem;
    }
  }
}
