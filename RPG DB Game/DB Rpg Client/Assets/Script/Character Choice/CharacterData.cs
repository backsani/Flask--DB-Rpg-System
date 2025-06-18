using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterData : MonoBehaviour
{
    public TMP_Text CharacterNameText;
    public TMP_Text GenderText;
    public TMP_Text LevelText;
    private CharacterInfo Info;

    [SerializeField] private Image characterImage;

    public void Init(CharacterInfo info, Sprite image)
    {
        Info = info;
        CharacterNameText.text = Info.character_name;
        GenderText.text = Info.gender == 1 ? "Male" : "FeMale";
        LevelText.text = Info.character_level.ToString();
        characterImage.sprite = image;
    }

    public void OnPlayClick()
    {
        GameManager.Instance.CharacterInfo = Info;

        SceneManager.LoadScene("WorldSelect");
    }
}
