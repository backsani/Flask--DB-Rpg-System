using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPotion : BaseItem
{
    public override void UseItem()
    {
        CharacterUIManager.Instance.player.currentHp += 15f;
        CharacterUIManager.Instance.SetHp();
    }

    // Start is called before the first frame update
    void Start()
    {
        itemCode = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
