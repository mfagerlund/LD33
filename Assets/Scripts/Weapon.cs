﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MyMonoBehaviour
{
    public int clipSize = 10;
    public int ammo = 50;
    public int bulletsInClip = 10;
    public int reloadTime = 3;
    public float roundsPerMinute = 10;
    public float fireDistance = 30;
    public float damageAmount = 5;

    private bool _reloading;
    private bool _cycling;

    //public GameObject weaponPrefab;
    public GameObject shellPrefab;
    public GameObject bulletPrefab;
    //public GameObject damagePrefab;
    public LineRenderer lineRendererPrefab;
    public Transform bulletSpawnPosition;
    public Transform shellSpawnPosition;

    private Agent _previosTarget;
    public int priority = 0;
    public WeaponDrop weaponDrop;

    public Agent selectedTarget;

    public bool IsOutOfAmmo { get { return ammo == 0 && bulletsInClip == 0; } }

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
        if (selectedTarget != null)
        {
            if (!CanHitAgent(selectedTarget))
            {
                selectedTarget = null;
            }
        }

        if (selectedTarget == null)
        {
            FindNewTarget();
        }

        if (selectedTarget != null)
        {
            Vector2 delta = selectedTarget.Position - _agent.Position;
            float enemyAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            _agent.Rotation = enemyAngle;
            Fire(selectedTarget);
        }
    }

    public void Fire(Agent enemyAgent)
    {
        if (!CanFire)
        {
            return;
        }

        if (_previosTarget != enemyAgent && Random.Range(0, 1f) < 0.95f)
        {
            return;
        }
        _previosTarget = enemyAgent;

        _cycling = true;
        float damage = (Random.Range(0, damageAmount) + Random.Range(0, damageAmount)) * 0.5f;
        GetComponent<AudioSource>().Play();
        enemyAgent.TakeDamage(damage);
        //ShowDamage(enemyAgent);
        ShowFiring(enemyAgent);
        ShowCasing();
        Level.Instance.RegisterViolence();
        Invoke(() => _cycling = false, _secondsBetweenBullets);
    }

    public bool CanFire { get { return !_cycling && !_reloading && bulletsInClip > 0; } }

    public bool HasTarget { get { return selectedTarget != null; } }

    public void Reload()
    {
        if (ammo > 0)
        {
            // Play reload sound
            int wantToLoad = clipSize - bulletsInClip;
            int canLoad = Mathf.Min(ammo, wantToLoad);
            bulletsInClip += canLoad;
            ammo -= canLoad;
            _reloading = true;
            Invoke(() => _reloading = false, reloadTime);
        }
        else
        {
            // Play failed to reload sound
        }
    }

    private void ShowFiring(Agent enemyAgent)
    {
        if (lineRendererPrefab != null)
        {
            LineRenderer lineRenderer = InstantiateAtMe(lineRendererPrefab);
            lineRenderer.SetPosition(0, _agent.Position);
            lineRenderer.SetPosition(1, enemyAgent.Position);
            lineRenderer.transform.SetParent(Level.Instance.garbageHome, true);
            Destroy(lineRenderer.gameObject, 0.2f);
        }
    }

    private void ShowCasing()
    {
        if (shellPrefab != null)
        {
            GameObject shell = (GameObject)Instantiate(shellPrefab, shellSpawnPosition.position, Quaternion.identity);
            Rigidbody2D rigidbody = shell.GetComponent<Rigidbody2D>();
            Vector2 force = shellSpawnPosition.right * Random.Range(2, 3);
            rigidbody.AddForce(force + _agent.Velocity, ForceMode2D.Impulse);
            rigidbody.AddTorque(Random.Range(-0.1f, 0.1f));
            shell.transform.SetParent(Level.Instance.garbageHome, true);
        }
    }

    private bool CanHitAgent(Agent targetAgent)
    {
        if (targetAgent == null)
        {
            return false;
        }

        Vector2 delta = (targetAgent.Position - _agent.Position);

        // Is it within range?
        if (delta.magnitude > fireDistance)
        {
            return false;
        }

        delta = delta.normalized;
        RaycastHit2D hit = Physics2D.Raycast(_agent.Position + delta * (Agent.AgentRadius + 0.2f), delta, fireDistance, _agent.enemies | Level.Instance.buildings);
        if (hit.collider != targetAgent.GetComponent<Collider2D>())
        {
            return false;
        }
        return true;
    }

    //private void ShowDamage(Agent enemyAgent)
    //{
    //    if (damagePrefab != null)
    //    {
    //        Instantiate(damagePrefab, enemyAgent.transform.position, Quaternion.identity);
    //    }
    //}

    private void FindNewTarget()
    {
        List<Collider2D> colliders = Physics2D.OverlapCircleAll(_agent.Position, fireDistance, _agent.enemies).ToList();
        colliders = colliders.OrderBy(c => ((Vector2)c.transform.position - _agent.Position).sqrMagnitude).ToList();

        foreach (Collider2D target in colliders)
        {
            Agent targetAgent = target.GetComponent<Agent>();
            if (CanHitAgent(targetAgent))
            {
                selectedTarget = targetAgent;
                return;
            }
        }
    }

    public void Drop()
    {
        WeaponDrop drop = (WeaponDrop)Instantiate(weaponDrop, transform.position, Quaternion.identity);
        drop.transform.Rotate(0, 0, Random.Range(0, 360));
        drop.transform.SetParent(Level.Instance.garbageHome);        
    }
}
