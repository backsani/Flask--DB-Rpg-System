using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    public static CharacterUIManager Instance { get; private set; }

    public List<int> xpList = new List<int>();

    public Slider hpSlider;
    public Slider xpSlider;
    public BaseCharacter player;

    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI XpText;

    private float health;
    private float xp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void InitPlayer(BaseCharacter player)
    {
        this.player = player;
        
    }
    public void InitHpBar()
    {
        hpSlider.value = 1f;
        xpSlider.value = 1f;
    }

    public void SetHp()
    {
        health = Mathf.Clamp(player.currentHp, 0f, player.maxHp);

        if(hpSlider != null )
        {
            hpSlider.value = health / player.maxHp;

            HealthText.text = (player.currentHp).ToString() + "/" + player.maxHp.ToString();
        }

        xp = Mathf.Clamp(player.playerInfo.xp, 0f, xpList[player.playerInfo.character_level - 1]);

        if(xpSlider != null )
        {
            xpSlider.value = xp / xpList[player.playerInfo.character_level - 1];

            XpText.text = player.playerInfo.character_level.ToString();
        }
    }
}
