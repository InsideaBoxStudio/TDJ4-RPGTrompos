using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class EnergyCounter : MonoBehaviour
{
    public TMP_Text energyCounterText;
    public List<GameObject> energias = new List<GameObject>();

    public int currentEnergy = 0;

    public Dictionary<string, int> ataquesUsados = new Dictionary<string, int>();

    public int playerIndex = 0;

    [Header("Generación automática")]
    public GameObject energiaPrefab;

    void Start()
    {
        // Buscar los hijos existentes
        energias.Clear();

        foreach (Transform hijo in transform)
        {
            energias.Add(hijo.gameObject);
        }

        // Si no hay hijos, crear las energías automáticamente
        if (energias.Count == 0)
        {
            CrearEnergias();
        }
        else
        {
            // Si ya existen hijos, mantener el funcionamiento anterior.
            // La energía inicial sigue siendo currentEnergy.
            currentEnergy = Mathf.Clamp(currentEnergy, 0, energias.Count);
        }

        energyCounterText.text = "x" + currentEnergy.ToString();

        UpdateBar();
    }

    private void CrearEnergias()
    {
        if (energiaPrefab == null)
        {
            Debug.LogWarning(
                "EnergyCounter: No se asignó un prefab de energía. " +
                "No se pueden crear las energías automáticamente."
            );

            currentEnergy = 0;
            return;
        }

        int maxEnergy = Mathf.Max(0, PlayerSettings.maxEnergy[playerIndex]);

        // Crear la cantidad máxima de energías
        for (int i = 0; i < maxEnergy; i++)
        {
            GameObject nuevaEnergia = Instantiate(
                energiaPrefab,
                transform
            );

            energias.Add(nuevaEnergia);
        }

        // Establecer la energía inicial
        currentEnergy = Mathf.Clamp(
            PlayerSettings.initEnergy[playerIndex],
            0,
            energias.Count
        );
    }

    public void ChangeEnergy(int cantidad, string nombreAtaque)
    {
        currentEnergy += cantidad;

        // Limitar entre 0 y el máximo
        currentEnergy = Mathf.Clamp(currentEnergy, 0, energias.Count);

        energyCounterText.text = "x" + currentEnergy.ToString();
        UpdateBar();

        if (cantidad < 0 && (nombreAtaque != "Ninguno" && nombreAtaque != "BasicAttack"))
        {
            // Contar cuál es el ataque más usado
            string nombreClase = nombreAtaque;

            RegistrarAtaque(nombreClase);
        }
    }

    private void RegistrarAtaque(string nombreClase)
    {
        if (ataquesUsados.ContainsKey(nombreClase))
        {
            ataquesUsados[nombreClase]++;
        }
        else
        {
            ataquesUsados[nombreClase] = 1;
        }
    }

    public string ObtenerAtaqueMasUsado()
    {
        if (ataquesUsados.Count == 0)
            return "SwordThrust";

        return ataquesUsados
            .OrderByDescending(x => x.Value)
            .First()
            .Key;
    }

    void UpdateBar()
    {
        for (int i = 0; i < energias.Count; i++)
        {
            // Activa los primeros "currentEnergy"
            energias[i].SetActive(i < currentEnergy);
        }
    }
}