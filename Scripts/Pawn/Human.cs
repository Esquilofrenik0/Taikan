using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class Human: Humanoid {
    public override void Respawn() {
      base.Respawn();
      if (avatar.activeRace.name == "HumanMaleDCS") {
        avatar.GetDNA()["height"].Set(0.4f);
        avatar.SetSlot("Underwear", "MaleUnderwear");
      }
      else if (avatar.activeRace.name == "HumanFemaleDCS") {
        avatar.GetDNA()["height"].Set(0.6f);
        avatar.SetSlot("Underwear", "FemaleUndies2");
      }
    }
  }
}
