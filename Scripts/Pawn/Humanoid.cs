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
      if (GetComponent<RandomUMA>()) { GetComponent<RandomUMA>().Randomize(avatar); }
      else { RandomGender(); }
      equipment.init();
      Timer.rDelay(this, equipment.dHolster, 0.05f, equipment.holsterRoutine);
      Timer.rDelay(this, RefreshStats, 0.1f, equipment.refreshRoutine);
    }

    public void RandomGender() {
      float male = Random.Range(0, 2);
      if (male < 1) { avatar.ChangeRace("HumanMaleDCS"); }
      else { avatar.ChangeRace("HumanFemaleDCS"); }
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
          if (equipment.weapon1.Value != 0 && equipment.weapon2.Value == 0 && GetNetworkedObject(equipment.weapon1.Value).GetComponent<Weapon>().dWeapon.isRanged) {
            if (!aiming) {
              aiming = true;
              anim.SetBool("Aiming", true);
              anim.SetTrigger("Aim");
              SetSpeed();
              if (GetComponent<Player>()) { GetComponent<Player>().heroCam.m_Lens.FieldOfView = 30; }
            }
          }
          else {
            SetState((int)pS.Block);
            anim.SetTrigger("Block");
            if (equipment.weapon2.Value != 0) {
              // equipment.weapon2.Value.GetComponent<Collider>().isTrigger = false;
              GetNetworkedObject(equipment.weapon2.Value).transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
          }
        }
      }
      else {
        if (anim.GetInteger("State") == (int)pS.Block) {
          SetState(0);
          if (equipment.weapon2.Value != 0) {
            // equipment.weapon2.Value.GetComponent<Collider>().isTrigger = true;
            GetNetworkedObject(equipment.weapon2.Value).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
          }
        }
        if (aiming) {
          aiming = false;
          anim.SetBool("Aiming", false);
          SetSpeed();
          if (GetComponent<Player>()) { GetComponent<Player>().heroCam.m_Lens.FieldOfView = 45; }
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
        if (equipment.equip[i] != null) {
          dWeapon dWeapon = equipment.data.GetItem(equipment.equip[i]) as dWeapon;
          defense.Value += dWeapon.defense;
          damage.Value += dWeapon.damage;
        }
      }
      for (int i = 0; i < 5; i++) {
        if (equipment.equip[i + 2] != null) {
          dArmor dArmor = equipment.data.GetItem(equipment.equip[i + 2]) as dArmor;
          defense.Value += dArmor.defense;
        }
      }
      if (!IsLocalPlayer) { return; }
      if (GetComponent<HUD>()) {
        GetComponent<HUD>().Refresh();
        GetComponent<HUD>().DisplayStats();
      }
    }
    #endregion
  }
}
