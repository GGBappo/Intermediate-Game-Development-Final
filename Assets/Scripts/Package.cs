using System;
using UnityEngine;

public class Package : MonoBehaviour
{
    [Range(0f, 10f)]
    public float packageThreshold = 3f; // Threshold for high-speed collision

    public float damageCoolDown = 1f; // Cooldown time between damage applications
    private float lastDamageTime = -99f; // Time of the last damage application

    [SerializeField] private Package_SO packageData;
    [SerializeField] private Rigidbody packageRigidbody;
    [SerializeField] private CheckPoint checkPoint;

    [Header("SO matching")]
    string packageName;
    public float packageHealth;
    public float crashModifier;
    float floatModifier;
    bool isDestroyed = false;
    private int damageTaken;

    [Tooltip("Events triggered when the package takes damage.")]

    public event Action<float> onPackageDamaged;
    //public properties to expose packageHealth and packageData safely
    public float PackageHealth => packageHealth;
    //public property to expose packageData safely
    public Package_SO PackageData => packageData;
    //public property to expose checkPoint safely
    public CheckPoint CheckPoint => checkPoint;
    //public property to expose isDestroyed safely
    public int DamageTaken => damageTaken;
    public event Action<Package> onPackageDestroyed;

    private void Awake()
    {
        if(packageRigidbody == null)
        {
            packageRigidbody = GetComponent<Rigidbody>();
        }

        packageName = packageData.packageName;
        packageHealth = packageData.packageHealth;
        crashModifier = packageData.crashModifier;
        floatModifier = packageData.floatModifier;

        packageRigidbody.mass *= floatModifier; // Adjust the mass of the Rigidbody based on the floatModifier
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude*crashModifier >= packageThreshold)
        {
            if(Time.time - lastDamageTime < damageCoolDown)
            {
                Debug.Log($"Damage cooldown active. Time since last damage: {Time.time - lastDamageTime}s");
                return; // Exit if the cooldown period has not passed
            }
            lastDamageTime = Time.time; // Update the last damage time

            float damage = collision.relativeVelocity.magnitude * crashModifier;

            TakeDamage(damage);

            Debug.Log($"Hit {collision.gameObject.name}, speed {collision.relativeVelocity.magnitude}, modifier {crashModifier}");
        }
    }


    private void TakeDamage(float damage)
    {
        if (isDestroyed || packageHealth <= 0f)
            return;

        float appliedDamage = Mathf.Min(damage, packageHealth);

        packageHealth -= appliedDamage;
        packageHealth = Mathf.Max(0f, Mathf.Round(packageHealth));

        damageTaken += Mathf.RoundToInt(appliedDamage);
        onPackageDamaged?.Invoke(appliedDamage);

        Debug.Log($"Package {packageName} took {appliedDamage} damage. Remaining health: {packageHealth}");

        if (packageHealth <= 0f)
        {
            DestroyPackage();
        }
    }

    private void DestroyPackage()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        packageHealth = 0f;

        onPackageDestroyed?.Invoke(this);

        Debug.Log($"Package {packageName} is destroyed!");

        Destroy(gameObject);
    }

    public void SetDestination(CheckPoint destination)
    {
        checkPoint = destination;
    }
}
