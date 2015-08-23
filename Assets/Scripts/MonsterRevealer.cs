using UnityEngine;
using System.Collections;

public class MonsterRevealer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ParticleSystem particleSystem;

    private static Color saviorBlood = new Color(255 / 255f, 6 / 255f, 6 / 255f, 180 / 255f);

    public void Update()
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
