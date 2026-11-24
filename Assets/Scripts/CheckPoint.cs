
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private List<GameObject> checkpointObjects;
    [SerializeField] private CargoManager cargoManager;

    private bool packageDelivered = false;

    private void OnTriggerEnter(Collider other)
    {
        Package package = other.GetComponentInParent<Package>();

        if (package != null &&
            package.CheckPoint == this &&
            !packageDelivered)
        {
            if (cargoManager.CompleteDelivery(package))
            {
                packageDelivered = true;

                Debug.Log($"Package {package.PackageData.packageName} delivered to {gameObject.name}!");

                // Later: Update checkpoint visuals
                // Later: Update delivery UI
            }
        }
    }
}
