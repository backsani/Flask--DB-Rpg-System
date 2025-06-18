using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string user_name;
    public string token;
    public CharacterInfo CharacterInfo;
    public CharacterStatDB characterStat;
    
    public LastCharacterInfo lastInfo = new LastCharacterInfo();
    public int lastWorld;

    public int CurrentWorldId;

    public int remainStat = 0;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

}
