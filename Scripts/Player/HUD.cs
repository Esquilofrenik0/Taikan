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
    // [HideInInspector] public ESlot[] eSlot;
    [HideInInspector] public ASlot[] aSlot;
    [HideInInspector] public WSlot[] wSlot;
    [HideInInspector] public ISlot[] iSlot;
    [HideInInspector] public RSlot[] rSlot;
    [HideInInspector] public Text[] recipeInfo;
    [HideInInspector] public Text[] stats;
    [HideInInspector] public Text[] itemInfo;
    [HideInInspector] public Text worldInfo;
    [HideInInspector] public Text ammoText;
    [HideInInspector] public GameObject inventoryUI;
    [HideInInspector] public GameObject containerUI;
    [HideInInspector] public GameObject craftingUI;
    [HideInInspector] public int maxSlots = 64;
    [HideInInspector] public Coroutine clearWorldInfo;
    [HideInInspector] public Database data;

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
      for (int i = 0; i < wSlot.Length; i++) { wSlot[i].UpdateSlot(); }
      for (int i = 0; i < aSlot.Length; i++) { aSlot[i].UpdateSlot(); }
      RefreshAmmo();
    }

    public void RefreshAmmo() {
      if (hero.equipment.weapon[0] is dGun) {
        dGun dGun = hero.equipment.weapon[0] as dGun;
        ammoText.text = dGun.clipAmmo + " / " + dGun.totalAmmo + " ";
      }
      else { ammoText.text = "0 / 0 "; }
    }

    public void initHUD() {
      if (!IsLocalPlayer) { return; }
      data = GameObject.FindObjectOfType<Database>();
      maxSlots = 64;
      hero = gameObject.GetComponent<Hero>();
      GameObject.Find("Canvas").transform.Find("HUD").gameObject.SetActive(true);
      inventoryUI = GameObject.Find("Canvas").transform.Find("HUD/InventoryUI").gameObject;
      inventoryUI.SetActive(true);
      initBars();
      initBag();
      initEquipment();
      initRecipe();
      containerUI = GameObject.Find("HUD/InventoryUI/ContainerUI");
      containerUI.SetActive(true);
      initContainer();
      recipeInfo = new Text[8];
      itemInfo = new Text[8];
      stats = new Text[5];
      for (int i = 0; i < itemInfo.Length; i++) {
        recipeInfo[i] = GameObject.Find("HUD/InventoryUI/RecipeUI/RecipeInfo").transform.GetChild(i).GetComponent<Text>();
        itemInfo[i] = GameObject.Find("HUD/InventoryUI/BagUI/Info").transform.GetChild(i).GetComponent<Text>();
      }
      for (int i = 0; i < stats.Length; i++) {
        stats[i] = GameObject.Find("HUD/InventoryUI/EquipmentUI/Stats").transform.GetChild(i).GetComponent<Text>();
      }
      inventoryUI.SetActive(false);
      worldInfo = GameObject.Find("HUD/WorldInfo").GetComponent<Text>();
      worldInfo.gameObject.SetActive(false);
      ammoText = GameObject.Find("HUD/AmmoText").GetComponent<Text>();
    }

    public void WriteWorldInfo(string text) {
      if (!IsLocalPlayer) { return; }
      worldInfo.gameObject.SetActive(true);
      worldInfo.text += "\n" + text;
      Timer.rDelay(this, ClearWorldInfo, 5f, clearWorldInfo);
    }

    public void ClearWorldInfo() {
      if (!IsLocalPlayer) { return; }
      worldInfo.text = null;
      worldInfo.gameObject.SetActive(false);
    }

    public void initRecipe() {
      if (!IsLocalPlayer) { return; }
      rSlot = new RSlot[maxSlots];
      for (int i = 0; i < maxSlots; i++) {
        rSlot[i] = GameObject.Find("HUD/InventoryUI/RecipeUI").transform.GetChild(i).GetComponent<RSlot>();
        rSlot[i].gameObject.SetActive(true);
        rSlot[i].number = i;
        rSlot[i].hero = hero;
        rSlot[i].textAmount.gameObject.SetActive(false);
        if (i < data.recipes.Count) { rSlot[i].UpdateSlot(); }
        else { rSlot[i].gameObject.SetActive(false); }
      }
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
      float healthPercent = hero.stats.health.Value / hero.stats.maxHealth;
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
      wSlot = new WSlot[4];
      wSlot[0] = GameObject.Find("HUD/InventoryUI/EquipmentUI/RightHandSlot").GetComponent<WSlot>();
      wSlot[1] = GameObject.Find("HUD/InventoryUI/EquipmentUI/LeftHandSlot").GetComponent<WSlot>();
      wSlot[2] = GameObject.Find("HUD/InventoryUI/EquipmentUI/RightOffHandSlot").GetComponent<WSlot>();
      wSlot[3] = GameObject.Find("HUD/InventoryUI/EquipmentUI/LeftOffHandSlot").GetComponent<WSlot>();
      for (int i = 0; i < wSlot.Length; i++) {
        wSlot[i].hero = hero;
        wSlot[i].number = i;
      }
      aSlot = new ASlot[5];
      aSlot[0] = GameObject.Find("HUD/InventoryUI/EquipmentUI/HelmetSlot").GetComponent<ASlot>();
      aSlot[1] = GameObject.Find("HUD/InventoryUI/EquipmentUI/ChestSlot").GetComponent<ASlot>();
      aSlot[2] = GameObject.Find("HUD/InventoryUI/EquipmentUI/HandsSlot").GetComponent<ASlot>();
      aSlot[3] = GameObject.Find("HUD/InventoryUI/EquipmentUI/LegsSlot").GetComponent<ASlot>();
      aSlot[4] = GameObject.Find("HUD/InventoryUI/EquipmentUI/FeetSlot").GetComponent<ASlot>();
      for (int i = 0; i < aSlot.Length; i++) {
        aSlot[i].hero = hero;
        aSlot[i].number = i;
      }
    }

    public void DisplayStats() {
      if (!IsLocalPlayer) { return; }
      stats[0].text = "Health: " + hero.stats.maxHealth + " (+" + hero.stats.healthRegen + "/s)";
      stats[1].text = "Stamina: " + hero.maxStamina + " (+" + hero.staminaRegen + "/s)";
      stats[2].text = "Mana: " + hero.maxMana + " (+" + hero.manaRegen + "/s)";
      stats[3].text = "Damage: " + hero.stats.damage.Value;
      stats[4].text = "Defense: " + hero.stats.defense.Value;
    }

    public void DisplayRecipeInfo(dRecipe recipe) {
      if (!IsLocalPlayer) { return; }
      recipeInfo[0].text = recipe.result.dItem.Name;
      for (int i = 0; i < recipe.cost.Count; i++) {
        recipeInfo[i + 1].text = recipe.cost[i].amount + " x " + recipe.cost[i].dItem.Name;
      }
    }

    public void ResetRecipeInfo() {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < recipeInfo.Length; i++) {
        recipeInfo[i].text = "";
      }
    }

    public void DisplayItemInfo(dItem dItem) {
      if (!IsLocalPlayer) { return; }
      itemInfo[0].text = "Name: " + dItem.Name;
      // itemInfo[1].text = "Type: " + dItem.type;
      if (dItem is dArmor) {
        dArmor dArmor = dItem as dArmor;
        itemInfo[2].text = "Slot: " + dArmor.armorSlot;
        itemInfo[3].text = "Defense: " + dArmor.defense;
        itemInfo[4].text = "Durability: " + dArmor.durability;
      }
      else if (dItem is dConsumable) {
        dConsumable dConsumable = dItem as dConsumable;
        itemInfo[2].text = "Health: " + dConsumable.hRestore;
        itemInfo[3].text = "Stamina: " + dConsumable.sRestore;
        itemInfo[4].text = "Mana: " + dConsumable.mRestore;
      }
      else if (dItem is dWeapon) {
        dWeapon dWeapon = dItem as dWeapon;
        if (dWeapon is dShield) {
          dShield dShield = dWeapon as dShield;
          itemInfo[2].text = "Slot: " + dShield.weaponSlot;
          itemInfo[3].text = "Defense: " + dShield.defense;
          itemInfo[4].text = "Durability: " + dShield.durability;
        }
        else {
          itemInfo[2].text = "Slot: " + dWeapon.weaponSlot;
          itemInfo[3].text = "Damage: " + dWeapon.damage;
          itemInfo[4].text = "Durability: " + dWeapon.durability;
        }
      }
      itemInfo[5].text = "Value: " + dItem.value;
      itemInfo[6].text = "Weight: " + dItem.weight;
      itemInfo[7].text = "Description: " + dItem.description;
    }

    public void ResetInfo() {
      if (!IsLocalPlayer) { return; }
      for (int i = 0; i < itemInfo.Length; i++) {
        itemInfo[i].text = "";
      }
    }
  }
}
