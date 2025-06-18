using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class WorldInfo
{
    public int id;
    public string world_name;
    public string world_explan;
    public int open_level;
}

[System.Serializable]
public class WorldListResponse
{
    public bool success;
    public string message;
    public WorldInfo[] worlds;
}

public class WorldManager : MonoBehaviour
{
    public GameObject WrongPopup;
    private string worldListUrl = "http://localhost:5000/map/list";

    private int StartWorldId;
    [SerializeField] private TMP_Text StartWorldName;
    [SerializeField] private TMP_Text StartWorldExplan;
    private int startWorldLevel;

    private int MiddleWorldId;
    [SerializeField] private TMP_Text MiddleWorldName;
    [SerializeField] private TMP_Text MiddleWorldExplan;
    private int middleWorldLevel;

    private int EndWorldId;
    [SerializeField] private TMP_Text EndWorldName;
    [SerializeField] private TMP_Text EndWorldExplan;
    private int endWorldLevel;

    // Start is called before the first frame update
    void Start()
    {
        CheckLastConnect();
        StartCoroutine(WorldLoad());
    }

    public void ShowPopup(string message)
    {
        WrongPopup.transform.GetChild(1).GetComponent<TMP_Text>().text = message;
        WrongPopup.SetActive(true);
        Invoke(nameof(HidePopup), 1f);
    }

    public void HidePopup()
    {
        WrongPopup.SetActive(false);
    }

    public void CheckLastConnect()
    {
        int worldId = GameManager.Instance.lastWorld;
        if (worldId == 0)
            return;

        if(worldId == 1)
        {
            GameManager.Instance.CurrentWorldId = 1;
            SceneManager.LoadScene("StartWorld");
        }
        else if (worldId == 2)
        {
            GameManager.Instance.CurrentWorldId = 2;
            SceneManager.LoadScene("MiddleWorld");
        }
        else if (worldId == 3)
        {
            GameManager.Instance.CurrentWorldId = 3;
            SceneManager.LoadScene("EndWorld");
        }
    }
        
    public void OnStartWorldClick()
    {
        Debug.Log("startWorld");
        if(startWorldLevel > GameManager.Instance.CharacterInfo.character_level)
        {
            ShowPopup("레벨이 낮습니다.");
        }
        else
        {
            GameManager.Instance.CurrentWorldId = StartWorldId;
            GameManager.Instance.lastWorld = StartWorldId;
            SceneManager.LoadScene("StartWorld");
        }
    }

    public void OnMiddleWorldClick()
    {
        Debug.Log("MiddleWorld");
        if (middleWorldLevel > GameManager.Instance.CharacterInfo.character_level)
        {
            ShowPopup("레벨이 낮습니다.");

        }
        else
        {
            GameManager.Instance.CurrentWorldId = MiddleWorldId;
            GameManager.Instance.lastWorld = MiddleWorldId;
            SceneManager.LoadScene("MiddleWorld");
        }
    }

    public void OnEndWorldClick()
    {
        Debug.Log("endWorld");
        if (endWorldLevel > GameManager.Instance.CharacterInfo.character_level)
        {
            ShowPopup("레벨이 낮습니다.");

        }
        else
        {
            GameManager.Instance.CurrentWorldId = EndWorldId;
            GameManager.Instance.lastWorld = EndWorldId;
            SceneManager.LoadScene("EndWorld");
        }
    }

    IEnumerator WorldLoad()
    {
        UnityWebRequest request = UnityWebRequest.Get(worldListUrl);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("맵 리스트 응답: " + responseText);

            WorldListResponse response = JsonUtility.FromJson<WorldListResponse>(responseText);

            if (response.success)
            {
                foreach (WorldInfo world in response.worlds)
                {
                    switch (world.id)
                    {
                        case 1:
                            StartWorldId = world.id;
                            StartWorldName.text = world.world_name;
                            StartWorldExplan.text = world.world_explan;
                            startWorldLevel = world.open_level;
                            break;
                        case 2:
                            MiddleWorldId = world.id;
                            MiddleWorldName.text = world.world_name;
                            MiddleWorldExplan.text = world.world_explan;
                            middleWorldLevel = world.open_level;
                            break;
                        case 3:
                            EndWorldId = world.id;
                            EndWorldName.text = world.world_name; 
                            EndWorldExplan.text = world.world_explan;
                            endWorldLevel = world.open_level;
                            break;
                        default:
                            break;
                    }
                }


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
}
