using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using UnityEditor;
using TMPro;
using UnityEngine;

public class CargoManager : MonoBehaviour
{
    //List of deliveries to be completed in the current mission
    [SerializeField] private List<DeliveryAssignment> deliveries = new List<DeliveryAssignment>();
    //List of active packages currently in the scene
    [SerializeField] private List<Package> activePackages = new List<Package>();
     
    [SerializeField] private Transform packageSpawnPoint;
    [SerializeField] private float ratingThreshhold = 100;
    [SerializeField] private GameObject endingScreen;
    [SerializeField] private TMP_Text totalDamageText;

    private float totalMaxHealth = 0f;
    private float totalRemainingHealth = 0f;
    private float totalDamageTaken = 0f;
    private int totalPackagesDestroyed;

    // Public properties to access deliveries and active packages
    public List<DeliveryAssignment> Deliveries => deliveries;
    public int DeliveriesCompleted => deliveries.Count - activePackages.Count;
    public int DeliveriesFailed => activePackages.Count;
    public IReadOnlyList<Package> ActivePackages => activePackages;
    public float TotalDamageTaken => totalDamageTaken;
    public int TotalPackagesDestroyed => totalPackagesDestroyed;
    public float TotalRemainingHealth => totalRemainingHealth;
    // Event to notify when the rating changes
    public event Action OnRatingChanged;

    private void Awake()
    {
        endingScreen.SetActive(false);
        if (packageSpawnPoint == null)
        {
            Debug.LogError("Package Spawn Point is not assigned in the inspector.");
            return;
        }

        if (deliveries.Count == 0)
        {
            Debug.LogError("Deliveries list is empty.");
            return;
        }

        activePackages.Clear();

        //For each delivery assignment, spawn the corresponding package prefab at the spawn point and set its destination
        foreach (DeliveryAssignment delivery in deliveries)
        {
            if (delivery == null || delivery.PackageData == null || delivery.PackageData.packagePrefab == null)
            {
                Debug.LogError("Delivery item is null or has no prefab assigned.");
                continue;
            }

            //Create a new package instance from the prefab and set its position and rotation to the spawn point
            GameObject packageObj = Instantiate(
                delivery.PackageData.packagePrefab,
                packageSpawnPoint.position,
                packageSpawnPoint.rotation
            );

            //Get the Package component from the spawned package object
            //Spawned package is added to the activePackages list and its destination is set to the delivery assignment's destination
            Package spawnedPackage = packageObj.GetComponent<Package>();
            //spawnedPackage.onPackageDestroyed += (pkg) => CompleteDelivery(pkg);
            if (spawnedPackage != null)
            {
                spawnedPackage.onPackageDestroyed += HandleDestroyedPackage;
                spawnedPackage.onPackageDamaged += HandlePackageDamaged;
                spawnedPackage.SetDestination(delivery.Destination);
                activePackages.Add(spawnedPackage);

                totalMaxHealth += spawnedPackage.PackageHealth;

                Debug.Log($"Spawned package: {packageObj.name}");
            }
            else
            {
                Debug.LogError(
                    $"{packageObj.name} does not have a Package component."
                );

                Destroy(packageObj);
            }
        }
    }
    private void NotifyRatingChanged()
    {
        OnRatingChanged?.Invoke();
    }

    public bool CompleteDelivery(Package package)
    {
        if (package == null)
        {
            Debug.LogError("Attempted to complete delivery for a null package.");
            return false;
        }

        if (!activePackages.Contains(package))
        {
            Debug.LogWarning($"Package {package.name} was not found.");
            return false;
        }

        totalRemainingHealth += package.PackageHealth;
        activePackages.Remove(package);
        CheckForCompleteMission();

        Debug.Log($"Package {package.name} delivered. {activePackages.Count} remaining.");
       
        Destroy(package.gameObject);

        return true;
    }

    private void CheckForCompleteMission()
    {
        if (activePackages.Count == 0)
        {
            int rating = CalculateRating();

            if (totalDamageText != null)
            {
                totalDamageText.text = $"Total Damage Taken: {Mathf.RoundToInt(totalDamageTaken)}";
            }

            Debug.Log($"Mission complete! Rating: {rating} stars.");
            Debug.Log($"Total damage taken: {totalDamageTaken}");

            endingScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void HandleDestroyedPackage(Package package)
    {
        if (package == null)
        {
            Debug.LogError("Attempted to handle a null package.");
            return;
        }

        if (!activePackages.Contains(package))
        {
            Debug.LogWarning($"{package.name} was not found.");
            return;
        }

        totalPackagesDestroyed++;
        activePackages.Remove(package);

        package.onPackageDestroyed -= HandleDestroyedPackage;

        Debug.Log(
            $"Package {package.name} destroyed. {activePackages.Count} remaining."
        );

        CheckForCompleteMission();
    }

    private void HandlePackageDamaged(float damage)
    {
        totalDamageTaken += damage;
        NotifyRatingChanged();
    }

    public int CalculateRating()
    {
        if (totalMaxHealth <= 0f)
            return 0;

        float combinedHealth = totalRemainingHealth;

        foreach (Package package in activePackages)
        {
            if (package != null)
            {
                combinedHealth += package.PackageHealth;
            }
        }

        float healthPercentage = Mathf.Clamp01(
            combinedHealth / totalMaxHealth
        );

        return Mathf.CeilToInt(healthPercentage * 3);
    }
}
