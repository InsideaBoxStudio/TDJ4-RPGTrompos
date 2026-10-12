using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

// Oscurece las opciones de ataque que el jugador no puede pagar con su energía actual.
//
// Lo crea AttacksMenuPreset al armar el menú (una vez por panel de Ataques y otra por el
// de Especiales), así que no hace falta tocar escenas ni prefabs. Mira la energía del
// jugador y solo vuelve a pintar cuando CAMBIA, no en cada frame.
public class ColorSinEnergia : MonoBehaviour
{
    // Se multiplica por el color original de cada gráfico: queda el mismo dibujo pero
    // apagado, sin perder el matiz ni el alfa.
    private static readonly Color Apagado = new Color(0.35f, 0.35f, 0.35f, 1f);

    private class Opcion
    {
        public int costo;
        public Graphic[] graficos;
        public Color[] coloresOriginales;
        public SpriteRenderer[] sprites;
        public Color[] coloresSprites;
        public bool apagada;
    }

    private Opcion[] opciones;
    private EnergyCounter energia;
    private int jugador;
    private int ultimaEnergia = -1;

    public void Iniciar(GameObject[] instancias, int playerIndex)
    {
        jugador = playerIndex;
        opciones = new Opcion[instancias.Length];

        for (int i = 0; i < instancias.Length; i++)
        {
            opciones[i] = CrearOpcion(instancias[i]);
        }
    }

    // Al reactivarse el menú la energía pudo haber cambiado mientras estaba cerrado.
    private void OnEnable()
    {
        ultimaEnergia = -1;
    }

    private void Update()
    {
        if (opciones == null) return;

        if (energia == null)
        {
            energia = BuscarEnergia();
            if (energia == null) return;
        }

        int actual = energia.currentEnergy;
        if (actual == ultimaEnergia) return;
        ultimaEnergia = actual;

        for (int i = 0; i < opciones.Length; i++)
        {
            Opcion o = opciones[i];
            if (o == null) continue;

            bool sinEnergia = o.costo > actual;
            if (sinEnergia != o.apagada) Pintar(o, sinEnergia);
        }
    }

    private EnergyCounter BuscarEnergia()
    {
        GameObject trompo = PlayerSettings.player[jugador];
        if (trompo == null) return null;

        DefPlayerID id = trompo.GetComponent<DefPlayerID>();
        return id != null ? id.energyCounter : null;
    }

    private static Opcion CrearOpcion(GameObject instancia)
    {
        if (instancia == null) return null;

        var o = new Opcion
        {
            costo = LeerCosto(instancia),
            graficos = instancia.GetComponentsInChildren<Graphic>(true),
            sprites = instancia.GetComponentsInChildren<SpriteRenderer>(true)
        };

        o.coloresOriginales = new Color[o.graficos.Length];
        for (int i = 0; i < o.graficos.Length; i++) o.coloresOriginales[i] = o.graficos[i].color;

        o.coloresSprites = new Color[o.sprites.Length];
        for (int i = 0; i < o.sprites.Length; i++) o.coloresSprites[i] = o.sprites[i].color;

        return o;
    }

    private static void Pintar(Opcion o, bool apagar)
    {
        o.apagada = apagar;

        for (int i = 0; i < o.graficos.Length; i++)
        {
            if (o.graficos[i] == null) continue;
            o.graficos[i].color = apagar ? Multiplicar(o.coloresOriginales[i]) : o.coloresOriginales[i];
        }

        for (int i = 0; i < o.sprites.Length; i++)
        {
            if (o.sprites[i] == null) continue;
            o.sprites[i].color = apagar ? Multiplicar(o.coloresSprites[i]) : o.coloresSprites[i];
        }
    }

    private static Color Multiplicar(Color original)
    {
        return new Color(original.r * Apagado.r, original.g * Apagado.g, original.b * Apagado.b, original.a);
    }

    // El costo de energía de una opción. Cada script de ataque lo guarda en un campo
    // `energyCost`, pero no comparten una clase base: algunos exponen EnergyCost por la
    // interfaz de la IA (IAIAction) y otros solo tienen el campo. Se prueba primero la
    // interfaz y, si no, se lee el campo. Se hace UNA vez al armar el menú, no por frame.
    // Si una opción no tiene ninguna de las dos cosas, se considera gratis (queda
    // siempre disponible) y se avisa en la Console.
    private static int LeerCosto(GameObject instancia)
    {
        foreach (MonoBehaviour mb in instancia.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;

            if (mb is IAIAction accion) return accion.EnergyCost;

            FieldInfo campo = mb.GetType().GetField("energyCost",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (campo != null && campo.FieldType == typeof(int)) return (int)campo.GetValue(mb);
        }

        Debug.LogWarning($"ColorSinEnergia: no encontré el costo de energía de '{instancia.name}'; " +
                         "se deja siempre disponible.");
        return 0;
    }
}
