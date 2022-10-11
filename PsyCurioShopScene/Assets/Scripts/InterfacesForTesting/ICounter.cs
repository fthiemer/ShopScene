using System.Collections.Generic;
using UnityEngine;

public interface ICounter {
    int MaxBuyableItems { get; }

    /// <summary>
    /// Holds the components with information about bought items
    /// </summary>
    List<BuyableObject> BoughtItems { get; }

    /// <summary>
    /// Place boughtItem on counter considering yOffset, if current number of placed items is below maxBuyableItems
    /// and add their BuyableObject components to boughtItem list.
    /// </summary>
    /// <param name="boughtItem"></param>
    GameObject PlaceOnCounter(GameObject boughtItem);

    void RemoveItemFromCounter(int uniqueID);
}