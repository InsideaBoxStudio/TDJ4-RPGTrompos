
using UnityEngine;
using System.Collections;

public class RandomObjectSpawner : MonoBehaviour
{
    [Header("Objetos a instanciar")]
    public GameObject[] objetos;

    [Header("Área de aparición")]
    [Min(0f)]
    public float radio = 5f;

    [Header("Tiempo entre apariciones")]
    [Min(0f)]
    public float tiempoMinimo = 2f;

    [Min(0f)]
    public float tiempoMaximo = 5f;

    private Coroutine rutinaInstanciacion;

    private void OnEnable()
    {
        rutinaInstanciacion = StartCoroutine(InstanciarObjetos());
    }

    private void OnDisable()
    {
        if (rutinaInstanciacion != null)
        {
            StopCoroutine(rutinaInstanciacion);
            rutinaInstanciacion = null;
        }
    }

    private IEnumerator InstanciarObjetos()
    {
        while (true)
        {
            float tiempoEspera = Random.Range(
                tiempoMinimo,
                Mathf.Max(tiempoMinimo, tiempoMaximo)
            );

            yield return new WaitForSeconds(tiempoEspera);

            if (objetos == null || objetos.Length == 0)
                continue;

            GameObject objetoElegido = objetos[
                Random.Range(0, objetos.Length)
            ];

            if (objetoElegido == null)
                continue;

            Vector2 posicionAleatoria =
                Random.insideUnitCircle * radio;

            Vector3 posicionFinal = transform.position +
                new Vector3(
                    posicionAleatoria.x,
                    posicionAleatoria.y,
                    0f
                );

            Instantiate(
                objetoElegido,
                posicionFinal,
                Quaternion.identity
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}