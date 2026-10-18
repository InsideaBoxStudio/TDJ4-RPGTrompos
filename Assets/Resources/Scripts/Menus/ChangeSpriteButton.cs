using UnityEngine;
using UnityEngine.UI;

public class ChangeSpriteButton : MonoBehaviour
{
    [SerializeField] private bool updateImage = false;
    [SerializeField] private int playerIndex;
    [SerializeField] private Image buttonImage;
    [SerializeField] private SpriteRenderer buttonSprite;

    public ButtonType buttonType;

    public enum ButtonType
    {
        South,
        East,
        West,
        North,
        L1,
        R1
    }

    private void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonSprite = GetComponent<SpriteRenderer>();

        if (buttonImage != null)
        {
            ImageChange();
        }
        else if (buttonSprite != null)
        {
            SpriteRendererChange();
        }
        else
        {
            Debug.LogWarning(
                "No se detectó un componente Image o SpriteRenderer en "
                + gameObject.name
            );
        }
    }

    private void Update()
    {
        if (!updateImage) return;

        if (buttonImage != null)
        {
            ImageChange();
        }
        else if (buttonSprite != null)
        {
            SpriteRendererChange();
        }
    }

    private void SpriteRendererChange()
    {
        switch (buttonType)
        {
            case ButtonType.South:
                buttonSprite.sprite =
                    JoystickSpriteManager.Instance.GetButtonSouth(playerIndex);
                break;

            case ButtonType.East:
                buttonSprite.sprite =
                    JoystickSpriteManager.Instance.GetButtonEast(playerIndex);
                break;

            case ButtonType.West:
                buttonSprite.sprite =
                    JoystickSpriteManager.Instance.GetButtonWest(playerIndex);
                break;

            case ButtonType.North:
                buttonSprite.sprite =
                    JoystickSpriteManager.Instance.GetButtonNorth(playerIndex);
                break;

            case ButtonType.L1:
                buttonSprite.sprite =
                    JoystickSpriteManager.Instance.GetButtonL1(playerIndex);
                break;

            case ButtonType.R1:
                buttonSprite.sprite =
                    JoystickSpriteManager.Instance.GetButtonR1(playerIndex);
                break;
        }
    }

    private void ImageChange()
    {
        switch (buttonType)
        {
            case ButtonType.South:
                buttonImage.sprite =
                    JoystickSpriteManager.Instance.GetButtonSouth(playerIndex);
                break;

            case ButtonType.East:
                buttonImage.sprite =
                    JoystickSpriteManager.Instance.GetButtonEast(playerIndex);
                break;

            case ButtonType.West:
                buttonImage.sprite =
                    JoystickSpriteManager.Instance.GetButtonWest(playerIndex);
                break;

            case ButtonType.North:
                buttonImage.sprite =
                    JoystickSpriteManager.Instance.GetButtonNorth(playerIndex);
                break;

            case ButtonType.L1:
                buttonImage.sprite =
                    JoystickSpriteManager.Instance.GetButtonL1(playerIndex);
                break;

            case ButtonType.R1:
                buttonImage.sprite =
                    JoystickSpriteManager.Instance.GetButtonR1(playerIndex);
                break;
        }
    }
}