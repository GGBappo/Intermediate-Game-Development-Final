using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CargoManagerEditModeTests
{
    private GameObject managerObject;
    private CargoManager manager;
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        // Keeping this object inactive prevents CargoManager.Awake from
        // requiring scene dependencies during isolated EditMode tests.
        managerObject = new GameObject("CargoManager - test");
        managerObject.SetActive(false);
        manager = managerObject.AddComponent<CargoManager>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in createdObjects)
            if (obj != null) UnityEngine.Object.DestroyImmediate(obj);

        if (managerObject != null)
            UnityEngine.Object.DestroyImmediate(managerObject);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field, "Could not find field: " + fieldName);
        field.SetValue(target, value);
    }

    [Test]
    public void ZeroMaximumHealth_ReturnsZeroStars()
    {
        Assert.AreEqual(0, manager.CalculateRating());
    }

    [TestCase(0f, 0)]
    [TestCase(1f, 1)]
    [TestCase(33.33f, 1)]
    [TestCase(40f, 2)]
    [TestCase(66.66f, 2)]
    [TestCase(70f, 3)]
    [TestCase(100f, 3)]
    public void DeliveredHealth_ProducesExpectedRating(float health, int expected)
    {
        SetPrivateField(manager, "totalMaxHealth", 100f);
        SetPrivateField(manager, "totalRemainingHealth", health);

        Assert.AreEqual(expected, manager.CalculateRating());
    }

    [Test]
    public void ActiveAndDeliveredPackageHealth_AreCombined()
    {
        GameObject obj = new GameObject("Active package - test");
        obj.SetActive(false); // Do not invoke Package.Awake without a Package_SO.
        createdObjects.Add(obj);
        Package package = obj.AddComponent<Package>();
        package.packageHealth = 40f;

        SetPrivateField(manager, "totalMaxHealth", 100f);
        SetPrivateField(manager, "totalRemainingHealth", 30f);
        SetPrivateField(manager, "activePackages", new List<Package> { package });

        Assert.AreEqual(3, manager.CalculateRating()); // (30 + 40) / 100 = 70%
    }

    [Test]
    public void PackageDamage_RaisesEventAndReducesHealth()
    {
        GameObject obj = new GameObject("Damaged package - test");
        obj.SetActive(false);
        createdObjects.Add(obj);
        Package package = obj.AddComponent<Package>();
        package.packageHealth = 100f;

        float reportedDamage = -1f;
        int eventCount = 0;
        package.onPackageDamaged += amount =>
        {
            reportedDamage = amount;
            eventCount++;
        };

        MethodInfo takeDamage = typeof(Package).GetMethod(
            "TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(takeDamage);
        takeDamage.Invoke(package, new object[] { 12f });

        Assert.AreEqual(88f, package.PackageHealth);
        Assert.AreEqual(12f, reportedDamage);
        Assert.AreEqual(1, eventCount);
    }

    [Test]
    public void DamageLargerThanHealth_IsClampedToRemainingHealth()
    {
        GameObject obj = new GameObject("Low health package - test");
        obj.SetActive(false);
        createdObjects.Add(obj);
        Package package = obj.AddComponent<Package>();
        package.packageHealth = 10f;

        float reportedDamage = -1f;
        package.onPackageDamaged += amount => reportedDamage = amount;

        // Avoid the destruction path in EditMode: that path uses Destroy(),
        // which must be tested in PlayMode instead.
        MethodInfo takeDamage = typeof(Package).GetMethod(
            "TakeDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(takeDamage);
        takeDamage.Invoke(package, new object[] { 8f });

        Assert.AreEqual(2f, package.PackageHealth);
        Assert.AreEqual(8f, reportedDamage);
    }
}
