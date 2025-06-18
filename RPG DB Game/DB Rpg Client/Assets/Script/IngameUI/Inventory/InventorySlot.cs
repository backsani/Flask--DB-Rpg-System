using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public int slotID;
    public int itemCode;
    public Image itemImage;
    public BaseItem item;

    public void Init(int id, int code)
    {
        slotID = id;
        itemCode = code;
    }

    public void Init(int code, Sprite sprite, BaseItem item)
    {
        this.item = item;
        itemCode = code;
        itemImage.sprite = sprite;
    }
}
