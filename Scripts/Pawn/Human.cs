using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;


namespace Postcarbon {
  public class Human: Humanoid {
    public override void Respawn() {
      if (GetComponent<Human>()) {
        if (GetComponent<RandomUMA>()) { GetComponent<RandomUMA>().Randomize(avatar); }
        else { RandomGender(); }
        recipeAvatar.Value = avatar.GetCurrentRecipe();
      }
      base.Respawn();
    }
  }
}
