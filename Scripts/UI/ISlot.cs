using System.Collections;
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
      slot.dItem = hero.inventory.item[number];
      if (slot.dItem) {
        textAmount.text = slot.amount.ToString();
        if (slot.amount == 1) { textAmount.gameObject.SetActive(false); }
        else { textAmount.gameObject.SetActive(true); }
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
      slot.dItem = hero.inventory.item[number];
      if (slot.dItem) {
        if (hero.containerOpen) {
          int freeSlot = hero.container.inventory.FreeSlot();
          if (freeSlot >= 0) { hero.container.Store(hero, number); }
        }
        else {
          if (slot.dItem is dArmor || slot.dItem is dWeapon) {
            hero.equipment.EquipItem(slot.dItem);
            hero.inventory.RemoveStack(number);
          }
          else if (slot.dItem is dBuildable) {
            dBuildable dBuildable = slot.dItem as dBuildable;
            hero.player.buildSystem.NewBuild(dBuildable.preview);
            hero.inventory.Remove(number, 1);
          }
        }
      }
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      // slot = hero.inventory.slot[number];
      slot.amount = hero.inventory.amount[number];
      slot.dItem = hero.inventory.item[number];
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (slot.dItem) { hero.hud.DisplayInfo(slot.dItem); }
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetInfo();
    }
  }
}