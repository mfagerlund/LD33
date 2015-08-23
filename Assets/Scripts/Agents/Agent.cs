using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

[RequireComponent(typeof(Rigidbody2D))]
public class Agent : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    public Target Target { get; set; }
    public LayerMask enemies;
    public Vector2 Position { get { return _rigidbody2D.position; } set { _rigidbody2D.position = value; } }
    public float Rotation { get { return _rigidbody2D.rotation; } set { _rigidbody2D.rotation = value; } }
    //public Vector2 Velocity { get; set; }
    public bool Selected { get; set; }
    public float health = 20;
    public float maxHealth = 20;
    public Weapon[] weaponPrefabs;
    public Weapon currentWeapon;
    public AgentType agentType;
    public GameObject[] bloodSpillPrefabs;
    public AudioClip deathSound;
    public const float AgentRadius = 0.3f;

    private List<Weapon> WeaponInstances { get; set; }

    public bool IsPlayerControlled
    {
        get
        {
            return
                agentType == AgentType.Savior
                || agentType == AgentType.ConvertedMonster;
        }
    }

    private Vector2 _wantedSpeed;

    public void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        InstantiateWeapons();
        health = maxHealth;
    }

    public void Update()
    {
        GoToDestination();
        FireAtEnemies();
        health = Mathf.Min(health + Level.Instance.agentHealthRegeneration * Time.deltaTime, maxHealth);

        if (currentWeapon == null || currentWeapon.IsOutOfAmmo)
        {
            EquipWeaponWithLowestPriority();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;

        if (health <= 0 || health > Random.Range(0, 20))
        {
            AddBloodSpill();
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        AudioSource.PlayClipAtPoint(deathSound, Position);
        Destroy(gameObject);
    }

    public void FixedUpdate()
    {
        _rigidbody2D.velocity = Vector2.Lerp(_wantedSpeed, _rigidbody2D.velocity, Level.Instance.agentMomentum);
        if (currentWeapon != null && !currentWeapon.HasTarget)
        {
            float wantedAngle = Mathf.Atan2(_wantedSpeed.y, _wantedSpeed.x) * Mathf.Rad2Deg;
            _rigidbody2D.rotation = Mathf.LerpAngle(wantedAngle, _rigidbody2D.rotation, 1 - Time.deltaTime * Level.Instance.agentRotationSpeed);
        }
    }

    private void AddBloodSpill()
    {
        if (!bloodSpillPrefabs.Any())
        {
            return;
        }

        GameObject prefab = bloodSpillPrefabs[Random.Range(0, bloodSpillPrefabs.Length)];
        GameObject blood = (GameObject)Instantiate(prefab, Position + Random.insideUnitCircle * 0.5f, Quaternion.identity);
        blood.transform.Rotate(0, 0, Random.Range(0, 360));
    }

    private void InstantiateWeapons()
    {
        WeaponInstances = new List<Weapon>();
        foreach (Weapon weaponPrefab in weaponPrefabs)
        {
            Weapon instance = (Weapon)Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(this.transform, false);
            instance.gameObject.SetActive(false);
            WeaponInstances.Add(instance);
        }
    }

    private void EquipWeaponWithLowestPriority()
    {
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeapon =
            WeaponInstances
                .OrderBy(w => w.priority)
                .FirstOrDefault(w => !w.IsOutOfAmmo);

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
        }
    }

    private void FireAtEnemies()
    {
        if (currentWeapon == null)
        {
            return;
        }

        currentWeapon.TryFire();
    }

    private void GoToDestination()
    {
        if (Target != null)
        {
            Vector2 flow = Target.GetFlowToTarget(Position);
            _wantedSpeed = flow * Level.Instance.agentMaxSpeed;

            // We've arrived!
            Vector2 actualTarget;
            if (Target.IsAtTarget(Position, out actualTarget))
            {
                Target = null;
            }
        }
        else
        {
            _wantedSpeed = Vector2.zero;
        }
    }
}