using UnityEngine;
using System.Collections;

public class DeleteObjectAfterParticleSystemEnds : MonoBehaviour
{
    public GameObject objectToDelete;

    private ParticleSystem _particleSystem;

    public void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void Update()
    {
        if (_particleSystem.IsAlive())
        {
            return;
        }
        
        if (objectToDelete != null)
        {
            Destroy(objectToDelete);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
