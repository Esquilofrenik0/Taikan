﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace Postcarbon {
  public class ISlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [HideInInspector] public Hero hero;
    [HideInInspector] public int number;
    [HideInInspector] public Slot slot;
    [HideInInspector] public Image bgImage;
    [HideInInspector] public Sprite background;
    [HideInInspector] public Text textAmount;
    [HideInInspector] public GameObject slotIcon;
    [HideInInspector] public GameObject thisSlot;


    void Awake() {
      bgImage = GetComponent<Image>();
      slotIcon = transform.GetChild(0).gameObject;
      background = slotIcon.GetComponent<Image>().sprite;
      textAmount = slotIcon.GetComponentInChildren<Text>(true);
    }

    public void UpdateSlot() {
      // slot = hero.inventory.slot[number];
      slot.amount = hero.inventory.amount[number];
      slot.dItem = hero.inventory.data.GetItem(hero.inventory.item[number]);
      if (slot.amount > 0) {
        textAmount.text = slot.amount.ToString();
        if (slot.amount == 1) { textAmount.gameObject.SetActive(false); } else { textAmount.gameObject.SetActive(true); }
        slotIcon.GetComponent<Image>().sprite = slot.dItem.icon;
      }
      else {
        textAmount.text = slot.amount.ToString();
        textAmount.gameObject.SetActive(false);
        slotIcon.GetComponent<Image>().sprite = background;
      }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      // slot = hero.inventory.slot[number];
      slot.amount = hero.inventory.amount[number];
      slot.dItem = hero.inventory.data.GetItem(hero.inventory.item[number]);
      if (slot.amount > 0) {
        if (hero.containerOpen) {
          int freeSlot = hero.container.inventory.FreeSlot();
          if (freeSlot >= 0) {
            hero.container.Store(hero, number);
            Timer.Delay(this,hero.hud.cSlot[freeSlot].UpdateSlot,0.1f);
          }
        }
        else {
          if (slot.dItem.type == iT.Armor || slot.dItem.type == iT.Weapon) {
            hero.equipment.EquipItem(slot.dItem);
            hero.inventory.RemoveStack(number);
          }
        }
        Timer.Delay(this,UpdateSlot,0.1f);
      }
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      // slot = hero.inventory.slot[number];
      slot.amount = hero.inventory.amount[number];
      slot.dItem = hero.inventory.data.GetItem(hero.inventory.item[number]);
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (slot.amount > 0) { hero.hud.DisplayInfo(slot.dItem); }
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetInfo();
    }
  }
}