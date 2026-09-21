using UnityEngine;

public class GravityToPoint : MonoBehaviour
{
    [SerializeField] public Transform gravityPoint; // El punto al que atrae
    public float gravityStrength = 10f;
    [SerializeField] private float spinDirection = 1f;
    public float frictionStrength = 0.5f;

    Rigidbody2D rb;

    void Start()
    {
        if (gravityPoint == null)
        {
            // Antes esto hacia .transform directo sobre el resultado del Find: si no
            // habia ningun objeto con ese tag, reventaba con NullReference aca mismo.
            GameObject punto = GameObject.FindGameObjectWithTag("GravityPoint");
            if (punto != null) gravityPoint = punto.transform;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Desactivamos gravedad normal
        rb.linearVelocity = new Vector2(0, spinDirection);
    }

    void FixedUpdate()
    {
        // >>> FIX ERRORES AL VOLVER AL MENU <<<
        // Al terminar la pelea, Unity destruye los objetos de la escena en orden
        // arbitrario. Si el Estadio (el punto de gravedad) se destruye antes que el
        // trompo, este FixedUpdate sigue corriendo uno o dos frames mas y encuentra
        // la referencia muerta -> MissingReferenceException 50 veces por segundo
        // hasta que termina de cargar la escena nueva.
        // En Unity, comparar contra null devuelve true tambien para objetos
        // destruidos, asi que este chequeo cubre los dos casos: destruido y sin
        // asignar. rb tambien puede ser null si Start() no llego a correr.
        if (gravityPoint == null || rb == null) return;

        Vector2 direction = (gravityPoint.position - transform.position).normalized;
        rb.AddForce(direction * gravityStrength);

        rb.AddForce(-rb.linearVelocity * frictionStrength);
    }
}