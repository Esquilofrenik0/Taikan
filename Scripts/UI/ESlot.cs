using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace SRPG {
  public class ESlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Transform slotIcon;
    [HideInInspector] public dItem dItem;
    [HideInInspector] public bool empty = true;
    [HideInInspector] public Sprite background;
    [HideInInspector] public Image bgImage;
    [HideInInspector] public Hero hero;
    [HideInInspector] public int number;

    void Start() {
      bgImage = GetComponent<Image>();
      background = slotIcon.GetComponent<Image>().sprite;
    }

    public void UpdateSlot() {
      if (hero.equipment.equip[number]) {
        dItem = hero.equipment.equip[number];
        slotIcon.GetComponent<Image>().sprite = dItem.icon;
        empty = false;
      }
      else {
        dItem = null;
        slotIcon.GetComponent<Image>().sprite = background;
        empty = true;
      }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      if (!hero) { return; }
      if (hero.equipment.equip[number]) {
        dItem = hero.equipment.equip[number];
        int slot = hero.equipment.GetSlot(dItem);
        hero.equipment.UnequipItem(slot);
      }
      UpdateSlot();
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (hero.equipment.equip[number]) {
        dItem = hero.equipment.equip[number];
        hero.hud.DisplayInfo(dItem);
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