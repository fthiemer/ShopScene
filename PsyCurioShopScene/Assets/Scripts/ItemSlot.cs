using UnityEngine;
using Object = UnityEngine.Object;

public class ItemSlot : MonoBehaviour {
    /// <summary>
    /// Index to keep boughtItemList in Counter actualized.
    /// </summary>
    public int Index {
        get => index;
        set {
            if (indexIsSet) return;
            index = value;
            indexIsSet = true;
        }
    }
    public bool SlotUsed { get; private set; }
    public Vector3 Position { get; private set; }
    public GameObject ObjectInSlot;
    public Buyable BuyableComponentInSlot;
    private bool indexIsSet;
    private int index;

    public void DestroyObject() {
        SlotUsed = false;
        Object.Destroy(ObjectInSlot);
    }

    public void PutIntoSlot(GameObject objectForSlot) {
        SlotUsed = true;
        ObjectInSlot = objectForSlot;
        BuyableComponentInSlot = ObjectInSlot.GetComponent<Buyable>();
        BuyableComponentInSlot.ItemSlot = this;
        BuyableComponentInSlot.MarkAsBought();
    }

    public void SetPosition(Vector3 rootPosition) {
        Position = rootPosition;
    }
}
