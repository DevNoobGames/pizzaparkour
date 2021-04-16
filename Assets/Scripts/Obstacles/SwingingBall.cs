using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingBall : MonoBehaviour
{
    bool started;
    float timer;

    private void Start()
    {
        started = false;
        timer = 0;
        float wait = Random.Range(1, 5);
        StartCoroutine(startnum(wait));
    }

    IEnumerator startnum(float sec)
    {
        yield return new WaitForSeconds(sec);
        started = true;
    }

    private void Update()
    {
        if (started)
        {
            timer += Time.deltaTime;
            float angle = Mathf.Sin(timer) * 70; //tweak this to change frequency

            transform.rotation = Quaternion.AngleAxis(angle, Vector3.right);
        }
    }
}
