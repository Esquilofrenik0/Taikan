using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Spawning;
using MLAPI.Connection;
using MLAPI.Serialization;
using System.IO;

namespace Postcarbon {
  [System.Serializable]
  public class Equipment: NetworkedBehaviour {
    public Humanoid humanoid;
    public List<dItem> initItems;
    public List<Transform> weaponSlot = new List<Transform>() { null, null, null, null };
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    // public NetworkedList<ulong> weapon = new NetworkedList<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<ulong> { 0, 0 });
    public List<Weapon> weapon = new List<Weapon> { null, null };
    public NetworkedList<dItem> equip = new NetworkedList<dItem>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<dItem> { null, null, null, null, null, null, null });
    [HideInInspector] public Coroutine holsterRoutine;
    [HideInInspector] public Coroutine refreshRoutine;

    void equipChanged(NetworkedListEvent<dItem> changeEvent) {
      if (equip[changeEvent.index] == null) { Undress(changeEvent.index); }
      else { Dress(equip[changeEvent.index]); }
      humanoid.RefreshStats();
      if (IsLocalPlayer && GetComponent<HUD>() && GetComponent<HUD>().hero) {
        GetComponent<HUD>().Refresh();
        GetComponent<HUD>().DisplayStats();
      }
    }

    public override void NetworkStart() {
      base.NetworkStart();
      equip.OnListChanged += equipChanged;
      if (IsServer) { for (int i = 0; i < initItems.Count; i++) { EquipItem(initItems[i]); } }
      init();
    }

    public void init() {
      for (int i = 0; i < 7; i++) { if (equip[i]) { Dress(equip[i]); } }
      Timer.rDelay(this, dHolster, 0.05f, holsterRoutine);
      Timer.rDelay(this, humanoid.RefreshStats, 0.1f, refreshRoutine);
    }

    public void dHolster() { Holster(holstered.Value); }

    [ServerRPC(RequireOwnership = false)]
    public void Holster(bool equipped) {
      if (IsServer) { InvokeClientRpcOnEveryone(nHolster, equipped); }
      else { InvokeServerRpc(Holster, equipped); }
    }

    [ClientRPC]
    public void nHolster(bool equipped) {
      for (int i = 0; i < weapon.Count; i++) {
        if (weapon[i]) {
          if (equipped) { weapon[i].transform.SetParent(weaponSlot[i]); }
          else { weapon[i].transform.SetParent(weaponSlot[i + 2]); }
          weapon[i].transform.localPosition = Vector3.zero;
          weapon[i].transform.localRotation = Quaternion.identity;
        }
      }
    }

    public void ClearSlot(int slot, dItem dItem) {
      UnequipItem(slot);
      if (slot < 2) {
        dWeapon dWeapon = dItem as dWeapon;
        if (slot == 0) { if (dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(1); } }
        else if (slot == 1) { if (weapon[0] && weapon[0].dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(0); } }
      }
    }

    public void UpdateSlot(int slot, dItem dItem) {
      if (slot == 0) {
        dWeapon dWeapon = dItem as dWeapon;
        humanoid.anim.SetInteger("Weapon", (int)dWeapon.wType);
      }
      else if (slot == 1) {
        dWeapon dWeapon = dItem as dWeapon;
        if (dWeapon is dShield) { humanoid.anim.SetBool("Shield", true); }
      }
    }

    public void EquipItem(dItem dItem) {
      int slot = GetSlot(dItem);
      ClearSlot(slot, dItem);
      UpdateSlot(slot, dItem);
      equip[slot] = dItem;
    }

    public void SpawnWeapon(int slot, dItem dItem) {
      dWeapon dWeapon = dItem as dWeapon;
      GameObject spawn = Instantiate(dWeapon.resource, weaponSlot[EquipSlot(slot)]);
      weapon[slot] = spawn.GetComponent<Weapon>();
    }

    public void UnequipItem(int slot) {
      if (equip[slot] == null) { return; }
      if (GetComponent<Inventory>()) { GetComponent<Inventory>().Store(equip[slot], 1); }
      equip[slot] = null;
    }

    public void DestroyWeapon(int slot) {
      if (slot == 0) { humanoid.anim.SetInteger("Weapon", 0); }
      else if (slot == 1) { humanoid.anim.SetBool("Shield", false); }
      Destroy(weapon[slot].gameObject);
      weapon[slot] = null;
    }

    [ServerRPC(RequireOwnership = false)]
    public void Dress(dItem dItem) {
      if (IsServer) { InvokeClientRpcOnEveryone(nDress, dItem); }
      else { InvokeServerRpc(Dress, dItem); }
    }

    [ClientRPC]
    public void nDress(dItem dItem) {
      if (dItem is dArmor) {
        dArmor dArmor = dItem as dArmor;
        humanoid.avatar.SetSlot(dArmor.armorSlot.ToString(), dArmor.name);
        humanoid.avatar.BuildCharacter();
      }
      else if (dItem is dWeapon) {
        if (weapon[GetSlot(dItem)] == null) {
          SpawnWeapon(GetSlot(dItem), dItem);
        }
      }
    }

    [ServerRPC(RequireOwnership = false)]
    public void Undress(int slot) {
      if (IsServer) { InvokeClientRpcOnEveryone(nUndress, slot); }
      else { InvokeServerRpc(Undress, slot); }
    }

    [ClientRPC]
    public void nUndress(int slot) {
      if (slot > 1) {
        aS armorSlot = (aS)(slot - 2);
        humanoid.avatar.ClearSlot(armorSlot.ToString());
        humanoid.avatar.BuildCharacter();
      }
      else {
        if (weapon[slot] != null) {
          DestroyWeapon(slot);
        }
      }
    }

    public int EquipSlot(int slot) {
      if (holstered.Value == false) { slot += 2; }
      return slot;
    }

    public int WeaponSlot(int weaponSlot) {
      int slot = weaponSlot;
      if (weaponSlot == 2 || weaponSlot == 3) { slot = 0; }
      return slot;
    }

    public int ArmorSlot(int armorSlot) {
      int slot = armorSlot + 2;
      return slot;
    }

    public int GetSlot(dItem dItem) {
      int slot = 0;
      if (dItem is dWeapon) {
        dWeapon weapon = dItem as dWeapon;
        slot = WeaponSlot((int)weapon.weaponSlot);
      }
      else if (dItem is dArmor) {
        dArmor armor = dItem as dArmor;
        slot = ArmorSlot((int)armor.armorSlot);
      }
      return slot;
    }
  }
}
