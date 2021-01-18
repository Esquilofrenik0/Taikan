using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;


namespace SRPG {
  [System.Serializable]
  public class Human: Pawn {
    [Header("Components")]
    public DynamicCharacterAvatar avatar;

    [Header("Movement")]
    public float idleSpeed = 5;
    public float crouchSpeed = 2;
    public float sprintSpeed = 8;
    public float jumpHeight = 2;
    [HideInInspector] public bool crouching = false;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public Vector3 direction = Vector3.zero;

    #region Init
    public override void Respawn() {
      base.Respawn();
      if (avatar.activeRace.name == "HumanMaleDCS") {
        // avatar.GetDNA()["height"].Set(0.4f);
        avatar.SetSlot("Underwear", "MaleUnderwear");
      }
      else if (avatar.activeRace.name == "HumanFemaleDCS") {
        // avatar.GetDNA()["height"].Set(0.6f);
        avatar.SetSlot("Underwear", "FemaleUndies2");
      }
      avatar.BuildCharacter();
    }
    #endregion

    #region Actions
    public void Block(bool block) {
      if (!block) {
        if (anim.GetInteger("State") == (int)pS.Block) { SetState(0); }
        if (aiming) {
          aiming = false;
          anim.SetBool("Aiming", false);
          if (state == (int)pS.Block) { SetState(0); }
        }
        if (equipment.item[1] != 0 && GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.wType == wT.Shield) {
          GetNetworkedObject(equipment.item[1]).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
      }
      else if (block) {
        if (state == 0 || state == (int)pS.Sprint) {
          if (!equipment.holstered.Value) { equipment.holstered.Value = true; equipment.Holster(equipment.holstered.Value); }
          if (equipment.item[0] == 0 || equipment.item[1] != 0) {
            SetState((int)pS.Block);
            anim.SetTrigger("Block");
            if (equipment.item[1] != 0 && GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.wType == wT.Shield) {
              GetNetworkedObject(equipment.item[1]).transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
          }
          else if (!GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.isRanged) {
            SetState((int)pS.Block);
            anim.SetTrigger("Block");
          }
          else if (GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.isRanged) {
            if (!aiming) {
              aiming = true;
              anim.SetBool("Aiming", true);
              anim.SetTrigger("Aim");
            }
          }
        }
      }
    }

    public void Crouch(bool crouch) {
      crouching = crouch;
    }

    public void Sprint(bool sprint) {
      if (sprint) {
        if (grounded && !crouching) {
          if (state == 0) {
            SetState((int)pS.Sprint);
          }
          else if (state == (int)pS.Sprint) {
            if (GetComponent<Hero>()) {
              Hero hero = GetComponent<Hero>();
              if (!hero.StaminaCost(hero.sprintCost * Time.deltaTime)) {
                SetState(0);
              }
            }
          }
        }
        else if (crouching || !grounded) { if (state == (int)pS.Sprint) { SetState(0); } }
      }
      else if (!sprint && anim.GetInteger("State") == (int)pS.Sprint) { SetState(0); }
    }

    public void SetSpeed() {
      if (aiming) { speed = crouchSpeed; }
      else if (crouching) { speed = crouchSpeed; }
      else if (state == (int)pS.Idle) { speed = idleSpeed; }
      else if (state == (int)pS.Attack) { speed = idleSpeed; }
      else if (state == (int)pS.Block) { speed = crouchSpeed; }
      else if (state == (int)pS.Sprint) { speed = sprintSpeed; }
      else if (state == (int)pS.Dodge) { speed = idleSpeed; }
      else if (state == (int)pS.Climb) { speed = crouchSpeed; }
      else if (state == (int)pS.Swim) { speed = crouchSpeed; }
    }
    #endregion

    #region State
    public void RefreshState() {
      anim.SetBool("Crouching", crouching);
      SetSpeed();
    }
    #endregion

    #region Stats
    public void RefreshStats() {
      defense.Value = baseDefense;
      if (equipment.item[2] != 0) { defense.Value += GetNetworkedObject(equipment.item[2]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[3] != 0) { defense.Value += GetNetworkedObject(equipment.item[3]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[4] != 0) { defense.Value += GetNetworkedObject(equipment.item[4]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[5] != 0) { defense.Value += GetNetworkedObject(equipment.item[5]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[6] != 0) { defense.Value += GetNetworkedObject(equipment.item[6]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[1] != 0) { defense.Value += GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.defense; }
      damage.Value = baseDamage;
      if (equipment.item[0] != 0) { damage.Value += GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.damage; }
      if (equipment.item[1] != 0) { damage.Value += GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.damage; }
      if (!IsLocalPlayer) { return; }
      if (GetComponent<Hero>()) {
        Hero hero = GetComponent<Hero>();
        hero.hud.Refresh();
        hero.hud.DisplayStats();
      }
    }
    #endregion
  }
}
