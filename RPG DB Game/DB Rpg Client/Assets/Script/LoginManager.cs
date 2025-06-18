using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginRequest
{
    public string email;
    public string password;
}

public class LoginResponse
{
    public bool success;
    public string token;
    public string user_name;
    public int user_id;
    public string message;
}

public class SignUpResqust
{
    public string email;
    public string password;
    public string name;
}

public class SignUpResponse
{
    public bool success;
    public string message;
}

public class LoginManager : MonoBehaviour
{
    [Tooltip("-----로그인 관련-----")]
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public GameObject WrongPopup;

    [Tooltip("-----회원가입 관련-----")]
    public GameObject SignUpWindow;
    public TMP_InputField SignUpEmail;
    public TMP_InputField SignUpPassword;
    public TMP_InputField SignUpName;

    private string LoginUrl = "http://localhost:5000/login";
    private string SignUpUrl = "http://localhost:5000/signup";

    private static readonly Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex passwordRegex = new Regex(@"^[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex nameRegex = new Regex(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);

    public void OnLoginClick()
    {
        if(IsValidEmail(EmailInput.text))
        {
            StartCoroutine(Login(EmailInput.text, PasswordInput.text));
        }
        else
        {
            ShowPopup("invalid email");
        }
    }

    public void OnSignUpClick()
    {
        if (!IsValidEmail(SignUpEmail.text))
        {
            ShowPopup("invalid email");
            
        }
        else if(!IsValidPassword(SignUpPassword.text))
        {
            ShowPopup("invalid password");
        }
        else if (!IsValidName(SignUpName.text))
        {
            ShowPopup("invalid name");
        }
        else
        {
            StartCoroutine(SignUp(SignUpEmail.text, SignUpPassword.text, SignUpName.text));
        }
    }

    public void OnOpenSignUpClick()
    {
        SignUpWindow.SetActive(true);
    }

    public void OnCloseSignUpClick()
    {
        SignUpWindow.SetActive(false);
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

    public static bool IsValidEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }
    public static bool IsValidPassword(string password)
    {
        return passwordRegex.IsMatch(password);
    }
    public static bool IsValidName(string name)
    {
        return nameRegex.IsMatch(name);
    }

    IEnumerator Login(string email, string password)
    {
        LoginRequest requestData = new LoginRequest { email = email, password = password };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(LoginUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("로그인 응답 : " + responseText);

            LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseText);
            if(response.success)
            {
                Debug.Log("로그인 성공! 유저 ID : " + response.user_name);
                GameManager.Instance.user_name = response.user_name;
                GameManager.Instance.token = response.token;

                SceneManager.LoadScene("SelectScene");
            }
            else
            {
                Debug.LogWarning("로그인 실패 : " + response.message);
                ShowPopup(response.message);
            }
        }
        else
        {
            Debug.LogError("요청 실패 : " + request.error);
        }
    }

    IEnumerator SignUp(string email, string password, string name)
    {
        SignUpResqust requestData = new SignUpResqust { email = email, password = password, name = name };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(SignUpUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");


        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("회원가입 응답 : " + responseText);

            SignUpResponse response = JsonUtility.FromJson<SignUpResponse>(responseText);
            if(response.success)
            {
                Debug.Log("회원가입 성공! ");
            }
            else
            {
                Debug.LogWarning("회원가입 실패 : " + response.message);
                ShowPopup(response.message);
            }
        }
        else
        {
            Debug.LogError("요청 실패 : " + request.error);
        }

    }
}
