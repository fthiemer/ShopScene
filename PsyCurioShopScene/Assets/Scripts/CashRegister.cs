using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Creates bill message on click and lets shopkeeper say it.
/// Needs to access counter for list of bought items.
/// </summary>
public class CashRegister : MonoBehaviour
{
    [SerializeField] private Shopkeeper shopkeeper;
    [SerializeField] private Counter counter;
    [SerializeField] private string zeroItemsBoughtMsg = "You chose: \n\nNothing. Nothing at all..";
    
    /// <summary>
    /// List of praising attributes used randomly by the shopkeeper to
    /// describe items.
    /// </summary>
    private string[] _praisingAttributesList = new[]
        { "philosophical", "lightning-fast", "elegant", "magic", 
            "prestigious", "adventurous", "mystical", "fresh"};
    
    private void OnMouseDown() {
        shopkeeper.Say(MakeBillMessage());
    }

    /// <summary>
    /// Constructs bill message from boughtItems.
    /// </summary>
    /// <returns> The bill string for the shopkeeper to say. </returns>
    private string MakeBillMessage() {
        int boughtItemsCount = counter.BoughtItems.Count;
        //Return standard message if no items were bought
        if ( boughtItemsCount == 0) return zeroItemsBoughtMsg;
        float totalPrice = 0f;
        //Count occurences of items in dict and calculate price
        //Store name -> (pieces bought, price of one instance) in dict
        var itemCountDict = new Dictionary<string, (int, float)>(5); 
        foreach (BuyableObject buyableObject in counter.BoughtItems) {
            var tmpName = buyableObject.ItemName;
            if(itemCountDict.ContainsKey(tmpName)) {
                var countPriceTuple = itemCountDict[tmpName];
                countPriceTuple.Item1 += 1;
            } else {
                itemCountDict.Add(tmpName, (1, buyableObject.Price));
            }
            totalPrice += buyableObject.Price;
        }
        
        //Build bill Message
        StringBuilder billText = new StringBuilder("You selected:\n");
        string itemSuffix;
        foreach (string key in itemCountDict.Keys) {
            int curItemCount = itemCountDict[key].Item1;
            float curItemPrice = itemCountDict[key].Item2;
            itemSuffix = curItemCount == 1 ? "" : "s";
            int praiseIndex = Random.Range(0, counter.MaxBuyableItems);
            billText.Append($"{curItemCount} {_praisingAttributesList[praiseIndex]} {key}{itemSuffix}" +
                            $" for {curItemCount * curItemPrice}!!\n");
        }
        string summaryWord = counter.BoughtItems.Count == 1 ? "it" : "everything";
        billText.Append($"\n For only {totalPrice} Robobucks you can take {summaryWord} with you.");
        return billText.ToString();
    }
    

}
