using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

namespace Postcarbon {
  public class RandomUMA: MonoBehaviour {

    public List<UMARandomizer> Randomizers;

    public RandomWardrobeSlot GetRandomWardrobe(List<RandomWardrobeSlot> wardrobeSlots) {
      int total = 0;

      foreach (RandomWardrobeSlot rws in wardrobeSlots)
        total += rws.Chance;

      foreach (RandomWardrobeSlot rws in wardrobeSlots) {
        if (UnityEngine.Random.Range(0, total) < rws.Chance) {
          return rws;
        }
      }
      return wardrobeSlots[wardrobeSlots.Count - 1];
    }

    private OverlayColorData GetRandomColor(RandomColors rc) {
      int inx = UnityEngine.Random.Range(0, rc.ColorTable.colors.Length);
      return rc.ColorTable.colors[inx];
    }

    private void AddRandomSlot(DynamicCharacterAvatar Avatar, RandomWardrobeSlot uwr) {
      Avatar.SetSlot(uwr.WardrobeSlot);
      if (uwr.Colors != null) {
        foreach (RandomColors rc in uwr.Colors) {
          if (rc.ColorTable != null) {
            OverlayColorData ocd = GetRandomColor(rc);
            Avatar.SetColor(rc.ColorName, ocd, false);
          }
        }
      }
    }

    public void Randomize(DynamicCharacterAvatar Avatar) {
      Avatar.WardrobeRecipes.Clear();
      UMARandomizer Randomizer = null;
      if (Randomizers != null) {
        if (Randomizers.Count == 0) { return; }
        if (Randomizers.Count == 1) { Randomizer = Randomizers[0]; }
        else { Randomizer = Randomizers[UnityEngine.Random.Range(0, Randomizers.Count)]; }
      }
      if (Avatar != null && Randomizer != null) {
        RandomAvatar ra = Randomizer.GetRandomAvatar();
        Avatar.ChangeRaceData(ra.RaceName);
        // Avatar.BuildCharacterEnabled = true;
        var RandomDNA = ra.GetRandomDNA();
        Avatar.predefinedDNA = RandomDNA;
        var RandomSlots = ra.GetRandomSlots();

        if (ra.SharedColors != null && ra.SharedColors.Count > 0) {
          foreach (RandomColors rc in ra.SharedColors) {
            if (rc.ColorTable != null) { Avatar.SetColor(rc.ColorName, GetRandomColor(rc), false); }
          }
        }
        foreach (string s in RandomSlots.Keys) {
          List<RandomWardrobeSlot> RandomWardrobe = RandomSlots[s];
          RandomWardrobeSlot uwr = GetRandomWardrobe(RandomWardrobe);
          AddRandomSlot(Avatar, uwr);
        }
      }
      Avatar.BuildCharacter();
    }
  }
}
