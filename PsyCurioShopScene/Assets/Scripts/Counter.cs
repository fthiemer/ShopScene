using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Counter : MonoBehaviour {
    private List<Transform> boughtItemTargetPositions;
    private int maxBuyableItems;
    public int MaxBuyableItems => maxBuyableItems;

    /// <summary>
    /// Holds the components with information about bought items
    /// </summary>
    public List<BuyableObject> BoughtItems { get; private set; }

    private void Awake() {
        gameObject.tag = Tags.Counter;
        //Allow to place as many items, as there are positions on the counter,
        // as indicated by targetPosition gameObjects, which are child to the counter
        maxBuyableItems = gameObject.transform.childCount;
        boughtItemTargetPositions = new List<Transform>(maxBuyableItems);
        for (int i = 0; i < maxBuyableItems; i++) {
            boughtItemTargetPositions.Add(transform.GetChild(i));
        }
        BoughtItems = new List<BuyableObject>();
    }

    /// <summary>
    /// Place boughtItem on counter, if current number of placed items is below maxBuyableItems.
    /// </summary>
    /// <param name="boughtItem"></param>
    public void PlaceOnCounter(GameObject boughtItem) {
        if (BoughtItems.Count >= MaxBuyableItems) return;
        // Get BuyableObject component for later cash register use
        var curBuyableObject = boughtItem.GetComponent<BuyableObject>();
        BoughtItems.Add(curBuyableObject);
        //Clone and place Object with fitting offset
        Vector3 curOffset = new Vector3(0f, curBuyableObject.YOffset, 0f);
        Vector3 targetPos = boughtItemTargetPositions[BoughtItems.Count - 1].position + curOffset;
        GameObject tmpPlacedObject = Instantiate(boughtItem, targetPos, Quaternion.identity);
        //Remove BuyableObject component, so the object on the counter cant trigger a new buy
        var componentToRemove = tmpPlacedObject.GetComponent<BuyableObject>();
        Destroy(componentToRemove);
    }
}
