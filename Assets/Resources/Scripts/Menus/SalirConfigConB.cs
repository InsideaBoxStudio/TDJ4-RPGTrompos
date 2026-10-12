using UnityEngine;
using UnityEngine.EventSystems;

// Salir del menú de Configuración con B (botón de la derecha del joystick; Esc o
// Retroceso en el teclado).
//
// Cómo se reparte la tecla entre los dos niveles del menú:
//   - Con el cursor DENTRO de un submenú (Sonido, Video...), B lo maneja
//     CancelActionMenu: devuelve el cursor a la lista.
//   - Con el cursor en la LISTA (Sonido / Video / Continuar), B sale: hace lo mismo
//     que el botón "Continuar" (ChangeMap.ChangeScene).
//
// Para no pisarse con CancelActionMenu en el mismo frame se mira dónde ESTABA el
// cursor al terminar el frame anterior (LateUpdate). El orden en que Unity corre los
// Update de dos scripts distintos no está garantizado: si se mirara el cursor "ahora",
// un B dado dentro de un submenú podría devolver el cursor a la lista y, en el mismo
// frame, salir de la pantalla.
public class SalirConfigConB : MonoBehaviour
{
    [Tooltip("El ChangeMap del botón Continuar. Si está vacío, lo busca solo.")]
    [SerializeField] private ChangeMap salida;

    private CancelActionMenu[] submenus;
    private bool estabaDentroDeUnSubmenu;
    private bool saliendo;

    private void Awake()
    {
        if (salida == null) salida = FindFirstObjectByType<ChangeMap>(FindObjectsInactive.Include);
        submenus = FindObjectsByType<CancelActionMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void Update()
    {
        if (saliendo || salida == null) return;
        if (!Controles.Volver()) return;
        if (estabaDentroDeUnSubmenu) return; // este B es del submenú (CancelActionMenu)

        saliendo = true; // un solo pedido aunque se apriete B varias veces
        salida.ChangeScene();
    }

    private void LateUpdate()
    {
        estabaDentroDeUnSubmenu = CursorDentroDeUnSubmenu();
    }

    private bool CursorDentroDeUnSubmenu()
    {
        EventSystem sistema = EventSystem.current;
        if (sistema == null) return false;

        GameObject seleccionado = sistema.currentSelectedGameObject;
        if (seleccionado == null) return false;

        for (int i = 0; i < submenus.Length; i++)
        {
            if (submenus[i] != null && seleccionado.transform.IsChildOf(submenus[i].transform))
                return true;
        }
        return false;
    }
}
