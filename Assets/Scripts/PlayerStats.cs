using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private CargoManager cargoManager;

    [SerializeField] private TextMeshProUGUI totalPackagesDestroyedText;
    [SerializeField] private TextMeshProUGUI totalPackagesDeliveredText;
    [SerializeField] private TextMeshProUGUI totalDamageTakenText;
    [SerializeField] private TextMeshProUGUI winOrLoseText;

    private void Update()
    {
        UpdateDamageTakenText();
        UpdatePackagesDeliveredText();
        UpdatePackagesDestroyedText();
    }

    private void UpdatePackagesDestroyedText()
    {
        totalPackagesDestroyedText.text =
            $"Packages Destroyed: {cargoManager.TotalPackagesDestroyed}";
    }

    private void UpdatePackagesDeliveredText()
    {
        if (cargoManager.DeliveriesCompleted != cargoManager.Deliveries.Count)
        {
            winOrLoseText.text = "You Lose!";
        }
        else
        {
            winOrLoseText.text = "You Win!";
        }

        totalPackagesDeliveredText.text =
            $"Packages Delivered: {cargoManager.DeliveriesCompleted}";
    }

    private void UpdateDamageTakenText()
    {
        totalDamageTakenText.text =
            $"Damage Taken: {Mathf.RoundToInt(cargoManager.TotalDamageTaken)}";
    }
}
