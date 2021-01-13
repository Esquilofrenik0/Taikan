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
    public Human human;
    public NetworkedObject itemSpawner;
    public GameObject[] equipSlot = new GameObject[9];
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    public NetworkedList<ulong> item = new NetworkedList<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<ulong> { 0, 0, 0, 0, 0, 0, 0 });

    public override void NetworkStart() {
      base.NetworkStart();
      itemSpawner = GameObject.Find("ItemSpawner").GetComponent<NetworkedObject>();
      bool holst = false;
      for (int i = 0; i < item.Count; i++) {
        if (item[i] != 0) {
          holst = true;
          print("Client: " + OwnerClientId + " - item[" + i + "] - " + item[i]);
        }
      }
      if (holst) {
        Timer.Delay(this, dHolster,0.05f);
        Timer.Delay(this, human.RefreshStats, 0.1f);
      }
    }

    public void Holster(bool equipped) {
      if (!IsLocalPlayer) { return; }
      InvokeServerRpc(sHolster, equipped);
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

    public void dHolster(){
      Holster(holstered.Value);
    }

    public void nHolster(bool equipped) {
      for (int i = 0; i < item.Count; i++) {
        if (item[i] != 0) {
          NetworkedObject equip = GetNetworkedObject(item[i]);
          equip.transform.SetParent(equipSlot[i].transform);
          if (equip.GetComponent<Weapon>()) {
            if (!equipped) {
              equip.transform.SetParent(equipSlot[i + 7].transform);
            }
          }
          equip.transform.localPosition = Vector3.zero;
          equip.transform.localRotation = Quaternion.identity;
        }
      }
    }

    public void EquipItem(dItem dItem) {
      if (!IsLocalPlayer) { return; }
      int slot = GetSlot(dItem);
      UnequipItem(slot);
      if (slot < 2) {
        dWeapon dWeapon = dItem as dWeapon;
        if (slot == 0) {
          if (dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(WeaponSlot(1)); }
        }
        else if (slot == 1) {
          if (dWeapon.wType == wT.Shield) { human.anim.SetBool("Shield", true); }
          if (item[0] != 0 && GetNetworkedObject(item[0]).GetComponent<Weapon>().dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(WeaponSlot(0)); }
        }
      }
      else {
        dArmor armor = dItem as dArmor;
        human.avatar.SetSlot(armor.armorSlot.ToString(), armor.Name);
        human.avatar.BuildCharacter();
      }
      InvokeServerRpc(SpawnEquip, slot, dItem.name, OwnerClientId);
      Timer.Delay(this, dHolster, 0.05f);
      Timer.Delay(this, human.RefreshStats, 0.1f);
    }

    [ServerRPC(RequireOwnership = false)]
    public void SpawnEquip(int slot, string name, ulong clientID) {
      dItem dItem = itemSpawner.GetComponent<ItemSpawner>().GetItem(name);
      NetworkedObject spawn = Instantiate(dItem.resource).GetComponent<NetworkedObject>();
      spawn.GetComponent<NetworkedObject>().SpawnWithOwnership(clientID);
      item[slot] = spawn.GetComponent<NetworkedObject>().NetworkId;
    }

    public void UnequipItem(int slot) {
      if (!IsLocalPlayer) { return; }
      InvokeServerRpc(sUnequipItem, slot);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUnequipItem(int slot) {
      if (item[slot] == 0) { return; }
      NetworkedObject equip = GetNetworkedObject(item[slot]);
      if (slot == 1) { human.anim.SetBool("Shield", false); }
      if (equip.GetComponent<Armor>()) {
        dArmor armor = equip.GetComponent<Armor>().dArmor;
        human.avatar.ClearSlot(armor.armorSlot.ToString());
        human.avatar.BuildCharacter();
      }
      human.inventory.Store(equip.GetComponent<Item>().dItem, 1);
      equip.UnSpawn();
      Destroy(equip.gameObject);
      item[slot] = 0;
      Timer.Delay(this, human.RefreshStats, 0.1f);
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
