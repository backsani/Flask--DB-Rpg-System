using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseNpc : MonoBehaviour
{
    private int npc_id;
    private int reward;
    public List<string> chatList;
    [SerializeField] private TextMeshPro npc_name;
    public string Name { get { return npc_name.text; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(string name, int id, int reward)
    {
        npc_id = id;
        this.reward = reward;

        npc_name.text = name;
    }

    public void InitChatList(List<string> chat)
    {
        chatList = chat;

        foreach (string chatting in chatList)
        {
            Debug.Log(chatting);
        }
    }
}
