using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageExplosion : MonoBehaviour
{
    public float damage;

    public float lifeTime;
    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if(lifeTime < 0 )
            Destroy(gameObject);
    }
}
