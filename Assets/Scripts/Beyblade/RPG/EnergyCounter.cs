using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class EnergyCounter : MonoBehaviour
{
    public List<GameObject> energias = new List<GameObject>();
    public int currentEnergy = 0;
    public Dictionary<string, int> ataquesUsados = new Dictionary<string, int>();

    void Start()
    {
        // Opcional: llenar automáticamente la lista con los hijos
        energias.Clear();
        foreach (Transform hijo in transform)
        {
            energias.Add(hijo.gameObject);
        }

        UpdateBar();
    }

    public void ChangeEnergy(int cantidad, string nombreAtaque)
    {
        currentEnergy += cantidad;

        // Limitar entre 0 y el máximo
        currentEnergy = Mathf.Clamp(currentEnergy, 0, energias.Count);

        UpdateBar();

        if (cantidad < 0)
        {
            // contar cual es el ataque mas usado

            string nombreClase = nombreAtaque;

            UnityEngine.Debug.Log($"Ataque usado: {nombreClase}");
            RegistrarAtaque(nombreClase);
        }
    }

    private void RegistrarAtaque(string nombreClase)
    {
        if (ataquesUsados.ContainsKey(nombreClase)){
            ataquesUsados[nombreClase]++;
        }
        else{
            ataquesUsados[nombreClase] = 1;
        }
    }

    public string ObtenerAtaqueMasUsado()
    {
        if (ataquesUsados.Count == 0)
            return "Ninguno";

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
