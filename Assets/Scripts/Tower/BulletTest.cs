using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTest : MonoBehaviour
{
    protected float bullspeed = 10.0f;
    protected float bullDamage = 5.0f;

    private Vector3 Target;

    private void Update()
    {
        if (Target != null)
        {
            Vector3 target = Target - this.transform.position;
            this.transform.position += target.normalized * bullspeed * Time.deltaTime;
        }
    }


 public Vector3 SetTarget
    {
        set
        {
            Target = value;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(this.gameObject);
            other.GetComponent<Enemy>().EnemyDie(bullDamage);
        }
    }
}