using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

namespace Postcarbon {
  public class HUD: NetworkedBehaviour {
    [HideInInspector] public Hero hero;
    [HideInInspector] private UI_Bar healthBar;
    [HideInInspector] private UI_Bar staminaBar;
    [HideInInspector] private UI_Bar manaBar;
    [HideInInspector] public CSlot[] cSlot;
    [HideInInspector] public ESlot[] eSlot;
    [HideInInspector] public ISlot[] iSlot;
    [HideInInspector] public Text[] info;
    [HideInInspector] public Text[] stats;
    [HideInInspector] public GameObject inventoryUI;
    [HideInInspector] public GameObject containerUI;
    [HideInInspector] public int maxSlots = 64;

    public void Refresh() {
      if (!IsLocalPlayer) { return; }
      if (hero.containerOpen) { RefreshContainer(); }
      RefreshEquipment();
      RefreshInventory();
    }

    public void RefreshContainer() {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < hero.container.inventory.nSlots; i++) {
        cSlot[i].UpdateSlot();
      }
    }

    public void RefreshInventory() {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < hero.inventory.nSlots; i++) {
        iSlot[i].UpdateSlot();
      }
    }

    public void RefreshEquipment() {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < eSlot.Length; i++) {
        eSlot[i].UpdateSlot();
      }
    }

    public void initHUD() {
      if (!IsLocalPlayer) { return; }
      maxSlots = 64;
      hero = gameObject.GetComponent<Hero>();
      GameObject.Find("Canvas").transform.Find("HUD").gameObject.SetActive(true);
      inventoryUI = GameObject.Find("Canvas").transform.Find("HUD/InventoryUI").gameObject;
      inventoryUI.SetActive(true);
      initBars();
      initBag();
      initEquipment();
      containerUI = GameObject.Find("HUD/InventoryUI/ContainerUI");
      containerUI.SetActive(true);
      initContainer();
      info = new Text[8];
      stats = new Text[5];
      for (int i = 0; i < info.Length; i++) {
        info[i] = GameObject.Find("HUD/InventoryUI/BagUI/Info").transform.GetChild(i).GetComponent<Text>();
      }
      for (int i = 0; i < stats.Length; i++) {
        stats[i] = GameObject.Find("HUD/InventoryUI/EquipmentUI/Stats").transform.GetChild(i).GetComponent<Text>();
      }
      inventoryUI.SetActive(false);
    }

    public void cSlots(int cSlots) {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < maxSlots; i++) {
        if (i < cSlots) { cSlot[i].gameObject.SetActive(true); } else { cSlot[i].gameObject.SetActive(false); }
      }
    }

    public void iSlots(int nSlots) {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < maxSlots; i++) {
        if (i < nSlots) { iSlot[i].gameObject.SetActive(true); } else { iSlot[i].gameObject.SetActive(false); }
      }
    }

    public void initBars() {
      if (!IsLocalPlayer) { return; }
      healthBar = GameObject.Find("HUD/HealthBar").GetComponent<UI_Bar>();
      staminaBar = GameObject.Find("HUD/StaminaBar").GetComponent<UI_Bar>();
      manaBar = GameObject.Find("HUD/ManaBar").GetComponent<UI_Bar>();
    }

    public void SetBars() {
      if (!IsLocalPlayer) { return; }
      SetHealthBar();
      SetStaminaBar();
      SetManaBar();
    }

    public void SetHealthBar() {
      if (!IsLocalPlayer) { return; }
      float healthPercent = hero.health.Value / hero.maxHealth;
      healthBar.SetPercent(healthPercent);
    }

    public void SetStaminaBar() {
      if (!IsLocalPlayer) { return; }
      float staminaPercent = hero.stamina / hero.maxStamina;
      staminaBar.SetPercent(staminaPercent);
    }

    public void SetManaBar() {
      if (!IsLocalPlayer) { return; }
      float manaPercent = hero.mana / hero.maxMana;
      manaBar.SetPercent(manaPercent);
    }

    public void initBag() {
      if (!IsLocalPlayer) { return; }
      iSlot = new ISlot[maxSlots];
      for (int i = 0; i < maxSlots; i++) {
        iSlot[i] = GameObject.Find("HUD/InventoryUI/BagUI").transform.GetChild(i).GetComponent<ISlot>();
        iSlot[i].number = i;
        iSlot[i].hero = hero;
        iSlot[i].textAmount.gameObject.SetActive(false);
        iSlot[i].gameObject.SetActive(true);
      }
      iSlots(hero.inventory.nSlots);
    }

    public void initContainer() {
      if (!IsLocalPlayer) { return; }
      cSlot = new CSlot[maxSlots];
      for (int i = 0; i < maxSlots; i++) {
        cSlot[i] = containerUI.transform.GetChild(i).GetComponent<CSlot>();
        cSlot[i].number = i;
      }
      containerUI.SetActive(false);
    }

    public void initEquipment() {
      if (!IsLocalPlayer) { return; }
      eSlot = new ESlot[7];
      eSlot[0] = GameObject.Find("HUD/InventoryUI/EquipmentUI/RightHandSlot").GetComponent<ESlot>();
      eSlot[1] = GameObject.Find("HUD/InventoryUI/EquipmentUI/LeftHandSlot").GetComponent<ESlot>();
      eSlot[2] = GameObject.Find("HUD/InventoryUI/EquipmentUI/HelmetSlot").GetComponent<ESlot>();
      eSlot[3] = GameObject.Find("HUD/InventoryUI/EquipmentUI/ChestSlot").GetComponent<ESlot>();
      eSlot[4] = GameObject.Find("HUD/InventoryUI/EquipmentUI/HandsSlot").GetComponent<ESlot>();
      eSlot[5] = GameObject.Find("HUD/InventoryUI/EquipmentUI/LegsSlot").GetComponent<ESlot>();
      eSlot[6] = GameObject.Find("HUD/InventoryUI/EquipmentUI/FeetSlot").GetComponent<ESlot>();
      for (int i = 0; i < eSlot.Length; i++) {
        eSlot[i].hero = hero;
        eSlot[i].number = i;
      }
    }

    public void DisplayStats() {
      if (!IsLocalPlayer) { return; }
      stats[0].text = "Health: " + hero.maxHealth + " (+" + hero.healthRegen + "/s)";
      stats[1].text = "Stamina: " + hero.maxStamina + " (+" + hero.staminaRegen + "/s)";
      stats[2].text = "Mana: " + hero.maxMana + " (+" + hero.manaRegen + "/s)";
      stats[3].text = "Damage: " + hero.damage.Value;
      stats[4].text = "Defense: " + hero.defense.Value;
    }

    public void DisplayInfo(dItem dItem) {
      if (!IsLocalPlayer) { return; }
      info[0].text = "Name: " + dItem.Name;
      info[1].text = "Type: " + dItem.type;
      if (dItem.type == iT.Armor) {
        dArmor dArmor = dItem as dArmor;
        info[2].text = "Slot: " + dArmor.armorSlot;
        info[3].text = "Defense: " + dArmor.defense;
        info[4].text = "Durability: " + dArmor.durability;
      }
      else if (dItem.type == iT.Consumable) {
        dConsumable dConsumable = dItem as dConsumable;
        info[2].text = "Health: " + dConsumable.hRestore;
        info[3].text = "Stamina: " + dConsumable.sRestore;
        info[4].text = "Mana: " + dConsumable.mRestore;
      }
      else if (dItem.type == iT.Weapon) {
        dWeapon dWeapon = dItem as dWeapon;
        if (dWeapon.wType != wT.Shield) {
          info[2].text = "Slot: " + dWeapon.weaponSlot;
          info[3].text = "Damage: " + dWeapon.damage;
          info[4].text = "Durability: " + dWeapon.durability;
        }
        else {
          info[2].text = "Slot: " + dWeapon.weaponSlot;
          info[3].text = "Defense: " + dWeapon.defense;
          info[4].text = "Durability: " + dWeapon.durability;
        }
      }
      info[5].text = "Value: " + dItem.value;
      info[6].text = "Weight: " + dItem.weight;
      info[7].text = "Description: " + dItem.description;
    }

    public void ResetInfo() {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < info.Length; i++) {
        info[i].text = "";
      }
    }
  }
}
