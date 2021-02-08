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
    public List<Transform> holsterSlot = new List<Transform>() { null, null };
    public List<Transform> weaponSlot = new List<Transform>() { null, null };
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    public List<Weapon> weaponObject = new List<Weapon> { null, null };
    public NetworkedList<dArmor> armor = new NetworkedList<dArmor>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<dArmor> { null, null, null, null, null });
    public NetworkedList<dWeapon> weapon = new NetworkedList<dWeapon>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<dWeapon> { null, null, null, null });
    [HideInInspector] public Coroutine holsterRoutine;
    [HideInInspector] public Coroutine refreshRoutine;

    void armorChanged(NetworkedListEvent<dArmor> changeEvent) {
      if (armor[changeEvent.index] == null) { UndressArmor(changeEvent.index); }
      else { DressArmor(armor[changeEvent.index]); }
      humanoid.RefreshStats();
      if (IsLocalPlayer && GetComponent<HUD>() && GetComponent<HUD>().hero) {
        GetComponent<HUD>().Refresh();
        GetComponent<HUD>().DisplayStats();
      }
    }

    void weaponChanged(NetworkedListEvent<dWeapon> changeEvent) {
      if (weapon[changeEvent.index] == null) { UndressWeapon(changeEvent.index); }
      else {
        DressWeapon(changeEvent.index, weapon[changeEvent.index]);
        UpdateWeaponSlot(changeEvent.index, weapon[changeEvent.index]);
      }
      humanoid.RefreshStats();
      if (IsLocalPlayer && GetComponent<HUD>() && GetComponent<HUD>().hero) {
        GetComponent<HUD>().Refresh();
        GetComponent<HUD>().DisplayStats();
      }
    }

    void holsterChanged(bool prevHolster, bool newHolster) {
      Holster(newHolster);
    }

    public override void NetworkStart() {
      base.NetworkStart();
      armor.OnListChanged += armorChanged;
      weapon.OnListChanged += weaponChanged;
      holstered.OnValueChanged += holsterChanged;
      if (IsServer) { for (int i = 0; i < initItems.Count; i++) { EquipItem(Instantiate(initItems[i])); } }
      humanoid.RefreshStats();
    }

    public void init() {
      for (int i = 0; i < armor.Count; i++) { if (armor[i]) { DressArmor(armor[i]); } }
      for (int i = 0; i < weapon.Count; i++) { if (weapon[i]) { DressWeapon(i, weapon[i]); } }
    }

    public void Holster(bool equipped) {
      for (int i = 0; i < weaponObject.Count; i++) {
        if (weaponObject[i]) {
          if (i < 2) {
            if (equipped) { weaponObject[i].transform.SetParent(weaponSlot[i]); }
            else { weaponObject[i].transform.SetParent(holsterSlot[i]); }
          }
          else { weaponObject[i].transform.SetParent(weaponSlot[i]); }
          weaponObject[i].transform.localPosition = Vector3.zero;
          weaponObject[i].transform.localRotation = Quaternion.identity;
        }
      }
    }

    public void DressArmor(dArmor dArmor) {
      humanoid.avatar.SetSlot(dArmor.armorSlot.ToString(), dArmor.Name);
      humanoid.avatar.BuildCharacter();
    }

    public void UndressArmor(int slot) {
      aS armorSlot = (aS)(slot);
      humanoid.avatar.ClearSlot(armorSlot.ToString());
      humanoid.avatar.BuildCharacter();
    }

    public void DressWeapon(int slot, dWeapon dWeapon) {
      if (weaponObject[slot] == null) {
        SpawnWeapon(slot, dWeapon);
      }
    }

    public void UndressWeapon(int slot) {
      if (weaponObject[slot] != null) {
        DestroyWeapon(slot);
      }
    }

    public void SpawnWeapon(int slot, dWeapon dWeapon) {
      GameObject spawn;
      if (!holstered.Value && slot < 2) { spawn = Instantiate(dWeapon.resource, holsterSlot[slot]); }
      else { spawn = Instantiate(dWeapon.resource, weaponSlot[slot]); }
      weaponObject[slot] = spawn.GetComponent<Weapon>();
      weaponObject[slot].SetData(weapon[slot]);
    }

    public void DestroyWeapon(int slot) {
      if (slot == 0) { humanoid.anim.SetInteger("Weapon", 0); }
      else if (slot == 1) { humanoid.anim.SetBool("Shield", false); }
      Destroy(weaponObject[slot].gameObject);
      weaponObject[slot] = null;
    }

    public void EquipItem(dItem dItem) {
      if (dItem is dArmor) {
        dArmor dArmor = dItem as dArmor;
        armor[ArmorSlot(dArmor)] = dArmor;
      }
      else if (dItem is dWeapon) {
        dWeapon dWeapon = dItem as dWeapon;
        int slot = WeaponSlot(dWeapon);
        ClearWeaponSlot(slot, dWeapon);
        weapon[slot] = dWeapon;
      }
    }

    public void UnequipArmor(int slot) {
      if (armor[slot] == null) { return; }
      if (GetComponent<Inventory>()) { GetComponent<Inventory>().Store(armor[slot], 1); }
      armor[slot] = null;
    }

    public void UnequipWeapon(int slot) {
      if (weapon[slot] == null) { return; }
      if (GetComponent<Inventory>()) {
        Inventory inventory = GetComponent<Inventory>();
        inventory.Store(weapon[slot], 1);
        if (slot == 0 || slot == 2) {
          if (weapon[slot] is dGun) {
            dGun dGun = weapon[slot] as dGun;
            if (dGun.dAmmo != null) {
              GetComponent<Inventory>().Store(dGun.dAmmo, dGun.clipAmmo + dGun.totalAmmo);
              dGun.dAmmo = null;
              dGun.clipAmmo = 0;
              dGun.totalAmmo = 0;
            }
          }
        }
      }
      weapon[slot] = null;
    }

    public void ClearWeaponSlot(int slot, dWeapon dWeapon) {
      UnequipWeapon(slot);
      if (slot == 0) { if (dWeapon.weaponSlot == wS.TwoHand) { UnequipWeapon(1); } }
      else if (slot == 1) { if (weaponObject[0] && weaponObject[0].dWeapon.weaponSlot == wS.TwoHand) { UnequipWeapon(0); } }
    }

    public void UpdateWeaponSlot(int slot, dWeapon dWeapon) {
      if (slot == 0) { humanoid.anim.SetInteger("Weapon", (int)dWeapon.wType); }
      else if (slot == 1) { if (dWeapon is dShield) { humanoid.anim.SetBool("Shield", true); } }
    }

    public int WeaponSlot(dWeapon dWeapon) {
      int slot = (int)dWeapon.weaponSlot;
      if (slot == 2 || slot == 3) { slot = 0; }
      return slot;
    }

    public int ArmorSlot(dArmor dArmor) {
      int slot = (int)dArmor.armorSlot;
      return slot;
    }
  }
}
