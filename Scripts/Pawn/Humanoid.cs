using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using MLAPI.NetworkedVar;


namespace Postcarbon {
  [System.Serializable]
  public class Humanoid: Pawn {
    [Header("Components")]
    public DynamicCharacterAvatar avatar;
    public NetworkedVarString recipeAvatar = new NetworkedVarString(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendChannel = "Avatar", SendTickrate = 0f }, null);

    [Header("Movement")]
    [HideInInspector] public bool crouching = false;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public Vector3 direction = Vector3.zero;

    #region Init
    public void recipeAvatarChanged(string oldRecipe, string newRecipe){
      LoadAvatar();
    }

    public void LoadAvatar() {
      if (recipeAvatar.Value != null) {
        avatar.LoadFromRecipeString(recipeAvatar.Value);
        avatar.LoadDefaultWardrobe();
        avatar.BuildCharacter();
      }
    }

    public override void NetworkStart(){
      base.NetworkStart();
      recipeAvatar.OnValueChanged += recipeAvatarChanged;    
    }

    public override void Respawn() {
      base.Respawn();
      avatar.LoadDefaultWardrobe();
      equipment.init();
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
        if (state.Value == 0 || state.Value == (int)pS.Sprint) {
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
            state.Value = (int)pS.Block;
            AniTrig("Block");
          }
        }
      }
      else {
        if (anim.GetInteger("State") == (int)pS.Block) { state.Value = 0; }
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
          if (state.Value == 0) { state.Value = (int)pS.Sprint; }
          else if (state.Value == (int)pS.Sprint) {
            if (GetComponent<Hero>()) {
              Hero hero = GetComponent<Hero>();
              if (!hero.StaminaCost(hero.sprintCost * Time.deltaTime)) { state.Value = 0; }
            }
          }
        }
        else if (crouching || !grounded) { if (state.Value == (int)pS.Sprint) { state.Value = 0; } }
      }
      else if (!sprint && anim.GetInteger("State") == (int)pS.Sprint) { state.Value = 0; }
    }
    #endregion

    #region Stats
    public void RefreshStats() {
      defense.Value = baseDefense;
      damage.Value = baseDamage;
      for (int i = 0; i < 2; i++) {
        if (equipment.equip[i] != null) {
          dWeapon dWeapon = equipment.equip[i] as dWeapon;
          damage.Value += dWeapon.damage;
          if (dWeapon is dShield) {
            dShield dShield = (dShield)dWeapon;
            defense.Value += dShield.defense;
          }
        }
      }
      for (int i = 0; i < 5; i++) {
        if (equipment.equip[i + 2] != null) {
          dArmor dArmor = equipment.equip[i + 2] as dArmor;
          defense.Value += dArmor.defense;
        }
      }
    }
    #endregion
  }
}
