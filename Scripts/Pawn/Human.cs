using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;


namespace Postcarbon {
  public class Human: Humanoid {
    public override void Respawn() {
      base.Respawn();
      if (avatar.activeRace.name == "HumanMaleDCS" || avatar.activeRace.name == "HumanMale") {
        avatar.GetDNA()["height"].Set(0.4f);
        avatar.SetSlot("Underwear", "MaleUnderwear");
      }
      else if (avatar.activeRace.name == "HumanFemaleDCS" || avatar.activeRace.name == "HumanFemale") {
        avatar.GetDNA()["height"].Set(0.6f);
        avatar.SetSlot("Underwear", "FemaleUndies");
      }
    }
  }
}
