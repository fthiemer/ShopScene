using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Holds item information and triggers placement on counter on click.
/// Attach it to a GameObject to make the GameObject buyable.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class BuyableObject : MonoBehaviour, IPointerClickHandler {
    /// <summary>
    /// The distance between center and lower edge. Used for correct placement on the counter.
    /// </summary>
    public ICounter Counter { get; set; }
    public float YOffset { get; private set; }
    public string ItemName => itemName;
    public float Price => price;
    public bool IsBought => isBought;
    public int UniqueID => uniqueID;

    [SerializeField] private float price;
    [SerializeField] private string itemName;

    private bool isBought;
    private int uniqueID;

    private void Awake() {
        gameObject.tag = Tags.Item;
        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        YOffset = meshRenderer.bounds.extents.y;
    }

    private void Start() {
        Counter = GameObject.FindWithTag(Tags.Counter)?.GetComponent<Counter>();
    }

    public void OnPointerClick (PointerEventData eventData) {
        //Only react to click, if item was bought
        if (isBought) return; 
        Counter.PlaceOnCounter(gameObject);
        Debug.Log($"Bought {ItemName} for {Price}");
    }

    public void MarkAsBought() {
        isBought = true;
    }
}
