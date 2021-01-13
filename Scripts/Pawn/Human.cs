using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Spawning;
using UMA;
using UMA.CharacterSystem;
using UnityEngine.UI;


namespace SRPG {
  [System.Serializable]
  public class Human: Pawn {
    [Header("Components")]
    public DynamicCharacterAvatar avatar;

    [Header("Movement")]
    public float idleSpeed = 5;
    public float crouchSpeed = 2;
    public float sprintSpeed = 8;
    public float jumpHeight = 5;
    [HideInInspector] public bool aiming = false;
    [HideInInspector] public bool crouching = false;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public Vector3 direction = Vector3.zero;

    [Header("Combat")]
    public GameObject headLook;
    public GameObject spineLook;
    public GameObject neckLook;

    #region Init
    public override void Respawn() {
      base.Respawn();
      if (avatar.activeRace.name == "HumanMaleDCS") {
        avatar.SetSlot("Underwear", "MaleUnderwear");
      }
      else if (avatar.activeRace.name == "HumanFemaleDCS") {
        avatar.SetSlot("Underwear", "FemaleUndies2");
      }
      avatar.BuildCharacter();
    }
    #endregion

    #region Actions
    public void Block(bool block) {
      if (!block) {
        if (anim.GetInteger("State") == (int)pS.Block) { SetState(pS.Idle); }
        if (aiming) {
          aiming = false;
          anim.SetBool("Aiming", false);
          if (state == pS.Block) { SetState(pS.Idle); }
        }
        if (equipment.item[1] != 0 && GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.wType == wT.Shield) {
          GetNetworkedObject(equipment.item[1]).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
      }
      else if (block) {
        if (state == pS.Idle || state == pS.Sprint) {
          if (!equipment.holstered.Value) { equipment.holstered.Value = true; equipment.Holster(equipment.holstered.Value); }
          if (equipment.item[0] == 0 || equipment.item[1] != 0) {
            SetState(pS.Block);
            anim.SetTrigger("Block");
            if (equipment.item[1] != 0 && GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.wType == wT.Shield) {
              GetNetworkedObject(equipment.item[1]).transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
          }
          else if (!GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.isRanged) {
            SetState(pS.Block);
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
          if (state == pS.Idle) {
            SetState(pS.Sprint);
          }
          else if (state == pS.Sprint) {
            if (GetComponent<Hero>()) {
              Hero hero = GetComponent<Hero>();
              if (!hero.StaminaCost(hero.sprintCost * Time.deltaTime)) {
                SetState(pS.Idle);
              }
            }
          }
        }
        else if (crouching && state == pS.Sprint) { SetState(pS.Idle); }
      }
      else if (!sprint && anim.GetInteger("State") == (int)pS.Sprint) { SetState(pS.Idle); }
    }

    public void SetSpeed() {
      if (aiming) { speed = crouchSpeed; }
      else if (crouching) { speed = crouchSpeed; }
      else if (state == pS.Idle) { speed = idleSpeed; }
      else if (state == pS.Attack) { speed = idleSpeed; }
      else if (state == pS.Block) { speed = crouchSpeed; }
      else if (state == pS.Sprint) { speed = sprintSpeed; }
      else if (state == pS.Dodge) { speed = idleSpeed; }
      else if (state == pS.Climb) { speed = crouchSpeed; }
      else if (state == pS.Swim) { speed = crouchSpeed; }
    }
    #endregion

    #region State
    public void RefreshState() {
      anim.SetBool("Crouching", crouching);
      if (anim.GetInteger("State") != (int)state) { SetState((pS)(anim.GetInteger("State"))); }
      SetAnimatorLayer();
      ResetWeaponTrace();
      SetSpeed();
    }
    #endregion

    #region Animator
    public void SetAnimatorLayer() {
      if (equipment.item[0] != 0) {
        Weapon weapon = GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>();
        if (anim.GetInteger("Weapon") != (int)weapon.dWeapon.wType) {
          anim.SetInteger("Weapon", (int)weapon.dWeapon.wType);
        }
      }
      else {
        if (anim.GetInteger("Weapon") != (int)wT.Unarmed) {
          anim.SetInteger("Weapon", (int)wT.Unarmed);
        }
      }
      if (aiming) {
        anim.SetLayerWeight(1, 1);
      }
      else {
        if (!equipment.holstered.Value || state == pS.Dodge || state == pS.Sprint) {
          anim.SetLayerWeight(1, 0);
        }
        else if (equipment.holstered.Value) {
          anim.SetLayerWeight(1, 1);
        }
      }
    }
    #endregion

    #region Stats
    public void RefreshStats() {
      if (!IsLocalPlayer) { return; }
      defense = baseDefense;
      if (equipment.item[2] != 0) { defense += GetNetworkedObject(equipment.item[2]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[3] != 0) { defense += GetNetworkedObject(equipment.item[3]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[4] != 0) { defense += GetNetworkedObject(equipment.item[4]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[5] != 0) { defense += GetNetworkedObject(equipment.item[5]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[6] != 0) { defense += GetNetworkedObject(equipment.item[6]).GetComponent<Armor>().dArmor.defense; }
      if (equipment.item[1] != 0) { defense += GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.defense; }
      damage = baseDamage;
      if (equipment.item[0] != 0) { damage += GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.damage; }
      if (equipment.item[1] != 0) { damage += GetNetworkedObject(equipment.item[1]).GetComponent<Weapon>().dWeapon.damage; }
      if (GetComponent<Hero>()) {
        Hero hero = GetComponent<Hero>();
        hero.hud.Refresh();
        hero.hud.DisplayStats();
      }
    }
    #endregion
  }
}
