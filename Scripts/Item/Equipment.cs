using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Spawning;
using MLAPI.Connection;

namespace SRPG {
  public class Equipment: NetworkedBehaviour {
    public Humanoid humanoid;
    public GameObject[] equipSlot = new GameObject[9];
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    public NetworkedList<ulong> item = new NetworkedList<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<ulong> { 0, 0, 0, 0, 0, 0, 0 });
    // public NetworkedList<GameObject> weapon = new NetworkedList<GameObject>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<GameObject> { null, null });
    // public NetworkedList<int> armor = new NetworkedList<int>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<int> { null, null, null, null, null });
    private ItemSpawner ispawner;

    void Awake(){
      ispawner = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>();
    }

    public override void NetworkStart() {
      base.NetworkStart();
      ispawner = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>();
      bool holst = false;
      for (int i = 0; i < item.Count; i++) { if (item[i] != 0) { holst = true; } }
      if (holst) {
        Timer.Delay(this, dHolster, 0.05f);
        Timer.Delay(this, humanoid.RefreshStats, 0.1f);
      }
    }

    public void dHolster() {
      Holster(holstered.Value);
    }

    public void Holster(bool equipped) {
      if (IsServer) { sHolster(equipped); }
      else { InvokeServerRpc(sHolster, equipped); }
    }

    public void nHolster(bool equipped) {
      for (int i = 0; i < item.Count; i++) {
        if (item[i] != 0) {
          NetworkedObject equip = GetNetworkedObject(item[i]);
          equip.transform.SetParent(equipSlot[i].transform);
          if (equip.GetComponent<Weapon>()) { if (!equipped) { equip.transform.SetParent(equipSlot[i + 7].transform); } }
          equip.transform.localPosition = Vector3.zero;
          equip.transform.localRotation = Quaternion.identity;
        }
      }
    }

    [ServerRPC(RequireOwnership = false)]
    public void sHolster(bool equipped) {
      nHolster(equipped);
      InvokeClientRpcOnEveryone(cHolster, equipped);
    }

    [ClientRPC]
    public void cHolster(bool equipped) {
      nHolster(equipped);
    }

    public void EquipItem(dItem dItem) {
      int slot = GetSlot(dItem);
      ClearSlot(slot, dItem);
      UpdateSlot(slot, dItem);
      if (IsServer) { SpawnEquip(slot, dItem.name, OwnerClientId); }
      else { InvokeServerRpc(SpawnEquip, slot, dItem.name, OwnerClientId); }
      Timer.Delay(this, dHolster, 0.05f);
      Timer.Delay(this, humanoid.RefreshStats, 0.1f);
    }

    public void ClearSlot(int slot, dItem dItem) {
      UnequipItem(slot);
      if (slot < 2) {
        dWeapon dWeapon = dItem as dWeapon;
        if (slot == 0) {
          if (dWeapon.weaponSlot == wS.TwoHand) {
            UnequipItem(WeaponSlot(1));
          }
        }
        else if (slot == 1) {
          if (item[0] != 0 && GetNetworkedObject(item[0]).GetComponent<Weapon>().dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(WeaponSlot(0)); }
        }
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
      else {
        dArmor armor = dItem as dArmor;
        humanoid.avatar.SetSlot(armor.armorSlot.ToString(), armor.Name);
        humanoid.avatar.BuildCharacter();
      }
    }

    [ServerRPC(RequireOwnership = false)]
    public void SpawnEquip(int slot, string name, ulong clientID) {
      dItem dItem = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>().GetItem(name);
      NetworkedObject spawn = Instantiate(dItem.resource, equipSlot[slot].transform).GetComponent<NetworkedObject>();
      if (slot < 2) { spawn.GetComponent<Weapon>().owner.Value = GetComponent<NetworkedObject>().NetworkId; }
      spawn.SpawnWithOwnership(clientID);
      item[slot] = spawn.GetComponent<NetworkedObject>().NetworkId;
    }

    public void UnequipItem(int slot) {
      if (item[slot] == 0) { return; }
      InvokeServerRpc(sUnequipItem, slot);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUnequipItem(int slot) {
      if (item[slot] == 0) { return; }
      NetworkedObject equip = GetNetworkedObject(item[slot]);
      if (slot == 0) { humanoid.anim.SetInteger("Weapon", 0); }
      if (slot == 1) { humanoid.anim.SetBool("Shield", false); }
      if (equip.GetComponent<Armor>()) {
        dArmor armor = equip.GetComponent<Armor>().dArmor;
        humanoid.avatar.ClearSlot(armor.armorSlot.ToString());
      }
      if (GetComponent<Inventory>()) { GetComponent<Inventory>().Store(equip.GetComponent<Item>().dItem, 1); }
      equip.UnSpawn();
      Destroy(equip.gameObject);
      item[slot] = 0;
      Timer.Delay(this, humanoid.RefreshStats, 0.1f);
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
