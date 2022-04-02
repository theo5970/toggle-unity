using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrangeRotator : MonoBehaviour
{
    private static bool workwork = false;

    float offset = 0;
    private static float speed = 2f;
    IEnumerator Start()
    {
        offset = Random.Range(-1000, 1000);
        if (!workwork) workwork = Random.value < 0.0005f;

        yield return null;
        yield return StartLoop();
    }

    void OnEnable()
    {
        StartCoroutine(StartLoop());
    }
    IEnumerator StartLoop()
    {
        if (workwork)
        {
            float angleOffset = transform.eulerAngles.z;
            while (true)
            {
                float angle = angleOffset - 360 * Mathf.PerlinNoise(offset + Time.time * speed, offset + Time.time * speed );
                transform.rotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }
        }
    }
}
