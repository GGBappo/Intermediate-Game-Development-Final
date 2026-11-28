using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DeliveryPlayModeTests
{
    private readonly List<GameObject> sceneObjects = new List<GameObject>();
    private Package_SO packageData;
    private GameObject managerObject;
    private CargoManager manager;
    private CheckPoint correctCheckpoint;
    private Package spawnedPackage;

    private GameObject Track(string name, Vector3 position)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        sceneObjects.Add(obj);
        return obj;
    }

    private static void SetPrivateField(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(
            name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, "Missing field: " + name);
        field.SetValue(target, value);
    }

    private CheckPoint CreateCheckpoint(string name, Vector3 position)
    {
        GameObject obj = Track(name, position);
        BoxCollider trigger = obj.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = Vector3.one * 3f;
        CheckPoint checkpoint = obj.AddComponent<CheckPoint>();
        SetPrivateField(checkpoint, "cargoManager", manager);
        return checkpoint;
    }

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;

        managerObject = Track("Test cargo manager", Vector3.zero);
        managerObject.SetActive(false);
        manager = managerObject.AddComponent<CargoManager>();

        GameObject endingScreen = Track("Ending screen", Vector3.zero);
        GameObject spawnPoint = Track("Spawn point", new Vector3(-6f, 0f, 0f));

        correctCheckpoint = CreateCheckpoint("Correct checkpoint", Vector3.zero);

        packageData = ScriptableObject.CreateInstance<Package_SO>();
        packageData.packageName = "Test parcel";
        packageData.packageHealth = 100f;
        packageData.crashModifier = 1f;
        packageData.floatModifier = 1f;

        // Build a runtime prefab substitute without loading the real scene.
        GameObject source = Track("Package source", new Vector3(1000f, 0f, 0f));
        source.SetActive(false);
        source.AddComponent<BoxCollider>();
        Rigidbody sourceBody = source.AddComponent<Rigidbody>();
        sourceBody.useGravity = false;
        Package sourcePackage = source.AddComponent<Package>();
        SetPrivateField(sourcePackage, "packageData", packageData);
        packageData.packagePrefab = source;
        source.SetActive(true); // Package.Awake can now read its ScriptableObject.

        DeliveryAssignment assignment = new DeliveryAssignment();
        SetPrivateField(assignment, "packageData", packageData);
        SetPrivateField(assignment, "destination", correctCheckpoint);

        SetPrivateField(manager, "deliveries", new List<DeliveryAssignment> { assignment });
        SetPrivateField(manager, "packageSpawnPoint", spawnPoint.transform);
        SetPrivateField(manager, "endingScreen", endingScreen);

        managerObject.SetActive(true); // Calls Awake and spawns one package.
        Assert.AreEqual(1, manager.ActivePackages.Count);
        spawnedPackage = manager.ActivePackages[0];
        Assert.NotNull(spawnedPackage);
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;

        foreach (GameObject obj in sceneObjects)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }
        sceneObjects.Clear();

        if (spawnedPackage != null)
            Object.DestroyImmediate(spawnedPackage.gameObject);

        if (packageData != null)
            Object.DestroyImmediate(packageData);
    }

    [UnityTest]
    public IEnumerator CorrectPackageEnteringCheckpoint_CompletesDelivery()
    {
        Rigidbody body = spawnedPackage.GetComponent<Rigidbody>();
        body.position = correctCheckpoint.transform.position;
        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(0, manager.ActivePackages.Count);
        Assert.AreEqual(100f, manager.TotalRemainingHealth);
        Assert.AreEqual(3, manager.CalculateRating());
        Assert.AreEqual(0f, Time.timeScale);
    }

    [UnityTest]
    public IEnumerator WrongCheckpoint_DoesNotCompleteDelivery()
    {
        CheckPoint wrongCheckpoint = CreateCheckpoint(
            "Wrong checkpoint", new Vector3(10f, 0f, 0f));

        Rigidbody body = spawnedPackage.GetComponent<Rigidbody>();
        body.position = wrongCheckpoint.transform.position;
        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(1, manager.ActivePackages.Count);
        Assert.AreEqual(0f, manager.TotalRemainingHealth);
        Assert.AreEqual(3, manager.CalculateRating());
    }

    [Test]
    public void TakingDamage_NotifiesManagerAndUpdatesLiveRating()
    {
        int notifications = 0;
        manager.OnRatingChanged += () => notifications++;

        MethodInfo takeDamage = typeof(Package).GetMethod(
            "TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(takeDamage);
        takeDamage.Invoke(spawnedPackage, new object[] { 70f });

        Assert.AreEqual(30f, spawnedPackage.PackageHealth);
        Assert.AreEqual(70f, manager.TotalDamageTaken);
        Assert.AreEqual(1, notifications);
        Assert.AreEqual(1, manager.CalculateRating());
    }

    [Test]
    public void DestroyedPackage_IsRemovedAndCountsAsFailedCargo()
    {
        MethodInfo takeDamage = typeof(Package).GetMethod(
            "TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(takeDamage);
        takeDamage.Invoke(spawnedPackage, new object[] { 100f });

        Assert.AreEqual(0, manager.ActivePackages.Count);
        Assert.AreEqual(1, manager.TotalPackagesDestroyed);
        Assert.AreEqual(0, manager.CalculateRating());
        Assert.AreEqual(0f, Time.timeScale);
    }
}
