using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Holds item information and triggers placement on counter on click.
/// Attach it to a GameObject to make the GameObject buyable.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class Buyable : MonoBehaviour, IPointerClickHandler {
    public ICounter ResponsibleCounter { get; set; }
    public ItemSlot ItemSlot { get; set; }
    /// <summary>
    /// The distance between center and lower edge. Used for correct placement on the counter.
    /// </summary>
    public float YOffset { get; private set; }
    public string ItemName => itemName;
    public float Price => price;
    public bool IsBought => isBought;

    [SerializeField] private float price;
    [SerializeField] private string itemName;

    private bool isBought;

    private void Awake() {
        gameObject.tag = Tags.Item;
        var meshRenderer = gameObject.GetComponent<MeshRenderer>();
        YOffset = meshRenderer.bounds.extents.y;
    }

    private void Start() {
        ResponsibleCounter = GameObject.FindWithTag(Tags.Counter)?.GetComponent<Counter>();
        if (ResponsibleCounter == null) {
            Debug.LogError("Counter is null. Perhaps the Counter Tag was removed?");
        }
    }

    public void OnPointerClick (PointerEventData eventData) {
        //React to click depending on whether item is bought
        if (isBought) {
            ResponsibleCounter.RemoveItemFromCounter(this);
            Debug.Log($"Put back {itemName}");
        } else {
            ResponsibleCounter.PlaceOnCounter(gameObject);
            Debug.Log($"Bought {itemName} for {price}");
        }
    }

    public void MarkAsBought() {
        isBought = true;
    }
}
