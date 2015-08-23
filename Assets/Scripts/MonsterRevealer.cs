using UnityEngine;
using UnityEngine.UI;

public class MonsterRevealer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ParticleSystem particleSystem;
    public Button button;

    private static Color saviorColor = new Color(255 / 255f, 6 / 255f, 6 / 255f, 180 / 255f);
    private static Color monsterColor = new Color(6 / 255f, 255 / 255f, 6 / 255f, 180 / 255f);

    public void Startup()
    {
        Reveal();
    }

    public void Awake()
    {
        Reveal();
    }

    public void Update()
    {
        Reveal();
    }

    private void Reveal()
    {
        if (Level.Instance == null)
        {
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Level.Instance.monstersRevealed ? saviorColor : monsterColor;
        }

        if (particleSystem != null)
        {
            particleSystem.startColor = Level.Instance.monstersRevealed ? saviorColor : monsterColor;
        }

        if (button != null)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = Level.Instance.monstersRevealed ? Color.red : Color.black;
            colorBlock.disabledColor = colorBlock.normalColor;
            colorBlock.highlightedColor = colorBlock.normalColor;
            colorBlock.pressedColor = colorBlock.normalColor;
            button.colors = colorBlock;
        }
    }
}
