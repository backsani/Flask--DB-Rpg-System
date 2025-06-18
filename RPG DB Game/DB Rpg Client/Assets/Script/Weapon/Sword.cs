using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapon
{
    public BoxCollider swordCollider;
    private bool attack;
    private bool attackMotion;

    public void Attack()
    {
        swordCollider.enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        attack = false;
        attackMotion = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && !attack)
        {
            attack = true;
        }
        if(attack)
        {
            if(!attackMotion)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 10f), 100f * Time.deltaTime);

                float currentZ = transform.rotation.eulerAngles.z;

                if (Mathf.Abs(Mathf.DeltaAngle(currentZ, 10f)) < 1f)
                {
                    attackMotion = true;
                    swordCollider.enabled = true;
                }
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 160f), 250f * Time.deltaTime);

                float currentZ = transform.rotation.eulerAngles.z;

                if (Mathf.Abs(Mathf.DeltaAngle(currentZ, 160f)) < 1f)
                {
                    attackMotion = false;
                    attack = false;
                    swordCollider.enabled = false;
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 60f);
                }
            }
        }
    }
}
