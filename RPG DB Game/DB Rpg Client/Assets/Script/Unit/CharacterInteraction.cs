using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterInteraction : MonoBehaviour
{
    [SerializeField] private BaseNpc InteractionNpc;
    private int chatCount;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (InteractionNpc == null)
                return;

            CharacterStatManager.Instance.OnChat();
            
            CharacterStatManager.Instance.ChatUI.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = InteractionNpc.Name;

            CharacterStatManager.Instance.ChatUI.transform.Find("Chat").GetComponent<TextMeshProUGUI>().text = InteractionNpc.chatList[chatCount];

            chatCount++;
            if(chatCount >= InteractionNpc.chatList.Count)
            {
                CharacterStatManager.Instance.OffChat();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("NPC"))
        {
            chatCount = 0;

            InteractionNpc = other.GetComponent<BaseNpc>();
            
        }

        if(other.CompareTag("Item"))
        {
            BaseItem item = other.GetComponent<BaseItem>();
            if (InventoryManager.Instance.AddItem(item.itemCode, item))
                Destroy(other.gameObject);
        }

        if(other.CompareTag("Xp"))
        {
            BaseCharacter player = CharacterUIManager.Instance.player;

            player.playerInfo.xp += other.GetComponent<Xp>().xp;

            int nextXp = CharacterUIManager.Instance.xpList[player.playerInfo.character_level - 1];

            if (nextXp <= player.playerInfo.xp)
            {
                player.playerInfo.xp -= nextXp;
                player.playerInfo.character_level++;
                GameManager.Instance.remainStat += 3;

                StatManager.Instance.Init();
            }

            CharacterUIManager.Instance.SetHp();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            CharacterStatManager.Instance.OffChat();

            InteractionNpc = null;
        }
    }
}
