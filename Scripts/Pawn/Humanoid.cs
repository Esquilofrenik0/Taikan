using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;


namespace Postcarbon {
  [System.Serializable]
  public class Humanoid: Pawn {
    [Header("Components")]
    public DynamicCharacterAvatar avatar;

    [Header("Movement")]
    [HideInInspector] public bool crouching = false;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public Vector3 direction = Vector3.zero;

    #region Init
    public override void Respawn() {
      base.Respawn();
      equipment.init();
      avatar.LoadDefaultWardrobe();
      Timer.rDelay(this, equipment.dHolster, 0.05f, equipment.holsterRoutine);
      Timer.rDelay(this, RefreshStats, 0.1f, equipment.refreshRoutine);
    }

    public void RandomGender() {
      float male = Random.Range(0, 2);
      if (male < 1) {
        if (avatar.activeRace.name == "HumanMale") { avatar.ChangeRace("HumanFemale"); }
        else if (avatar.activeRace.name == "o3n Male") { avatar.ChangeRace("o3n Female"); }
      }
      else {
        if (avatar.activeRace.name == "HumanFemale") { avatar.ChangeRace("HumanMale"); }
        else if (avatar.activeRace.name == "o3n Female") { avatar.ChangeRace("o3n Male"); }
      }
      avatar.BuildCharacter();
    }

    public void SetHeight(float height) {
      avatar.GetDNA()["height"].Set(height);
    }
    #endregion



    #region Actions
    public void Block(bool block) {
      if (block) {
        if (state == 0 || state == (int)pS.Sprint) {
          if (!equipment.holstered.Value) { equipment.holstered.Value = true; equipment.Holster(equipment.holstered.Value); }
          if (equipment.weapon[0] != 0 && equipment.weapon[1] == 0 && GetNetworkedObject(equipment.weapon[0]).GetComponent<Weapon>().dWeapon is dGun) {
            if (!aiming) {
              aiming = true;
              anim.SetBool("Aiming", true);
              AniTrig("Aim");
              SetSpeed();
              if (GetComponent<Player>()) { GetComponent<Player>().cam.fieldOfView = 45; }
            }
          }
          else {
            SetState((int)pS.Block);
            AniTrig("Block");
          }
        }
      }
      else {
        if (anim.GetInteger("State") == (int)pS.Block) { SetState(0); }
        if (aiming) {
          aiming = false;
          anim.SetBool("Aiming", false);
          SetSpeed();
          if (GetComponent<Player>()) { GetComponent<Player>().cam.fieldOfView = 60; }
        }
      }
    }

    public void Crouch(bool crouch) {
      crouching = crouch;
      anim.SetBool("Crouching", crouch);
      SetSpeed();
    }

    public void Sprint(bool sprint) {
      if (sprint) {
        if (grounded && !crouching) {
          if (state == 0) { SetState((int)pS.Sprint); }
          else if (state == (int)pS.Sprint) {
            if (GetComponent<Hero>()) {
              Hero hero = GetComponent<Hero>();
              if (!hero.StaminaCost(hero.sprintCost * Time.deltaTime)) { SetState(0); }
            }
          }
        }
        else if (crouching || !grounded) { if (state == (int)pS.Sprint) { SetState(0); } }
      }
      else if (!sprint && anim.GetInteger("State") == (int)pS.Sprint) { SetState(0); }
    }
    #endregion

    #region Stats
    public void RefreshStats() {
      defense.Value = baseDefense;
      damage.Value = baseDamage;
      for (int i = 0; i < 2; i++) {
        if (equipment.weapon[i] != 0) {
          dWeapon dWeapon = GetNetworkedObject(equipment.weapon[i]).GetComponent<Weapon>().dWeapon;
          damage.Value += dWeapon.damage;
          if(dWeapon is dShield){
            dShield dShield = dWeapon as dShield;
            defense.Value += dShield.defense;
          }
        }
      }
      for (int i = 0; i < 5; i++) {
        if (equipment.armor[i] != null) {
          dArmor dArmor = equipment.armor[i];
          defense.Value += dArmor.defense;
        }
      }
    }
    #endregion
  }
}
