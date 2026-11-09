using UnityEngine;

// Va en el objeto Estadio de cada pelea. Pone los muros del estadio elegido en
// StadiumSetting. Si no se eligió ninguno (botón sin prefab, o la escena se abrió
// directo desde el Editor), activa el muro que la escena ya trae como hijo, para
// que el estadio nunca quede sin paredes.
public class InstantiateStadium : MonoBehaviour
{
    [Tooltip("Usar siempre el muro de la escena, sin importar el estadio elegido (Tutorial: sus distancias están calibradas para ese muro).")]
    [SerializeField] private bool ignorarEleccion = false;

    void Awake()
    {
        GameObject elegido = ignorarEleccion ? null : PlayerSettings.stadium;

        // Muros que vienen en la escena (hijos directos "Muro ..."): se usan solo
        // si no hay estadio elegido, así no quedan dos muros encimados.
        foreach (Transform hijo in transform)
        {
            if (hijo.name.StartsWith("Muro"))
                hijo.gameObject.SetActive(elegido == null);
        }

        if (elegido != null)
        {
            Instantiate(elegido, transform);
        }
    }
}
