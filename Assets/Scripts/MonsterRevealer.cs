using UnityEngine;

public class MonsterRevealer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ParticleSystem particleSystem;

    private static Color saviorBlood = new Color(255 / 255f, 6 / 255f, 6 / 255f, 180 / 255f);

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
        if (spriteRenderer != null && Level.Instance.monstersRevealed)
        {
            spriteRenderer.color = saviorBlood;
        }

        if (particleSystem != null && Level.Instance.monstersRevealed)
        {
            particleSystem.startColor = saviorBlood;
        }
    }
}
