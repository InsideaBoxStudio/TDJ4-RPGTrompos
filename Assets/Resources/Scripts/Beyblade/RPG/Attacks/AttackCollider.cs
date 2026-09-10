using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    [SerializeField] private EnergyCounter energyCounter;
    [Header("Stats")]
    [SerializeField] private int energyGain = 2;
    [SerializeField] private int damage = 3;

    // >>> FIX DANO PERDIDO <<<
    // El cuerpo del trompo tiene su propio collider (BeybladeCollider) que al chocar
    // hace rb.linearVelocity = reflejada * rebote, o sea SEPARA los trompos al
    // instante. Era una carrera: si el cuerpo tocaba primero, este collider de ataque
    // podia no llegar a solaparse nunca y el golpe no hacia dano. Por eso fallaba
    // "a veces" y no siempre.
    //
    // Ahora el golpe se intenta tambien en OnCollisionStay2D, y una bandera garantiza
    // UN solo impacto por activacion del ataque (no se duplica el dano).
    private bool yaGolpeo = false;

    // Layer del duenio del ataque, cacheada al prender. Antes se leia
    // transform.parent en cada choque; si el objeto se reparenta (varios scripts
    // reparentan hijos: shuriken, sustitucion, magias) la comparacion se hacia
    // contra el objeto equivocado y el dano no salia.
    private int layerDuenio;

    private void OnEnable()
    {
        yaGolpeo = false;
        if (transform.parent != null) layerDuenio = transform.parent.gameObject.layer;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IntentarGolpe(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        IntentarGolpe(collision);
    }

    private void IntentarGolpe(Collision2D collision)
    {
        if (yaGolpeo) return;

        // verificar que sea un jugador
        if (!collision.gameObject.CompareTag("Player")) return;

        // verificar que sea un jugador diferente al que lanzo el ataque
        if (collision.gameObject.layer == layerDuenio) return;

        Vida vida = collision.gameObject.GetComponent<Vida>();
        if (vida == null) return; // sin Vida no hay a quien danar

        yaGolpeo = true;

        energyCounter.ChangeEnergy(energyGain, "GainEnergy");

        Vector3 contactPoint = collision.GetContact(0).point;
        vida.Damage(damage, contactPoint);
    }
}