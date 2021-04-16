using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OliveHealth : MonoBehaviour
{
    public float health = 2;

    void Deduct(int DamageAmount)
    {
        health -= 1;
        if (health <= 0)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>().addScore(10);
            Destroy(gameObject);
        }
    }
}
