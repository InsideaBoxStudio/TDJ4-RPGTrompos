using System.Collections;
using UnityEngine;

public class Shake : MonoBehaviour
{
    private Coroutine currentShake;
    private Vector3 shakeOffset;

    public void StartShake(float duration, float strength)
    {
        if (currentShake != null)
            StopCoroutine(currentShake);

        currentShake = StartCoroutine(ShakeCoroutine(duration, strength));
    }

    private IEnumerator ShakeCoroutine(float duration, float strength)
    {
        float endTime = Time.unscaledTime + duration;

        while (Time.unscaledTime < endTime)
        {
            transform.localPosition -= shakeOffset;

            shakeOffset = (Vector3)(Random.insideUnitCircle * strength);

            transform.localPosition += shakeOffset;

            yield return null;
        }

        transform.localPosition -= shakeOffset;
        shakeOffset = Vector3.zero;
        currentShake = null;
    }
}