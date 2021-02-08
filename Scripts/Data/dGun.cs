using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Ranged", menuName = "SRPG/Weapon/Gun")]
  [System.Serializable]
  public class dGun: dWeapon {
    public int clipSize = 0;
    public AudioClip[] audioClip;
    public dAmmo dAmmo;
    public int clipAmmo;
    public int totalAmmo;

    public void EquipAmmo(dAmmo _dAmmo, int _amount) {
      dAmmo = _dAmmo;
      totalAmmo = _amount;
    }

    public void Reload(Pawn pawn) {
      if (clipAmmo < clipSize) {
        if (pawn is Hero && pawn.IsLocalPlayer) {
          Hero hero = (Hero)pawn;
          int missing = clipSize - clipAmmo;
          if (totalAmmo >= missing) {
            clipAmmo += missing;
            totalAmmo -= missing;
          }
          else {
            clipAmmo += totalAmmo;
            totalAmmo = 0;
            dAmmo = null;
          }
          hero.hud.RefreshAmmo();
        }
        else { clipAmmo = clipSize; }
      }
    }

  }
}