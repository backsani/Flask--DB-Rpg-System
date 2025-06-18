using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore.Text;
using UnityEngine.XR;
using static System.Net.WebRequestMethods;

public class CharacterStatRequest
{
    public int character_id;
}

//=====ĳ���� ���� �ɷ�ġ ��=====
public class StatData
{
    public float start;
    public float perStat;
}

public class CharacterStatInfo
{
    public Dictionary<string, StatData> stats = new Dictionary<string, StatData>();
}

//=====ĳ���� ���� ��=====
[System.Serializable]
public class CharacterStatDB
{
    public string character_name;
    public float hp;
    public float atk;
    public float matk;
    public float def;
    public float speed;
}

//=====������ ����=====
[System.Serializable]
public class LastInfoUpdateRequest
{
    public int last_character_id;
    public int last_world_id;
}

public class CharacterStatDBResponse
{
    public bool success;
    public CharacterStatDB character_stat;
    public string message;
}

public class CharacterStatManager : MonoBehaviour
{
    public GameObject BasePrefab;
    public static CharacterStatManager Instance {  get; private set; }  

    public string characterStatCSVurl = "https://docs.google.com/spreadsheets/d/1tExifeVhc3fUzybXKR17y5aMIwxUdf78xTpFgQyrefo/export?format=csv&gid=";

    public string xpStatCSVurl = "https://docs.google.com/spreadsheets/d/1tExifeVhc3fUzybXKR17y5aMIwxUdf78xTpFgQyrefo/export?format=csv&gid=1246142090";


    public List<int> xpStatList = new List<int>();

    private string characterStatUrl = "http://localhost:5000/character/stat";

    public string updateLastInfoUrl = "http://localhost:5000/user/update_last_info";

    private bool statLoad;
    private bool dbLoad;
    private bool xpLoad;

    CharacterStatInfo info = null;
    CharacterStatDB stat = null;

    public GameObject ChatUI;

