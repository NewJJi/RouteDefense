using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BirdHitBox : MonoBehaviour,IEnumyAttacked
{
    private Enemy enemy = null;

    public void Attacked(float damage)
    {
        enemy.Attacked(damage);
    }

    private void Update()
    {
    Debug.Log(GetComponentInParent<Enemy>().gameObject.transform);
    }
        

    public Transform GetPos()
    {
        Debug.Log(GetComponentInParent<Enemy>().gameObject.transform);
        return GetComponentInParent<Enemy>().gameObject.transform;
        
    }

    private void Awake()
    {
        enemy = this.GetComponentInParent<Enemy>();
        this.gameObject.tag = "Enemy";
    }

}