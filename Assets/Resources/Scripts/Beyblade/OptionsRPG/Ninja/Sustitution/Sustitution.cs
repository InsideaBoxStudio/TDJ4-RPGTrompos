using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sustitution : MonoBehaviour
{
    public int playerIndex = 0;
    public GameObject parent;

    private GameObject[] players;

    private Vector3 initPositionPlayer;
    private Vector3 initPositionObject;
    private bool sustitutionUsed = false;

    void Update()
    {
        if (Time.timeScale == 0) return;
        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].buttonEast.wasPressedThisFrame)
        {
            parent = gameObject.transform.parent.gameObject;
            initPositionObject = parent.transform.position;
            if (parent.CompareTag("Player")) return;

            players = GameObject
                .FindGameObjectsWithTag("Player")
                .OrderBy(go => go.name)
                .ToArray();

            initPositionPlayer = players[playerIndex].transform.position;
            parent.GetComponent<Collider2D>().enabled = false;
            players[playerIndex].GetComponent<Collider2D>().enabled = false;

            players[playerIndex].transform.position = initPositionObject;
            parent.transform.position = initPositionPlayer;

            sustitutionUsed = true;
        }
        else if (sustitutionUsed && parent.transform.position != players[playerIndex].transform.position)
        {
            parent.GetComponent<Collider2D>().enabled = true;
            players[playerIndex].GetComponent<Collider2D>().enabled = true;

            Destroy(gameObject);
        }
    }
}
