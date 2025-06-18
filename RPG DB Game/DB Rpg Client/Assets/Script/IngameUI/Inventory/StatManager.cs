using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    public TextMeshProUGUI HpText;
    public TextMeshProUGUI AtkText;
    public TextMeshProUGUI MatkText;
    public TextMeshProUGUI DefText;
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI StatText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Init()
    {
        HpText.text = "hp : " + CharacterUIManager.Instance.player.maxHp.ToString();
        AtkText.text = "atk : " + CharacterUIManager.Instance.player.atk.ToString();
        MatkText.text = "matk : " + CharacterUIManager.Instance.player.matk.ToString();
        DefText.text = "def : " + CharacterUIManager.Instance.player.def.ToString();
        SpeedText.text = "speed : " + CharacterUIManager.Instance.player.speed.ToString();
        StatText.text = "³²´Â ½ºÅÝ : " + GameManager.Instance.remainStat.ToString();

        transform.gameObject.SetActive(false);
    }

    public void OnHpUpgrade()
    {
        Debug.Log(GameManager.Instance.remainStat);

        if(GameManager.Instance.remainStat > 0)
        {
            GameManager.Instance.remainStat--;
            GameManager.Instance.characterStat.hp++;

            CharacterStatManager.Instance.CalculateStatUpdate();

            HpText.text = "hp : " + CharacterUIManager.Instance.player.maxHp.ToString();
            StatText.text = "³²´Â ½ºÅÝ : " + GameManager.Instance.remainStat.ToString();
        }
    }
    public void OnAtkUpgrade()
    {
        if (GameManager.Instance.remainStat > 0)
        {
            GameManager.Instance.remainStat--;
            GameManager.Instance.characterStat.atk++;

            CharacterStatManager.Instance.CalculateStatUpdate();

            AtkText.text = "atk : " + CharacterUIManager.Instance.player.atk.ToString();
            StatText.text = "³²´Â ½ºÅÝ : " + GameManager.Instance.remainStat.ToString();
        }
    }
    public void OnMatkUpgrade()
    {
        if (GameManager.Instance.remainStat > 0)
        {
            GameManager.Instance.remainStat--;
            GameManager.Instance.characterStat.matk++;

            CharacterStatManager.Instance.CalculateStatUpdate();

            MatkText.text = "matk : " + CharacterUIManager.Instance.player.matk.ToString();
            StatText.text = "³²´Â ½ºÅÝ : " + GameManager.Instance.remainStat.ToString();
        }
    }
    public void OnDefUpgrade()
    {
        if (GameManager.Instance.remainStat > 0)
        {
            GameManager.Instance.remainStat--;
            GameManager.Instance.characterStat.def++;

            CharacterStatManager.Instance.CalculateStatUpdate();

            DefText.text = "def : " + CharacterUIManager.Instance.player.def.ToString();
            StatText.text = "³²´Â ½ºÅÝ : " + GameManager.Instance.remainStat.ToString();
        }
    }
    public void OnSpeedUpgrade()
    {
        if (GameManager.Instance.remainStat > 0)
        {
            GameManager.Instance.remainStat--;
            GameManager.Instance.characterStat.speed++;

            CharacterStatManager.Instance.CalculateStatUpdate();

            SpeedText.text = "speed : " + CharacterUIManager.Instance.player.speed.ToString();
            StatText.text = "³²´Â ½ºÅÝ : " + GameManager.Instance.remainStat.ToString();
        }
    }
}
