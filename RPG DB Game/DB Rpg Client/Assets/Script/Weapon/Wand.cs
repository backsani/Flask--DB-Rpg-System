using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : MonoBehaviour
{
    public GameObject BulletPrefab;
    private float coolTime;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        coolTime = 3.0f;
        timer = coolTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0.0f) timer -= Time.deltaTime;

        if (timer <= 0.0f && Input.GetMouseButtonDown(1))
        {
            timer = coolTime;

            GameObject arrow = Instantiate(BulletPrefab, transform.position, Quaternion.identity);

            arrow.GetComponent<Arrow>().Init(transform.forward, 15.0f, 10f);

        }
    }
}
