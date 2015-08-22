using System;
using UnityEngine;

public class Weapon : MyMonoBehaviour
{
    public int clipSize = 10;
    public int ammo = 50;
    public int bulletsInClip = 10;
    public int reloadTime = 3;
    public float roundsPerMinute = 10;
    public float fireDistance = 30;

    private bool _reloading;
    private bool _cycling;

    //public GameObject weaponPrefab;
    public GameObject shellPrefab;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPosition;
    public Transform shellSpawnPosition;

    public Agent targetAgent;

    private float _secondsBetweenBullets;
    private Agent _agent;

    public void Start()
    {
        _agent = GetComponentInParent<Agent>();
        _secondsBetweenBullets = 1 / (roundsPerMinute / 60f);
    }

    public void Update()
    {

    }

    public void TryFire()
    {
        // Is there a previous target?
        if (targetAgent != null)
        {
            // Is it within range?
            if ((targetAgent.Position - _agent.Position).magnitude > fireDistance)
            {
                targetAgent = null;
            }

            // Do we have a clear view to it?
            if (targetAgent != null)
            {
                CheckClearViewToTarget();
            }
        }

        if (targetAgent == null)
        {
            FindNewTarget();
        }

        if (targetAgent != null)
        {
            Vector2 delta = targetAgent.Position - _agent.Position;
            float enemyAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            _agent.Rotation = enemyAngle;
            Fire(targetAgent);
        }
    }

    private void CheckClearViewToTarget()
    {
        if (targetAgent == null)
        {
            return;
        }
        Vector2 delta = (targetAgent.Position - _agent.Position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(_agent.Position + delta * (Agent.AgentRadius + 0.2f), delta, fireDistance);
        if (hit == null || hit.collider != targetAgent.GetComponent<Collider2D>())
        {
            targetAgent = null;
        }
    }

    public void Fire(Agent enemyAgent)
    {
        if (!CanFire)
        {
            return;
        }

        _cycling = true;
        Debug.Log("Firing!");
        Debug.DrawLine(bulletSpawnPosition.position, enemyAgent.Position, Color.red, 0.1f);
        Invoke(() => _cycling = false, _secondsBetweenBullets);
    }

    public bool CanFire { get { return !_cycling && !_reloading && bulletsInClip > 0; } }

    public bool HasTarget { get { return targetAgent != null; } }

    public void Reload()
    {
        if (ammo > 0)
        {
            int wantToLoad = clipSize - bulletsInClip;
            int canLoad = Math.Min(ammo, wantToLoad);
            bulletsInClip += canLoad;
            ammo -= canLoad;
            _reloading = true;
            Invoke(() => _reloading = false, reloadTime);
        }
    }

    private void FindNewTarget()
    {
        Collider2D enemy = Physics2D.OverlapCircle(_agent.Position, fireDistance, _agent.enemies);
        if (enemy)
        {
            targetAgent = enemy.GetComponent<Agent>();
            CheckClearViewToTarget();
        }
    }
}
