using UnityEngine;

public class Package : MonoBehaviour
{
    public float packageThreshold = 3f; // Threshold for high-speed collision

    public float damageCoolDown = 1f; // Cooldown time between damage applications
    private float lastDamageTime = -99f; // Time of the last damage application

    [SerializeField] private Package_SO packageData;
    [SerializeField] private Rigidbody packageRigidbody;

    string packageName;
    float packageHealth;
    float crashModifier;
    float floatModifier;

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
        if (packageHealth <= 0)
        {
            Debug.Log($"Package {packageName} is already destroyed. No further damage can be applied.");
        }
        else
        {
            if (damage >= packageHealth)
            {
                damage = packageHealth; // Cap the damage to the remaining health
                packageHealth = 0; // Set health to zero if damage exceeds current health
                Destroy(gameObject); // Destroy the package GameObject
                Debug.Log($"Package {packageName} is destroyed!");
            }
            else
            {
                packageHealth -= damage; // Reduce the health by the damage amount
                packageHealth = Mathf.Round(packageHealth); // Round the health to avoid floating-point precision issues
                Debug.Log($"Package {packageName} took {damage} damage. Remaining health: {packageHealth}");

                if(packageHealth <= 0)
                {
                    packageHealth = 0; // Ensure health doesn't go below zero
                    Destroy(gameObject); // Destroy the package GameObject
                    Debug.Log($"Package {packageName} is destroyed!");
                }
            }
        }
    }
}
