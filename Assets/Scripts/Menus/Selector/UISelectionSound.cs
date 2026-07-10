using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class UISelectionSound : MonoBehaviour
{
    [Header("Sonido")]
    public AudioClip selectionSound;

    private AudioSource audioSource;
    private GameObject lastSelected;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // Si cambió la selección
        if (currentSelected != lastSelected)
        {
            // Reproduce el sonido solo si el objeto seleccionado tiene un Button
            if (currentSelected != null && currentSelected.GetComponent<Button>() != null)
            {
                if (selectionSound != null)
                {
                    audioSource.PlayOneShot(selectionSound);
                }
            }

            lastSelected = currentSelected;
        }
    }
}