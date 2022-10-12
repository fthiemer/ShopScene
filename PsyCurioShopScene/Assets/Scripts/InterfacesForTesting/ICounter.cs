using System.Collections.Generic;
using UnityEngine;

public interface ICounter {
    int MaxBuyableItems { get; }

    /// <summary>
    /// Holds the components with information about bought items
    /// </summary>
    Dictionary<int, (GameObject gameObject, Buyable buyable)> BoughtItems { get; }

    /// <summary>
    /// Place boughtItem on counter considering yOffset, if current number of placed items is below maxBuyableItems
    /// and add their Buyable components to boughtItem list.
    /// </summary>
    /// <param name="boughtItem"></param>
    GameObject PlaceOnCounter(GameObject boughtItem);

    /// <summary>
    /// Actualizes boughtItemsDict, then calls ItemSlot.DestroyObject().
    /// </summary>
    /// <param name="buyableToRemove"> Buyable Component of bought item to remove. </param>
    void RemoveItemFromCounter(Buyable buyableToRemove);
}