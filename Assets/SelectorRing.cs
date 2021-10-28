using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorRing : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Animate());
    }
    [Range(0,5)] public float iterations;
    [Range(0,200)] public float steps;
    [Range(0.0f, 0.1f)] public float scaleStep;
    [Range(0.0f, 0.1f)] public float timeStep;

    private IEnumerator Animate()
    {
        Vector3 baseScale = transform.localScale;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < steps; j++)
            {
                transform.localScale += new Vector3(scaleStep, scaleStep, 0f);
                yield return new WaitForSeconds(timeStep);
            }
            transform.localScale = baseScale;
        }

        Destroy(this.gameObject);
    }
}
