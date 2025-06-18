using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryRequest
{
    public int character_id;
}

[System.Serializable]
public class InventoryInfo
{
    public int slot_index;
    public int item_code;
}


[System.Serializable]
public class InventoryResponse
{
    public bool success;
    public InventoryInfo[] inventory;
    public string message;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance {  get; private set; }

    public GameObject statWindow;

    public InventorySlot[] inventory;

    public Sprite[] itemImages;

    private bool onStatWindow;
    
    public GameObject Settings;

    private string InventoryUrl = "http://localhost:5000/inventory/list";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        onStatWindow = false;
        StartCoroutine(InventoryLoad());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Camera.main.GetComponent<MouseLook>().ChangeUse();
            Settings.SetActive(!Settings.activeSelf);
        }

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (onStatWindow) { statWindow.SetActive(false); }
            else { statWindow.SetActive(true); }

            onStatWindow = !onStatWindow;
            Camera.main.GetComponent<MouseLook>().ChangeUse();
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            InventorySlot slot = inventory[0];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            InventorySlot slot = inventory[1];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            InventorySlot slot = inventory[2];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            InventorySlot slot = inventory[3];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            InventorySlot slot = inventory[4];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            InventorySlot slot = inventory[5];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            InventorySlot slot = inventory[6];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            InventorySlot slot = inventory[7];
            slot.item.UseItem();
            slot.Init(0, null, null);
        }

    }

    IEnumerator InventoryLoad()
    {
        InventoryRequest requestData = new InventoryRequest { character_id = GameManager.Instance.CharacterInfo.id };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(InventoryUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("인벤토리 응답 : " + responseText);

            InventoryResponse response = JsonUtility.FromJson<InventoryResponse>(responseText);

            if (response.success)
            {
                foreach(InventoryInfo info in response.inventory)
                {
                    inventory[info.slot_index - 1].Init(info.slot_index, info.item_code);
                }
            }
            else
            {
                Debug.LogWarning("로그인 실패 : " + response.message);
            }
        }
        else
        {
            Debug.LogError("요청 실패 : " + request.error);
        }
    }

    public bool AddItem(int itemCode, BaseItem item)
    {
        foreach (InventorySlot info in inventory)
        {
            if (info.itemCode != 0)
                continue;

            Sprite sprite = ItemToImage(itemCode);

            info.Init(itemCode, sprite, item);
            return true;
        }
        return false;
    }

    public Sprite ItemToImage(int itemCode)
    {
        Sprite sprite = null;
        switch (itemCode)
        {
            case 0:
                break;
            case 1:
                sprite = itemImages[itemCode];
                break;
            case 2:
                break;
            case 3:
                break;

            default:
                break;
        }

        return sprite;
    }
}