    [Tooltip("-----���� ������-----")]
    public GameObject Sword;
    public GameObject Bow;
    public GameObject Wand;
    public GameObject Scroll;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Init();
    }

    // Start is called before the first frame update
    public void Init()
    {
        statLoad = false;
        dbLoad = false;
        xpLoad = false;

        string fullUrl = characterStatCSVurl;

        switch ((CharacterClass)GameManager.Instance.CharacterInfo.character_class)
        {
            case CharacterClass.Warrior:
                fullUrl += "0";
                break;
            case CharacterClass.Mage:
                fullUrl += "117314805";
                break;
            case CharacterClass.Ranger:
                fullUrl += "552278626";
                break;
            case CharacterClass.Supporter:
                fullUrl += "2071071367";
                break;
            default:
                break;
        }

        StartCoroutine(DownloadCSV(fullUrl));
        StartCoroutine(DownloadXpCSV());
        StartCoroutine(CharacterStatLoad());
    }

    //ĳ������ ���� �ɷ�ġ�� ���� �� �ɷ�ġ�� �ҷ��´�.
    IEnumerator DownloadCSV(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        Debug.Log(url);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Character CSV �ٿ�ε� ����: " + www.error);
        }
        else
        {
            string csvText = www.downloadHandler.text;
            info = ParseCSV(csvText);

            statLoad = true;
            TryCalculateStat();
        }
    }

    //������ �� �ʿ��� ����ġ ������ csv���� �ҷ��´�
    IEnumerator DownloadXpCSV()
    {
        UnityWebRequest www = UnityWebRequest.Get(xpStatCSVurl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Character CSV �ٿ�ε� ����: " + www.error);
        }
        else
        {
            string csvText = www.downloadHandler.text;
            ParseXpCSV(csvText);

            xpLoad = true;
            TryCalculateStat();
        }
    }

    //ĳ���� ������ DB���� �ҷ��´�
    IEnumerator CharacterStatLoad()
    {
        CharacterStatRequest requestData = new CharacterStatRequest { character_id = GameManager.Instance.CharacterInfo.id };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(characterStatUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("ĳ���� ���� ���� : " + responseText);

            CharacterStatDBResponse response = JsonUtility.FromJson<CharacterStatDBResponse>(responseText);

            if(response.success)
            {
                stat = response.character_stat;

                GameManager.Instance.characterStat = stat;

                dbLoad = true;
                TryCalculateStat();
            }
            else
            {
                Debug.LogWarning("����: " + response.message);
            }
        }
        else
        {
            Debug.LogError("��û ����: " + request.error);
        }
    }

    //�ҷ��� ĳ���� �ɷ�ġ ������ �����Ѵ�.
    private CharacterStatInfo ParseCSV(string csv)
    {
        string[] lines = csv.Split('\n');
        CharacterStatInfo info = new CharacterStatInfo();

        foreach (var line in lines.Skip(1))
        {
            if(string.IsNullOrEmpty(line)) continue;
            
            string[] fields = line.Trim().Split(',');

            if (fields.Length < 3) continue;

            string key = fields[0].Trim();
            float start = float.Parse(fields[1]);
            float perStat = float.Parse(fields[2]);

            info.stats[key] = new StatData { start = start, perStat = perStat };
        }

        return info;
    }

    //�ҷ��� ����ġ ������ �����Ѵ�.
    private void ParseXpCSV(string csv)
    {
        string[] lines = csv.Split('\n');

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Trim().Split(',');

            if (fields.Length < 2) continue;

            xpStatList.Add(int.Parse(fields[1]));
        }
    }

    //ĳ������ �ɷ�ġ�� ����� ������ �ɷ�ġ�� ��ȯ
    void TryCalculateStat()
    {
        //��� �ε尡 �������� üũ�� �� �����ٸ� �����Ѵ�.
        if (statLoad && dbLoad && xpLoad)
        {

            if(info == null)
            {
                Debug.LogError("ĳ���� ������ ���� ���� : DB");
            }
            if(stat == null)
            {
                Debug.LogError("ĳ���� ������ ���� ���� : csv");
            }

            //���� �ɷ�ġ�� ����ϴ� ����
            float CHp = info.stats["hp"].start + info.stats["hp"].perStat * stat.hp;
            float CAtk = info.stats["atk"].start + info.stats["atk"].perStat * stat.atk;
            float CMatk = info.stats["matk"].start + info.stats["matk"].perStat * stat.matk;
            float CDef = info.stats["def"].start + info.stats["def"].perStat * stat.def;
            float CSpeed = info.stats["speed"].start + info.stats["speed"].perStat * stat.speed;

            //���� ������ ���� ���� - ��ü ����
            GameManager.Instance.remainStat = (GameManager.Instance.CharacterInfo.character_level * 3) - (int)stat.hp - (int)stat.atk - (int)stat.matk - (int)stat.def - (int)stat.speed;

            //ĳ���� ����
            GameObject character = Instantiate(BasePrefab, Vector3.zero, Quaternion.identity);

            BaseCharacter baseCharacter = character.GetComponent<BaseCharacter>();

            //ĳ���� ������ UI Manager���� �ѱ��.
            CharacterUIManager.Instance.InitPlayer(baseCharacter);
            

            //ĳ������ ������ �´� ���� ����
            switch ((CharacterClass)GameManager.Instance.CharacterInfo.character_class)
            {
                case CharacterClass.Warrior:
                    Instantiate(Sword).transform.SetParent(character.transform.Find("Weapon"), false);
                    break;
                case CharacterClass.Mage:
                    Instantiate(Wand).transform.SetParent(character.transform.Find("Weapon"), false);
                    break;
                case CharacterClass.Ranger:
                    Instantiate(Bow).transform.SetParent(character.transform.Find("Weapon"), false);
                    break;
                case CharacterClass.Supporter:
                    Instantiate(Scroll).transform.SetParent(character.transform.Find("Weapon"), false);
                    break;
                default:
                    break;
            }

            // ĳ���Ϳ� �̻��� ���ٸ� �ʱ�ȭ�Ѵ�.
            if (baseCharacter != null)
            {
                baseCharacter.Init(CHp, CAtk, CMatk, CDef, CSpeed);
                CharacterUIManager.Instance.InitHpBar();
                CharacterUIManager.Instance.xpList = xpStatList;
                CharacterUIManager.Instance.SetHp();
                StatManager.Instance.Init();
            }
            else
            {
                Debug.Log("ĳ���� ���� ���� ����");
            }

            StartCoroutine(LastInfoUpdate());
        }
    }

    public void CalculateStatUpdate()
    {
        //���� �ɷ�ġ�� ����ϴ� ����
        float CHp = info.stats["hp"].start + info.stats["hp"].perStat * stat.hp;
        float CAtk = info.stats["atk"].start + info.stats["atk"].perStat * stat.atk;
        float CMatk = info.stats["matk"].start + info.stats["matk"].perStat * stat.matk;
        float CDef = info.stats["def"].start + info.stats["def"].perStat * stat.def;
        float CSpeed = info.stats["speed"].start + info.stats["speed"].perStat * stat.speed;

        CharacterUIManager.Instance.player.Init(CHp, CAtk, CMatk, CDef, CSpeed);
        CharacterUIManager.Instance.SetHp();
    }

    public void OnChat()
    {
        ChatUI.SetActive(true);
    }

    public void OffChat()
    {
        ChatUI.SetActive(false);
    }

    // �ֱ� ������ ĳ���Ϳ� ���带 �����ͺ��̽��� �����Ѵ�.
    IEnumerator LastInfoUpdate()
    {
        LastInfoUpdateRequest requestData = new LastInfoUpdateRequest
        {
            last_character_id = GameManager.Instance.CharacterInfo.id,
            last_world_id = GameManager.Instance.lastWorld
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(updateLastInfoUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Last info updated successfully");
        }
        else
        {
            Debug.LogError("Failed to update last info: " + request.error);
        }
    }
}
