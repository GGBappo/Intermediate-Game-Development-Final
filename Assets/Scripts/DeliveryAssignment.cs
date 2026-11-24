using System;
using UnityEngine;

[Serializable]
public class DeliveryAssignment
{
    [SerializeField] private Package_SO packageData;
    [SerializeField] private CheckPoint destination;

    public Package_SO PackageData => packageData;
    public CheckPoint Destination => destination;

}
