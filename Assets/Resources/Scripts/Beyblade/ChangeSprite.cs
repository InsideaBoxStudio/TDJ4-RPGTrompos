using UnityEngine;

public class ChangeSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderer = new SpriteRenderer[2];
    [SerializeField] private int playerIndex = 0;

    void Start()
    {
        for(int i = 0; i < spriteRenderer.Length; i++)
        {
            if (spriteRenderer[i] != null)
            {
                spriteRenderer[i].sprite = PlayerSettings.layerSprite[playerIndex];
            }
        }
    }
}
