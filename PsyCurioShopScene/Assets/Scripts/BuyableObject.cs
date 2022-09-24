using UnityEngine;

/// <summary>
/// Holds item information and triggers placement on counter on click.
/// Attach it to a GameObject to make the GameObject buyable.
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

    private Counter counter;


    private void Awake() {
        gameObject.tag = Tags.Item;
        var tmpMeshFilter = gameObject.GetComponent<MeshRenderer>();
        YOffset = tmpMeshFilter.bounds.extents.y;
    }

    private void Start() {
        counter = GameObject.FindWithTag(Tags.Counter)?.GetComponent<Counter>();
    }
    
    private void OnMouseDown() {
        counter.PlaceOnCounter(gameObject);
        Debug.Log($"Bought {ItemName} for {Price}");
    }
}
