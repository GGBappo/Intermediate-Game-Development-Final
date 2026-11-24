
using UnityEngine;
using UnityEngine.UI;

public class PackageRatingUI : MonoBehaviour
{
    [SerializeField] private CargoManager cargoManager;
    [SerializeField] private Image[] stars;

    private void Awake()
    {
        if (cargoManager == null)
        {
            Debug.LogError("CargoManager reference is not assigned in the inspector.");
            return;
        }
        if (stars == null || stars.Length == 0)
        {
            Debug.LogError("Stars array is not assigned or empty in the inspector.");
            return;
        }

        for(int i = 0; i < stars.Length; i++)
        {
            stars[i].color = Color.white; // Initialize all stars to white
        }
    }

    private void OnEnable()
    {
        if (cargoManager == null)
            return;

        cargoManager.OnRatingChanged += UpdateStars;
        UpdateStars();
    }

    private void OnDisable()
    {
        if (cargoManager != null)
            cargoManager.OnRatingChanged -= UpdateStars;
    }

    private void UpdateStars()
    {
        if (cargoManager == null)
            return;

        int rating = cargoManager.CalculateRating();

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            stars[i].color = i < rating
                ? Color.white
                : Color.gray;
        }
    }
}
