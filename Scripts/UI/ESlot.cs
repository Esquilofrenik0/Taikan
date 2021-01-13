﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace SRPG {
  public class ESlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [HideInInspector] public Item item;
    [HideInInspector] public bool empty = true;
    [HideInInspector] public Transform slotIcon;
    [HideInInspector] public Sprite background;
    [HideInInspector] public Image bgImage;
    [HideInInspector] public Hero hero;
    [HideInInspector] public int number;

    void Start() {
      bgImage = GetComponent<Image>();
      slotIcon = transform.GetChild(0);
      background = slotIcon.GetComponent<Image>().sprite;
    }

    public void UpdateSlot() {
      if (hero.equipment.item[number] != 0) {
        item = GetNetworkedObject(hero.equipment.item[number]).GetComponent<Item>();
        slotIcon.GetComponent<Image>().sprite = item.dItem.icon;
        empty = false;
      }
      else {
        item = null;
        slotIcon.GetComponent<Image>().sprite = background;
        empty = true;
      }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      if (!hero) { return; }
      if (hero.equipment.item[number] != 0) {
        item = GetNetworkedObject(hero.equipment.item[number]).GetComponent<Item>();
        int slot = 0;
        if (item.GetComponent<Weapon>()) {
          dWeapon weapon = item.dItem as dWeapon;
          slot = hero.equipment.WeaponSlot((int)weapon.weaponSlot);
        }
        else if (item.GetComponent<Armor>()) {
          dArmor armor = item.dItem as dArmor;
          slot = hero.equipment.ArmorSlot((int)armor.armorSlot);
        }
        hero.equipment.UnequipItem(slot);
      }
      UpdateSlot();
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (hero.equipment.item[number] != 0) {
        item = GetNetworkedObject(hero.equipment.item[number]).GetComponent<Item>();
        hero.hud.DisplayInfo(item.dItem);
      }
      UpdateSlot();
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetInfo();
    }
  }
}