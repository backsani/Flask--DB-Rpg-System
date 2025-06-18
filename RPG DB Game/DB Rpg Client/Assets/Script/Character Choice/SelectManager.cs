using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterInfo
{
    public int character_class;
    public int character_level;
    public string character_name;
    public int gender;
    public int id;
    public int xp;
}

[System.Serializable]
public class LastInfo
{
    public int last_character_id;
    public int last_world_id;
}

[System.Serializable]
public class CharacterListResponse
{
    public bool success;
    public string message;
    public CharacterInfo[] characters;
    public LastInfo last_info;
}

public class LastCharacterInfo
{
    public CharacterInfo character;
    public int last_world_id;
}

public class SelectManager : MonoBehaviour
{
    public GameObject CharacterDataPrefab;
    public GameObject CharacterListContent;

    private string characterListUrl = "http://localhost:5000/character/list";

    [SerializeField] private Sprite Warrior;
    [SerializeField] private Sprite Mage;
    [SerializeField] private Sprite Ranger;
    [SerializeField] private Sprite Supporter;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CharacterLoad());
    }

    public void OnNewCharacterClick()
    {
        SceneManager.LoadScene("CharacterCreate");
    }

    IEnumerator CharacterLoad()
    {
        UnityWebRequest request = UnityWebRequest.Get(characterListUrl);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("캐릭터 리스트 응답: " + request.downloadHandler.text);
            CharacterListResponse response = JsonUtility.FromJson<CharacterListResponse>(request.downloadHandler.text);

            if (response.success)
            {
                Debug.Log(response.last_info.last_character_id);
                Debug.Log(response.last_info.last_world_id);

                foreach (CharacterInfo c in response.characters)
                {
                    if (c.id == response.last_info.last_character_id)
                    {
                        GameManager.Instance.lastInfo.character = c;
                        GameManager.Instance.lastInfo.last_world_id = response.last_info.last_world_id;
                    }

                    Sprite temp = null;
                    switch ((CharacterClass)c.character_class)
                    {
                        case CharacterClass.Warrior:
                            temp = Warrior;
                            break;
                        case CharacterClass.Mage:
                            temp = Mage;
                            break;
                        case CharacterClass.Ranger:
                            temp = Ranger;
                            break;
                        case CharacterClass.Supporter:
                            temp = Supporter;
                            break;
                        default:
                            break;
                    }

                    GameObject data = Instantiate(CharacterDataPrefab);
                    data.transform.SetParent(CharacterListContent.transform, false);

                    data.GetComponent<CharacterData>().Init(c, temp);
                }

                
                // TODO: 캐릭터 선택 UI 띄우기
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

    public void OnLastCharacterClick()
    {
        LastCharacterInfo lastInfo = GameManager.Instance.lastInfo;
        if (lastInfo.character == null)
        {
            Debug.Log("Null");
            return;
        }
            

        GameManager.Instance.CharacterInfo = lastInfo.character;
        GameManager.Instance.CurrentWorldId = lastInfo.last_world_id;
        GameManager.Instance.lastWorld = lastInfo.last_world_id;
        SceneManager.LoadScene("WorldSelect");
    }
}