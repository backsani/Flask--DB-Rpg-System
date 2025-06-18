using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Vector3 direction;
    public float speed;
    public float damage;

    public float lifeTime;
    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0 )
        {
            Destroy(gameObject);
        }
    }

    public void Init(Vector3 direction, float speed, float Damage)
    {
        this.speed = speed;
        damage = Damage;

        transform.rotation = Quaternion.LookRotation(direction);
        this.direction = Quaternion.Euler(0, 90, 0) * direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            //TODO: Damage
        }
    }
}
