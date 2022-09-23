using System.Collections.Generic;
using UnityEngine;

public interface ICounter {
    int MaxBuyableItems { get; }

    /// <summary>
    /// Holds the components with information about bought items
    /// </summary>
    List<BuyableObject> BoughtItems { get; }

    /// <summary>
    /// Place boughtItem on counter, if current number of placed items is below maxBuyableItems.
    /// </summary>
    /// <param name="boughtItem"></param>
    void PlaceOnCounter(GameObject boughtItem);
}