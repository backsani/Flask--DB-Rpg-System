using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class NpcListRequest
{
    public int worldId;
}

//=====NPC 스텟 값=====
[System.Serializable]
public class NpcStat
{
    public int id;
    public int pos_x;
    public int pos_y;
    public int pos_z;
    public string npc_name;
    public int reward;
}

[System.Serializable]
public class NpcListResponse
{
    public bool success;
    public NpcStat[] npcStat;

    public string message;
}

public class NPCManager : MonoBehaviour
{
    public GameObject NpcPrefab;

    private string npcLoadUrl = "http://localhost:5000/npc/list";

    public string NpcCSVurl = "https://docs.google.com/spreadsheets/d/1tExifeVhc3fUzybXKR17y5aMIwxUdf78xTpFgQyrefo/export?format=csv&gid=837659244";

    Dictionary<string, NpcStat> npcInfo = new Dictionary<string, NpcStat>();

    Dictionary<string, List<string>> chatList = new Dictionary<string, List<string>>();

    private bool dbLoad;
    private bool csvLoad;

    // Start is called before the first frame update
    void Start()
    {
        dbLoad = false;
        csvLoad = false;
        StartCoroutine(NpcLoad());
        StartCoroutine(NpcChatCSV());
    }


    IEnumerator NpcLoad()
    {
        NpcListRequest requestData = new NpcListRequest { worldId = GameManager.Instance.CurrentWorldId };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(npcLoadUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("NPC 리스트 응답: " + responseText);

            NpcListResponse response = JsonUtility.FromJson<NpcListResponse>(responseText);

            if (response.success)
            {
                foreach(NpcStat npc in response.npcStat)
                {
                    npcInfo[npc.npc_name] = npc;
                }
                dbLoad = true;

                TryCalculateStat();
            }
            else
            {
                Debug.LogWarning("실패: " + response.message);
            }
        }
        else
        {
            Debug.LogError("요청 실패: " + request.error);
        }
    }

    IEnumerator NpcChatCSV()
    {
        UnityWebRequest www = UnityWebRequest.Get(NpcCSVurl);
        yield return www.SendWebRequest();

        if(www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Npc CSV 다운로드 실패: " + www.error);
            Debug.LogError($"Npc CSV 다운로드 실패: {www.responseCode} {www.error}\nURL: {NpcCSVurl}");
        }
        else
        {
            string csvText = www.downloadHandler.text;
            ParseCSV(csvText);

            csvLoad = true;
            TryCalculateStat();
            
        }
    }

    private void ParseCSV(string csv)
    {
        string[] lines = csv.Split('\n');

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Trim().Split(',');

            if (fields.Length < 4) continue;

            int id = int.Parse(fields[0]);
            string name = fields[1];
            string text = fields[3];

            if (!chatList.ContainsKey(name))
            {
                chatList[name] = new List<string>();
            }
            chatList[name].Add(text);
        }
    }

    void TryCalculateStat()
    {
        if (csvLoad && dbLoad)
        {
            Debug.Log("양쪽 데이터 다 받아옴 → 합산 실행");

            foreach(KeyValuePair<string, NpcStat> npc in npcInfo)
            {
                GameObject npcObject = Instantiate(NpcPrefab, new Vector3(npc.Value.pos_x, npc.Value.pos_y, npc.Value.pos_z), Quaternion.identity);

                BaseNpc baseNpc = npcObject.GetComponent<BaseNpc>();
                baseNpc.Init(npc.Value.npc_name, npc.Value.id, npc.Value.reward);

                baseNpc.InitChatList(chatList[npc.Value.npc_name]);
            }

            
        }
    }
}
