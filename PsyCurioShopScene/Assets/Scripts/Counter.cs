using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class Counter : MonoBehaviour, ICounter {
    public int MaxBuyableItems => maxBuyableItems;
    public List<ItemSlot> ItemSlots => itemSlots;
    /// <summary>
    /// Keeps a (gameObject, buyable) tuple of bought items, the key to access them is the index
    /// of the ItemSlot they are in.
    /// </summary>
    public Dictionary<int, (GameObject gameObject, Buyable buyable)> BoughtItems => boughtItemsDict;

    private int maxBuyableItems;
    private Dictionary<int, (GameObject gameObject, Buyable buyable)> boughtItemsDict;
    /// <summary>
    /// List of item slots on counter, which hold references, positions and if there is an item in them.
    /// Initialized in Awake from the children of this scripts GameObject.
    /// </summary>
    private List<ItemSlot> itemSlots;

    private void Awake() {
        var tmpGameObject = gameObject;
        tmpGameObject.tag = Tags.Counter;
        //Allow to place as many items, as there are positions on the counter,
        // as indicated by targetPosition gameObjects, which are child to the counter
        maxBuyableItems = tmpGameObject.transform.childCount;
        //Prepare itemSlots
        itemSlots = new List<ItemSlot>(maxBuyableItems);
        int currentSlotIndex = 0;
        foreach (Transform child in transform.Cast<Transform>().OrderBy(t=>t.name)) {
            var curItemSlot = child.GetComponent<ItemSlot>();
            curItemSlot.Index = currentSlotIndex++;
            curItemSlot.SetPosition(child.transform.position);
            itemSlots.Add(curItemSlot);
        }
        //Initialize 
        boughtItemsDict = new Dictionary<int, (GameObject gameObject, Buyable buyable)>(maxBuyableItems);
        
    }

    /// <summary>
    /// Place boughtItem on counter considering yOffset, but only if current number of placed items is below
    /// maxBuyableItems. Also handles slot management.
    /// </summary>
    /// <param name="boughtItem"> Game object with buyable component that should be copied to the counter.</param>
    public GameObject PlaceOnCounter(GameObject boughtItem) {
        // Dont place if maxBuyableItems already bought
        if (boughtItemsDict.Count >= MaxBuyableItems) return null;
        // Determine first unused slot in itemSlots and make it the target slot.
        ItemSlot targetSlot = null;
        foreach (var slot in itemSlots) {
            if (slot.SlotUsed) continue;
            targetSlot = slot;
            break;
        }
        Debug.Assert(targetSlot != null, nameof(targetSlot) + " != null");
        
        // Calculate targetPosition and place clone
        var curBuyable = boughtItem.GetComponent<Buyable>();
        Vector3 curOffset = new Vector3(0f, curBuyable.YOffset, 0f);
        Vector3 targetPos = targetSlot.Position + curOffset;
        GameObject tmpPlacedObject = Instantiate(boughtItem, targetPos, Quaternion.identity);
        targetSlot.PutIntoSlot(tmpPlacedObject);
        
        // Update boughtItemsDict, then return placed object
        boughtItemsDict[targetSlot.Index] = (targetSlot.ObjectInSlot, targetSlot.BuyableComponentInSlot);
        return tmpPlacedObject;
    }

    /// <summary>
    /// Actualizes boughtItemsDict, then calls ItemSlot.DestroyObject().
    /// </summary>
    /// <param name="buyableToRemove"> Buyable Component of bought item to remove. </param>
    public void RemoveItemFromCounter(Buyable buyableToRemove) {
        // Remove from boughtItemsDict
        boughtItemsDict.Remove(buyableToRemove.ItemSlot.Index);
        // Destroy and empty slot
        buyableToRemove.ItemSlot.DestroyObject();
    }
}
