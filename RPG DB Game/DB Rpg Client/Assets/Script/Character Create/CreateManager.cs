using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum CharacterClass
{
    Warrior = 1,
    Mage = 2,
    Ranger = 3,
    Supporter = 4,
}

public class CreateRequest
{
    public string name;
    public string token;
    public int characterClass;
    public bool gender;
}

public class CreateResponse
{
    public bool success;
    public string message;
}

public class CreateManager : MonoBehaviour
{
    public GameObject WrongPopup;

    [Tooltip("-----캐릭터 생성-----")]
    public TMP_Text Class;
    public TMP_Text Gender;
    public TMP_InputField CharacterName;

    private bool genderIndex;
    private int classIndex;

    private string CreateUrl = "http://localhost:5000/character/create";

    // Start is called before the first frame update
    void Start()
    {
        classIndex = 1;
        genderIndex = true;
        Gender.text = "Male";

        Class.text = ((CharacterClass)classIndex).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClassLeftClick()
    {
        classIndex = --classIndex <= 0 ? 4 : classIndex;
        Class.text = ((CharacterClass)classIndex).ToString();
    }

    public void OnClassRightClick()
    {
        classIndex = ++classIndex > 4 ? 1 : classIndex;
        Class.text = ((CharacterClass)classIndex).ToString();
    }

    public void OnGenderClick()
    {
        genderIndex = !genderIndex;
        if (genderIndex)
        {
            Gender.text = "Male";
        }
        else
        {
            Gender.text = "FeMale";
        }
    }
    
    public void OnCreateClick()
    {
        if(LoginManager.IsValidName(CharacterName.text))
        {
            StartCoroutine(Create(CharacterName.text, classIndex, genderIndex));
        }
        else
        {
            ShowPopup("invalid name");
            
        }
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

    IEnumerator Create(string name, int characterClass, bool gender)
    {
        CreateRequest requestData = new CreateRequest { name = name, characterClass = characterClass, gender = gender };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(CreateUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.token);

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;

            CreateResponse response = JsonUtility.FromJson<CreateResponse>(responseText);

            if(response.success)
            {
                Debug.Log("캐릭터 생성 성공");
                SceneManager.LoadScene("SelectScene");
            }
            else
            {
                Debug.Log("캐릭터 생성 실패 : " + response.message);
                ShowPopup(response.message);
            }
        }
        else
        {
            Debug.LogError("요청 실패 : " + request.error);
        }
    }
}
