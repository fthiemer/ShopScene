using System;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour {
    [SerializeField] private List<Transform> boughtObjectPositionsOnCounter;
    
    private int maxBuyableItems;
    /// <summary>
    /// Holds the components with information about bought items
    /// </summary>
    private List<BuyableObject> boughtItems;

    private void Awake() {
        //Allow to place as many items, as there are positions on the counter
        maxBuyableItems = boughtObjectPositionsOnCounter.Count;
        boughtItems = new List<BuyableObject>();
    }

    /// <summary>
    /// Place boughtItem on counter, if current number of placed items is below maxBuyableItems.
    /// </summary>
    /// <param name="boughtItem"></param>
    public void PlaceOnCounter(GameObject boughtItem) {
        if (boughtItems.Count >= maxBuyableItems) return;
        // Get BuyableObject component for cash register
        var curBuyableObject = boughtItem.GetComponent<BuyableObject>();
        boughtItems.Add(curBuyableObject);
        //Place Object with fitting offset
        Vector3 curOffset = new Vector3(0f, curBuyableObject.YOffset, 0f);
        Vector3 targetPos = boughtObjectPositionsOnCounter[boughtItems.Count - 1].position + curOffset;
        GameObject tmpPlacedObject = Instantiate(boughtItem, targetPos, Quaternion.identity);
        //Remove BuyableObject component, so the object on the counter is not clickable
        var componentToRemove = tmpPlacedObject.GetComponent<BuyableObject>();
        Destroy(componentToRemove);
    }
}
