using UnityEngine;

[CreateAssetMenu(fileName = "Package_SO", menuName = "ScriptableObjects/Package_SO", order = 1)]
[RequireComponent(typeof(Rigidbody))]
public class Package_SO : ScriptableObject
{
    private void OnEnable()
    {
        // This method is called when the ScriptableObject is loaded or created.
        // You can initialize any data here if needed.
    }

    public GameObject packagePrefab;    
    public string packageName;
    public float packageHealth;
    public float crashModifier = 0.5f;
    public int floatModifier = 1;
}
