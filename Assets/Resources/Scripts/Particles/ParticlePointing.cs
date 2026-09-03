using UnityEngine;

public class ParticlePointing : MonoBehaviour
{
    public Transform objetoQueSeMueve;

    private Vector3 posicionAnterior;

    void Start()
    {
        posicionAnterior = objetoQueSeMueve.position;
    }

    void Update()
    {
        Vector3 movimiento = objetoQueSeMueve.position - posicionAnterior;

        if (movimiento.sqrMagnitude > 0.0001f)
        {
            // Dirección contraria al movimiento
            Vector3 direccion = -movimiento.normalized;

            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }

        posicionAnterior = objetoQueSeMueve.position;
    }
}