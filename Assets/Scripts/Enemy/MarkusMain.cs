using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkusMain : MonoBehaviour
{
    public GameObject Player;
    public Transform shotPos;
    public GameObject bullet;
    public float shotForce = 800;
    bool reloaded;
    public Animation shotAnim;
    public float health = 100;

    void Start()
    {
        reloaded = false;
        StartCoroutine(reloading());
    }

    void Deduct(int DamageAmount)
    {
        health -= DamageAmount;
        if (health <= 0)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>().addScore(20);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPostition = new Vector3(Player.transform.position.x,
                                        this.transform.position.y,
                                        Player.transform.position.z);
        this.transform.LookAt(targetPostition);

        RaycastHit hit;
        Vector3 rayDirection = Player.transform.position - shotPos.position;
        if (Physics.Raycast(shotPos.position, rayDirection, out hit))
        {
            if (hit.transform.CompareTag("Player"))
            {
                if (reloaded)
                {
                    reloaded = false;
                    StartCoroutine(reloading());
                    GameObject bul = Instantiate(bullet, shotPos.position, Quaternion.identity) as GameObject;
                    bul.GetComponent<Rigidbody>().AddForce((Player.transform.position - shotPos.transform.position).normalized * shotForce);
                    Destroy(bul, 4);
                    shotAnim.Play();
                }
            }
        }
    }

    IEnumerator reloading()
    {
        yield return new WaitForSeconds(2);
        reloaded = true;
    }
}
