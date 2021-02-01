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
    public List<Transform> weaponSlot = new List<Transform>() { null, null, null, null };
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    public NetworkedList<ulong> weapon = new NetworkedList<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<ulong> { 0, 0 });
    public NetworkedList<string> equip = new NetworkedList<string>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<string> { null, null, null, null, null, null, null });
    public List<dItem> initItems;
    [HideInInspector] public Database data;
    [HideInInspector] public Coroutine holsterRoutine;
    [HideInInspector] public Coroutine refreshRoutine;

    public override void NetworkStart() {
      base.NetworkStart();
      data = GameObject.Find("Database").GetComponent<Database>();
      equip.OnListChanged += equipChanged;
      if (IsServer) { for (int i = 0; i < initItems.Count; i++) { EquipItem(initItems[i]); } }
      init();
    }

    void equipChanged(NetworkedListEvent<string> changeEvent) {
      if (changeEvent.index < 2) { Holster(holstered.Value); }
      else {
        if (equip[changeEvent.index] == null) { Undress(changeEvent.index); }
        else { DressUp(equip[changeEvent.index]); }
      }
      humanoid.RefreshStats();
      if (IsLocalPlayer && GetComponent<HUD>() && GetComponent<HUD>().hero) {
        GetComponent<HUD>().Refresh();
        GetComponent<HUD>().DisplayStats();
      }
    }

    public void init() {
      for (int i = 0; i < 7; i++) {
        if (i > 1) { if (equip[i] != null) { DressUp(equip[i]); } }
        else { if (equip[i] != null && weapon[i] == 0) { EquipItem(data.GetItem(equip[i])); } }
      }
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
        if (weapon[i] != 0) {
          NetworkedObject w = GetNetworkedObject(weapon[i]);
          if (equipped) { w.transform.SetParent(weaponSlot[i]); }
          else { w.transform.SetParent(weaponSlot[i + 2]); }
          w.transform.localPosition = Vector3.zero;
          w.transform.localRotation = Quaternion.identity;
        }
      }
    }

    public void ClearSlot(int slot, dItem dItem) {
      UnequipItem(slot);
      if (slot < 2) {
        dWeapon dWeapon = dItem as dWeapon;
        if (slot == 0) { if (dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(1); } }
        else if (slot == 1) { if (weapon[0] != 0 && GetNetworkedObject(weapon[0]).GetComponent<Weapon>().dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(0); } }
      }
    }

    public void UpdateSlot(int slot, dItem dItem) {
      if (slot == 0) {
        dWeapon dWeapon = dItem as dWeapon;
        humanoid.anim.SetInteger("Weapon", (int)dWeapon.wType);
      }
      else if (slot == 1) {
        dWeapon dWeapon = dItem as dWeapon;
        if (dWeapon.wType == wT.Shield) { humanoid.anim.SetBool("Shield", true); }
      }
    }

    public void EquipItem(dItem dItem) {
      int slot = GetSlot(dItem);
      ClearSlot(slot, dItem);
      UpdateSlot(slot, dItem);
      if (slot < 2) {
        if (IsServer) { SpawnWeapon(slot, dItem.name); }
        else { InvokeServerRpc(SpawnWeapon, slot, dItem.name); }
      }
      equip[slot] = dItem.name;
    }

    [ServerRPC(RequireOwnership = false)]
    public void SpawnWeapon(int slot, string name) {
      dWeapon dWeapon = data.GetItem(name) as dWeapon;
      NetworkedObject spawn = Instantiate(dWeapon.resource).GetComponent<NetworkedObject>();
      spawn.GetComponent<Weapon>().ownerID = NetworkId;
      spawn.SpawnWithOwnership(OwnerClientId);
      weapon[slot] = spawn.NetworkId;
    }

    public void UnequipItem(int slot) {
      if (equip[slot] == null) { return; }
      InvokeServerRpc(sUnequipItem, slot);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUnequipItem(int slot) {
      if (equip[slot] == null) { return; }
      dItem dItem = data.GetItem(equip[slot]);
      if (GetComponent<Inventory>()) { GetComponent<Inventory>().Store(dItem, 1); }
      if (slot < 2 && weapon[slot] != 0) {
        if (slot == 0) { humanoid.anim.SetInteger("Weapon", 0); }
        else if (slot == 1) { humanoid.anim.SetBool("Shield", false); }
        GameObject toDestroy = GetNetworkedObject(weapon[slot]).gameObject;
        Destroy(toDestroy);
        weapon[slot] = 0;
      }
      equip[slot] = null;
    }

    [ServerRPC(RequireOwnership = false)]
    public void DressUp(string name) {
      if (IsServer) { InvokeClientRpcOnEveryone(nDressUp, name); }
      else { InvokeServerRpc(DressUp, name); }
    }

    [ClientRPC]
    public void nDressUp(string name) {
      dArmor dArmor = data.GetItem(name) as dArmor;
      humanoid.avatar.SetSlot(dArmor.armorSlot.ToString(), dArmor.Name);
      humanoid.avatar.BuildCharacter();
    }

    [ServerRPC(RequireOwnership = false)]
    public void Undress(int slot) {
      if (IsServer) { InvokeClientRpcOnEveryone(nUndress, slot); }
      else { InvokeServerRpc(Undress, slot); }
    }

    [ClientRPC]
    public void nUndress(int slot) {
      aS armorSlot = (aS)(slot - 2);
      humanoid.avatar.ClearSlot(armorSlot.ToString());
      humanoid.avatar.BuildCharacter();
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
      if (dItem.type == iT.Weapon) {
        dWeapon weapon = dItem as dWeapon;
        slot = WeaponSlot((int)weapon.weaponSlot);
      }
      else if (dItem.type == iT.Armor) {
        dArmor armor = dItem as dArmor;
        slot = ArmorSlot((int)armor.armorSlot);
      }
      return slot;
    }
  }
}
