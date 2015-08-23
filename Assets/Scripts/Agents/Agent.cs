using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utilities;
using Beehive.BehaviorTrees;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Agent : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    public Target Target { get; set; }
    public LayerMask enemies;
    public Vector2 Position { get { return _rigidbody2D.position; } set { _rigidbody2D.position = value; } }
    public Vector2 Velocity { get { return _rigidbody2D.velocity; } }
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
    public AudioClip walkingSound;
    public AudioClip arrivedSound;

    public BehaviourTree<AgentBlackboard> Ai { get; set; }
    public string aiFileName;
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

    public Vector2 WantedSpeed { get; set; }
    private static BehaviourTreeCompiler<AgentBlackboard> _compiler;

    public void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        InstantiateWeapons();
        health = maxHealth;

        RebuildAi();
    }

    public void RebuildAi()
    {
        Ai = null;
        if (!string.IsNullOrEmpty(aiFileName))
        {
            TextAsset aiCode = Resources.Load<TextAsset>(aiFileName);
            if (aiCode == null)
            {
                Debug.LogFormat("Unable to find code for {0}", aiFileName);
                return;
            }
            AgentBlackboard agentBlackboard = new AgentBlackboard(this);
            _compiler = _compiler ?? new BehaviourTreeCompiler<AgentBlackboard>();
            Ai = _compiler.Compile(agentBlackboard, aiCode.text);
        }
    }

    public void Update()
    {
        if (agentType == AgentType.ViolentMonster)
        {
            Target = Level.Instance.SaviorAgentTypeTarget;
        }

        if (Ai == null)
        {
            GoToTarget();
        }
        else
        {
            Ai.Tick();
        }
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

    public void FixedUpdate()
    {
        _rigidbody2D.velocity = Vector2.Lerp(WantedSpeed, _rigidbody2D.velocity, Level.Instance.agentMomentum);
        if (currentWeapon != null && !currentWeapon.HasTarget && WantedSpeed.sqrMagnitude > 0.01f)
        {
            float wantedAngle = Mathf.Atan2(WantedSpeed.y, WantedSpeed.x) * Mathf.Rad2Deg;
            _rigidbody2D.rotation = Mathf.LerpAngle(wantedAngle, _rigidbody2D.rotation, 1 - Time.deltaTime * Level.Instance.agentRotationSpeed);
        }
    }

    public bool GoToTarget()
    {
        if (Target != null)
        {
            Vector2 flow = Target.GetFlowToTarget(Position);
            WantedSpeed = flow * Level.Instance.agentMaxSpeed;

            // We've arrived!
            Vector2 actualTarget;
            if (Target.IsAtTarget(Position, out actualTarget))
            {
                if (!Target.HasArrived)
                {
                    PlaySound(arrivedSound);
                    Target.HasArrived = true;
                }

                Target = null;
                return false;
            }
            return true;
        }
        else
        {
            WantedSpeed = Vector2.zero;
            return false;
        }
    }

    private void Die()
    {
        PlaySound(deathSound);
        Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip)
        {
            AudioSource.PlayClipAtPoint(clip, Position);
        }
    }

    private void AddBloodSpill()
    {
        //if (!bloodSpillPrefabs.Any())
        //{
        //    return;
        //}

        //GameObject prefab = bloodSpillPrefabs[Random.Range(0, bloodSpillPrefabs.Length)];
        GameObject prefab = bloodSpillPrefabs.RandomItem();
        if (prefab != null)
        {
            GameObject blood = (GameObject)Instantiate(prefab, Position + Random.insideUnitCircle * 0.5f, Quaternion.identity);
            blood.transform.Rotate(0, 0, Random.Range(0, 360));
            blood.transform.SetParent(Level.Instance.garbageHome, true);
        }
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
}