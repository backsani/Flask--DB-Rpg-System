using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BaseCharacter : MonoBehaviour
{
    public CharacterInfo playerInfo;
    public Camera mCamera;

    [Tooltip("-----로그인 관련-----")]
    public float maxHp;
    public float currentHp;
    public float atk;
    public float matk;
    public float def;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void Init(float hp, float atk, float matk, float def, float speed)
    {
        maxHp = hp;
        currentHp = maxHp - 20.0f;
        this.atk = atk;
        this.matk = matk;
        this.def = def;
        this.speed = speed;

        playerInfo = GameManager.Instance.CharacterInfo;

        mCamera = Camera.main;
        mCamera.transform.SetParent(transform);
        mCamera.GetComponent<MouseLook>().playerBody = transform;

        mCamera.transform.position = new Vector3(0, 1f, 0);
    }

    private void Move()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDir += transform.forward; // 보는 방향 기준 앞으로
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += transform.right;
        }

        transform.position += moveDir.normalized * speed * Time.deltaTime * 0.5f;
    }
}
