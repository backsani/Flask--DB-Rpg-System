using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SaveRequest
{
    public int character_id;
    public int xp;
    public int level;
    public float hp;
    public float atk;
    public float matk;
    public float def;
    public float speed;
}

public class Settings : MonoBehaviour
{
    public string saveAndExitUrl = "http://localhost:5000/character/save";

    public void OnSaveAndExitClick()
    {
        StartCoroutine(SaveAndExit());
    }

    IEnumerator SaveAndExit()
    {
        SaveRequest requestData = new SaveRequest
        {
            character_id = GameManager.Instance.CharacterInfo.id, xp = GameManager.Instance.CharacterInfo.xp, level = GameManager.Instance.CharacterInfo.character_level, hp = GameManager.Instance.characterStat.hp, atk = GameManager.Instance.characterStat.atk, matk = GameManager.Instance.characterStat.matk, def = GameManager.Instance.characterStat.def, speed = GameManager.Instance.characterStat.speed
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        Debug.Log(jsonBytes);
        Debug.Log(jsonData);

        UnityWebRequest request = new UnityWebRequest(saveAndExitUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("save Data updated successfully");
            SceneManager.LoadScene("Login");
        }
        else
        {
            Debug.LogError("Failed to update last info: " + request.error);
        }
    }
}
