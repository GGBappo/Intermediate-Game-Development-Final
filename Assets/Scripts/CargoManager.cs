using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CargoManager : MonoBehaviour
{
    List<Package_SO> cargoList = new List<Package_SO>();
    public void AddCargo(Package_SO package)
    {
        cargoList.Add(package);
    }

    private void CalculateRating()
    {

    }


}
