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

//=====캐릭터 스텟 능력치 값=====
public class StatData
{
    public float start;
    public float perStat;
}

public class CharacterStatInfo
{
    public Dictionary<string, StatData> stats = new Dictionary<string, StatData>();
}

//=====캐릭터 스텟 값=====
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

//=====마지막 정보=====
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

    [Tooltip("-----무기 프리팹-----")]
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

    //캐릭터의 시작 능력치와 스텟 별 능력치를 불러온다.
    IEnumerator DownloadCSV(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        Debug.Log(url);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Character CSV 다운로드 실패: " + www.error);
        }
        else
        {
            string csvText = www.downloadHandler.text;
            info = ParseCSV(csvText);

            statLoad = true;
            TryCalculateStat();
        }
    }

    //레벨업 시 필요한 경험치 정보를 csv에서 불러온다
    IEnumerator DownloadXpCSV()
    {
        UnityWebRequest www = UnityWebRequest.Get(xpStatCSVurl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Character CSV 다운로드 실패: " + www.error);
        }
        else
        {
            string csvText = www.downloadHandler.text;
            ParseXpCSV(csvText);

            xpLoad = true;
            TryCalculateStat();
        }
    }

    //캐릭터 스텟을 DB에서 불러온다
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
            Debug.Log("캐릭터 스텟 응답 : " + responseText);

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
                Debug.LogWarning("실패: " + response.message);
            }
        }
        else
        {
            Debug.LogError("요청 실패: " + request.error);
        }
    }

    //불러온 캐릭터 능력치 정보를 추출한다.
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

    //불러온 경험치 정보를 추출한다.
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

    //캐릭터의 능력치를 계산해 실질적 능력치로 변환
    void TryCalculateStat()
    {
        //모든 로드가 끝났는지 체크한 후 끝났다면 실행한다.
        if (statLoad && dbLoad && xpLoad)
        {

            if(info == null)
            {
                Debug.LogError("캐릭터 데이터 적용 실패 : DB");
            }
            if(stat == null)
            {
                Debug.LogError("캐릭터 데이터 적용 실패 : csv");
            }

            //실제 능력치를 계산하는 과정
            float CHp = info.stats["hp"].start + info.stats["hp"].perStat * stat.hp;
            float CAtk = info.stats["atk"].start + info.stats["atk"].perStat * stat.atk;
            float CMatk = info.stats["matk"].start + info.stats["matk"].perStat * stat.matk;
            float CDef = info.stats["def"].start + info.stats["def"].perStat * stat.def;
            float CSpeed = info.stats["speed"].start + info.stats["speed"].perStat * stat.speed;

            //남은 스텟은 현재 레벨 - 전체 스텟
            GameManager.Instance.remainStat = (GameManager.Instance.CharacterInfo.character_level * 3) - (int)stat.hp - (int)stat.atk - (int)stat.matk - (int)stat.def - (int)stat.speed;

            //캐릭터 생성
            GameObject character = Instantiate(BasePrefab, Vector3.zero, Quaternion.identity);

            BaseCharacter baseCharacter = character.GetComponent<BaseCharacter>();

            //캐릭터 정보를 UI Manager에게 넘긴다.
            CharacterUIManager.Instance.InitPlayer(baseCharacter);
            

            //캐릭터의 종류에 맞는 무기 장착
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

            // 캐릭터에 이상이 없다면 초기화한다.
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
                Debug.Log("캐릭터 스텟 적용 실패");
            }

            StartCoroutine(LastInfoUpdate());
        }
    }

    public void CalculateStatUpdate()
    {
        //실제 능력치를 계산하는 과정
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

    // 최근 선택한 캐릭터와 월드를 데이터베이스에 저장한다.
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
