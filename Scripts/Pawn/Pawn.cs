using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;
using Cinemachine;

namespace Postcarbon {
  [System.Serializable]
  public class Pawn: NetworkedBehaviour {
    [Header("Components")]
    public Animator anim;
    public Rigidbody rb;
    public Collider col;
    public Equipment equipment;
    public GameObject floatingHealthBar;

    [Header("Stats")]
    public float maxHealth = 100;
    public float healthRegen = 0;
    public Faction faction = Faction.Loner;
    [HideInInspector] public int state = 0;
    public NetworkedVarFloat health = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 100f);
    public NetworkedVarFloat damage = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);
    public NetworkedVarFloat defense = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);

    [Header("Movement")]
    [HideInInspector] public float speed = 5;
    [HideInInspector] public bool grounded = true;
    [HideInInspector] public Vector3 spawnPoint;

    [Header("Combat")]
    public GameObject head;
    public GameObject spine;
    public float baseDamage = 1;
    public float baseDefense = 1;
    [HideInInspector] public int combo = 0;
    [HideInInspector] public bool aiming = false;
    [HideInInspector] public bool attacking = false;
    [HideInInspector] public bool resetAttack = true;
    [HideInInspector] public Coroutine resetCombo;
    [HideInInspector] public Coroutine disableHealthBar;

    [Header("Ragdoll")]
    public int respawnTime = 30;
    [HideInInspector] public Rigidbody[] bonesRB;

    #region Init
    public void initRagdoll() {
      bonesRB = transform.GetChild(0).GetComponentsInChildren<Rigidbody>(true);
      DisableRagdoll();
    }

    // [ServerRPC(RequireOwnership = false)]
    // public void DisableRagdoll() {
    // if (IsServer) { InvokeClientRpcOnEveryone(nDisableRagdoll); }
    // else { InvokeServerRpc(DisableRagdoll); }
    // }

    // [ClientRPC]
    public void DisableRagdoll() {
      bonesRB = transform.GetChild(0).GetComponentsInChildren<Rigidbody>(true);
      for (int i = 0; i < bonesRB.Length; i++) {
        bonesRB[i].isKinematic = true;
        bonesRB[i].GetComponent<Collider>().enabled = false;
      }
      anim.enabled = true;
      col.enabled = true;
      if (rb) { rb.isKinematic = false; }
    }

    // [ServerRPC(RequireOwnership = false)]
    // public void EnableRagdoll() {
    // if (IsServer) { InvokeClientRpcOnEveryone(nEnableRagdoll); }
    // else { InvokeServerRpc(EnableRagdoll); }
    // }

    // [ClientRPC]
    public void EnableRagdoll() {
      bonesRB = transform.GetChild(0).GetComponentsInChildren<Rigidbody>(true);
      for (int i = 0; i < bonesRB.Length; i++) {
        bonesRB[i].isKinematic = false;
        bonesRB[i].GetComponent<Collider>().enabled = true;
      }
      anim.enabled = false;
      col.enabled = false;
      if (rb) { rb.isKinematic = true; }
    }

    public virtual void Respawn() {
      transform.position = spawnPoint;
      health.Value = maxHealth;
      grounded = true;
      SetState(0);
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
      if (state == 0 || state == (int)pS.Sprint) {
        equipment.holstered.Value = !equipment.holstered.Value;
        equipment.Holster(equipment.holstered.Value);
      }
    }

    public void PunchActive(bool active) {
      for (int i = 0; i < 2; i++) { equipment.weaponSlot[i].GetComponent<Collider>().enabled = active; }
    }
    #endregion

    #region Combat
    public void Attack() {
      if (state == 0 || state == (int)pS.Sprint) {
        if (!equipment.holstered.Value) {
          equipment.holstered.Value = true;
          equipment.Holster(equipment.holstered.Value);
        }
        if (equipment.weapon[0] != 0 && GetNetworkedObject(equipment.weapon[0]).GetComponent<Weapon>().dWeapon.isRanged) { Shoot(); }
        else { Melee(); }
      }
    }

    public void Melee() {
      SetState((int)pS.Attack);
      anim.SetInteger("Combo", combo);
      AniTrig("Attack");
      resetCombo = Timer.rDelay(this, ResetCombo, 2, resetCombo);
      combo += 1;
      combo %= 2;
    }

    public void Shoot() {
      SetState((int)pS.Attack);
      AniTrig("Attack");
      Weapon weapon = GetNetworkedObject(equipment.weapon[0]).GetComponent<Weapon>();
      weapon.audioSource.PlayOneShot(weapon.dWeapon.audioClip[0]);
      weapon.fx.SetActive(true);
      RaycastHit hit;
      bool raycast = false;
      int prevLayer = gameObject.layer;
      gameObject.layer = 2;
      if (GetComponent<Player>()) {
        Player player = GetComponent<Player>();
        Ray ray = new Ray(player.cam.transform.position, player.cam.transform.forward);
        raycast = Physics.Raycast(ray, out hit, 100);
      }
      else {
        Ray ray = new Ray(col.bounds.center, transform.forward);
        raycast = Physics.Raycast(ray, out hit, 100);
      }
      gameObject.layer = prevLayer;
      if (raycast && hit.collider.GetComponent<Pawn>()) { hit.collider.GetComponent<Pawn>().TakeDamage(damage.Value); }
    }

    public void ResetCombo() {
      combo = 0;
      resetCombo = null;
    }
    #endregion

    #region Stats
    public void TakeDamage(float amount) {
      UpdateHealth(-amount);
      anim.SetTrigger("Impact");
      Timer.Delay(this, UpdateFloatingHealthBar, 0.1f);
      Timer.Delay(this, UpdateFloatingHealthBar, 0.2f);
    }

    public void UpdateHealth(float amount) {
      InvokeServerRpc(sUpdateHealth, amount);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUpdateHealth(float amount) {
      health.Value += amount;
      if (health.Value > maxHealth) { health.Value = maxHealth; }
      else if (health.Value <= 0) {
        health.Value = 0f;
        Die();
      }
    }

    public void UpdateFloatingHealthBar() {
      float percent = health.Value / maxHealth;
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

    public void Die() {
      SetState((int)pS.Dead);
      EnableRagdoll();
      Timer.Delay(this, Respawn, respawnTime);
    }
    #endregion

    #region State
    public void SetState(int pState) {
      state = pState;
      anim.SetInteger("State", state);
      SetSpeed();
    }

    public virtual void SetSpeed() { }
    #endregion

    #region Animator

    [ServerRPC(RequireOwnership = false)]
    public void AniTrig(string name) {
      anim.SetTrigger(name);
      if (IsServer) { InvokeClientRpcOnEveryone(nAniTrig, name); }
      else { InvokeServerRpc(AniTrig, name); }
    }
    [ClientRPC]
    public void nAniTrig(string name) {
      anim.SetTrigger(name);
    }
    #endregion
  }
}