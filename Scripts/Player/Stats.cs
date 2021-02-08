using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;

namespace Postcarbon {
  public class Stats: NetworkedBehaviour {
    public Pawn pawn;
    public GameObject floatingHealthBar;
    public float maxHealth = 100;
    public float healthRegen = 0;
    public float baseDamage = 1;
    public float baseDefense = 1;
    public NetworkedVarFloat health = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 100f);
    public NetworkedVarFloat damage = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);
    public NetworkedVarFloat defense = new NetworkedVarFloat(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 1f);
    [HideInInspector] public Coroutine disableHealthBar;

    public void TakeDamage(float amount) {
      UpdateHealth(-amount);
      Timer.Delay(this, UpdateFloatingHealthBar, 0.1f);
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
        pawn.Die();
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

    public void RefreshStats() {
      defense.Value = baseDefense;
      damage.Value = baseDamage;
      for (int i = 0; i < 2; i++) {
        if (pawn.equipment.weapon[i]) {
          damage.Value += pawn.equipment.weapon[i].damage;
          if (pawn.equipment.weapon[i] is dShield) {
            dShield dShield = (dShield)pawn.equipment.weapon[i];
            defense.Value += dShield.defense;
          }
        }
      }
      for (int i = 0; i < pawn.equipment.armor.Count; i++) {
        if (pawn.equipment.armor[i]) {
          defense.Value += pawn.equipment.armor[i].defense;
        }
      }
    }

  }
}