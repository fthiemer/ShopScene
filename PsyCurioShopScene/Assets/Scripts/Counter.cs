using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour, ICounter {
    private List<Transform> boughtItemTargetPositions;
    private int maxBuyableItems;
    /// <summary>
    /// Holds the components with information about bought items
    /// </summary>
    private List<BuyableObject> boughtItems;
    public int MaxBuyableItems => maxBuyableItems;
    public List<BuyableObject> BoughtItems => boughtItems;

    private void Awake() {
        gameObject.tag = Tags.Counter;
        //Allow to place as many items, as there are positions on the counter,
        // as indicated by targetPosition gameObjects, which are child to the counter
        maxBuyableItems = gameObject.transform.childCount;
        boughtItemTargetPositions = new List<Transform>(maxBuyableItems);
        foreach (Transform child in transform) {
            boughtItemTargetPositions.Add(child);
        }
        boughtItems = new List<BuyableObject>(maxBuyableItems);
    }

    /// <summary>
    /// Place boughtItem on counter considering yOffset, if current number of placed items is below maxBuyableItems
    /// and add their BuyableObject components to boughtItem list.
    /// </summary>
    /// <param name="boughtItem"></param>
    public GameObject PlaceOnCounter(GameObject boughtItem) {
        if (boughtItems.Count >= MaxBuyableItems) return null;
        // Get BuyableObject component for later cash register use
        var curBuyableObject = boughtItem.GetComponent<BuyableObject>();
        boughtItems.Add(curBuyableObject);
        
        //Clone and place Object with fitting offset
        Vector3 curOffset = new Vector3(0f, curBuyableObject.YOffset, 0f);
        Vector3 targetPos = boughtItemTargetPositions[boughtItems.Count - 1].position + curOffset;
        GameObject tmpPlacedObject = Instantiate(boughtItem, targetPos, Quaternion.identity);
        //Mark clone as already bought, so clicking it, wont work anymore
        tmpPlacedObject.GetComponent<BuyableObject>().MarkAsBought();
        return tmpPlacedObject;
    }
}
