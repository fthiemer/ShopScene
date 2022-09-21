using UnityEngine;

/// <summary>
/// Holds item information and triggers placement on counter on click.
/// Needs to be attached to an item to make it buyable.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class BuyableObject : MonoBehaviour {


    /// <summary>
    /// The distance between center and lower edge. Used for correct placement on the counter.
    /// </summary>
    public float YOffset { get; private set; }

    [SerializeField] private string itemName;
    public string ItemName => itemName;

    [SerializeField] private float price;
    public float Price => price;

    [SerializeField] private Counter counter;


    private void Awake() {
        var tmpMeshFilter = gameObject.GetComponent<MeshRenderer>();
        YOffset = tmpMeshFilter.bounds.extents.y;
    }
    
    private void OnMouseDown() {
        counter.PlaceOnCounter(gameObject);
        Debug.Log($"Bought {ItemName} for {Price}");
    }
}
