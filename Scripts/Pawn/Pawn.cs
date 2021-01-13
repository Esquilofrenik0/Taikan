using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Messaging;

namespace SRPG {
  [System.Serializable]
  public class Pawn: NetworkedBehaviour {
    [Header("Components")]
    public Animator anim;
    public GameObject floatingHealthBar;
    public Collider col;
    public Inventory inventory;
    public Equipment equipment;

    [Header("Stats")]
    public float maxHealth = 100;
    public float healthRegen = 0;
    public Faction faction = Faction.Loner;
    [HideInInspector] public pS state = pS.Idle;
    public NetworkedVarFloat health = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 100f);
    public NetworkedVarFloat damage = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);
    public NetworkedVarFloat defense = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);

    [Header("Movement")]
    [HideInInspector] public float speed = 5;
    [HideInInspector] public bool grounded = true;
    [HideInInspector] public float gravity = -9.81f;
    [HideInInspector] public Vector3 spawnPoint;

    [Header("Combat")]
    public float baseDamage = 1;
    public float baseDefense = 1;
    [HideInInspector] public int combo = 0;
    [HideInInspector] public Coroutine resetCombo;
    [HideInInspector] public Coroutine disableHealthBar;

    [Header("Ragdoll")]
    [HideInInspector] public Rigidbody[] bonesRB;
    [HideInInspector] public Collider[] bonesCol;


    #region Init
    public void initRagdoll() {
      bonesRB = transform.GetChild(0).GetComponentsInChildren<Rigidbody>(true);
      bonesCol = transform.GetChild(0).GetComponentsInChildren<Collider>(true);
    }

    public void DisableRagdoll() {
      anim.enabled = true;
      col.enabled = true;
      for (int i = 0; i < bonesRB.Length; i++) {
        bonesCol[i].enabled = false;
        bonesRB[i].isKinematic = true;
      }
    }

    public void EnableRagdoll() {
      anim.enabled = false;
      col.enabled = false;
      for (int i = 0; i < bonesRB.Length; i++) {
        bonesCol[i].enabled = true;
        bonesRB[i].isKinematic = false;
      }
    }

    public virtual void Respawn() {
      transform.position = spawnPoint;
      health.Value = maxHealth;
      grounded = true;
      equipment.holstered.Value = false;
      equipment.Holster(equipment.holstered.Value);
      state = pS.Idle;
      DisableRagdoll();
    }
    #endregion

    #region Utils
    public void IsGrounded() {
      Ray ray = new Ray(col.bounds.center, Vector3.down);
      grounded = Physics.SphereCast(ray, 0.15f, col.bounds.extents.y);
      anim.SetBool("Grounded", grounded);
    }

    public void Equip() {
      if (state == pS.Idle || state == pS.Sprint) {
        equipment.holstered.Value = !equipment.holstered.Value;
        equipment.Holster(equipment.holstered.Value);
      }
    }
    #endregion

    #region Combat
    public void Attack() {
      if (state == pS.Idle || state == pS.Sprint) {
        if (!equipment.holstered.Value) { equipment.holstered.Value = true; equipment.Holster(equipment.holstered.Value); }
        if (equipment.item[0] == 0) { Slash(); }
        else if (GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.isRanged) { Shoot(); }
        else if (!GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.isRanged) { Slash(); }
      }
    }

    public void Slash() {
      SetState(pS.Attack);
      anim.SetInteger("Combo", combo);
      anim.SetTrigger("Attack");
      resetCombo = Timer.rDelay(this, ResetCombo, 2, resetCombo);
      combo += 1;
      if (equipment.item[0] == 0) { combo %= 4; }
      else {
        wT wT = GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>().dWeapon.wType;
        if (wT == wT.Unarmed || wT == wT.Sword) { combo %= 2; }
        else if (wT == wT.WoodAxe) { combo %= 1; }
      }
    }

    public void Shoot() {
      SetState(pS.Attack);
      anim.SetTrigger("Attack");
      Weapon weapon = GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>();
      weapon.audioSource.PlayOneShot(weapon.dWeapon.audioClip[0]);
      weapon.fx.SetActive(true);
      RaycastHit hit;
      bool raycast = false;
      var layerMask = (1 << 9);
      layerMask = ~layerMask;
      if (GetComponent<Player>()) {
        Player player = GetComponent<Player>();
        raycast = Physics.Raycast(player.vcam.transform.position, player.vcam.transform.forward, out hit, 100, layerMask);
      }
      else { raycast = Physics.Raycast(transform.position, transform.forward, out hit, 100, layerMask); }
      if (raycast && hit.collider.GetComponent<Pawn>()) { hit.collider.GetComponent<Pawn>().TakeDamage(damage.Value); }
    }

    public void ResetCombo() {
      combo = 0;
      resetCombo = null;
    }

    public void ResetWeaponTrace() {
      if (equipment.item[0] != 0) {
        Weapon weapon = GetNetworkedObject(equipment.item[0]).GetComponent<Weapon>();
        if (state == pS.Attack) {
          if (!weapon.dWeapon.isRanged) {
            weapon.attacking = true;
          }
        }
        else if (state != pS.Attack) {
          if (weapon.attacking) {
            weapon.attacking = false;
            weapon.resetAttack = true;
          }
        }
      }
    }
    #endregion

    #region Stats
    public void TakeDamage(float amount) {
      UpdateHealth(-amount);
      UpdateFloatingHealthBar(health.Value / maxHealth);
    }


    public void UpdateHealth(float amount) {
      health.Value += amount;
      if (health.Value > maxHealth) { health.Value = maxHealth; }
      else if (health.Value <= 0) {
        health.Value = 0f;
        Die();
      }
    }

    public void UpdateFloatingHealthBar(float percent) {
      floatingHealthBar.SetActive(true);
      floatingHealthBar.GetComponent<UI_Bar>().SetPercent(percent);
      Timer.rDelay(this, DisableHealthBar, 5, disableHealthBar);
    }

    public void DisableHealthBar() {
      floatingHealthBar.SetActive(false);
    }

    public bool HealthCost(float cost) {
      if (health.Value - cost > 0) {
        health.Value -= cost;
        return true;
      }
      else { return false; }
    }

    public virtual void Die() {
      InvokeServerRpc(sDie);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sDie() {
      InvokeClientRpcOnEveryone(cDie);
    }

    [ClientRPC]
    public void cDie() {
      SetState(pS.Dead);
      // EnableRagdoll();
      Timer.Delay(this, Respawn, 5);
    }
    #endregion

    #region State
    public void SetState(pS pState) {
      state = pState;
      anim.SetInteger("State", (int)state);
    }
    #endregion
  }
}
