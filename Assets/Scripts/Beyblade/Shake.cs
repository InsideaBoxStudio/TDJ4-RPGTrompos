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
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Elimina el offset anterior
            transform.localPosition -= shakeOffset;

            // Genera uno nuevo
            shakeOffset = Random.insideUnitCircle * strength;

            // Aplica el nuevo offset
            transform.localPosition += shakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Elimina el último offset antes de terminar
        transform.localPosition -= shakeOffset;
        shakeOffset = Vector3.zero;

        currentShake = null;
    }
}