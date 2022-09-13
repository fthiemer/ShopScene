using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CashRegister : MonoBehaviour
{
    [SerializeField] private Shopkeeper shopkeeper;
    [SerializeField] private Counter counter;
    [SerializeField] private string zeroItemsBoughtMsg = "You chose: \n\nNothing. Nothing at all..";

    private string[] praisingAttributesList = new[]
        { "philosophical", "lightning-fast", "elegant", "magic", 
            "prestigious", "adventurous", "mystical", "fresh"};
    
    private void OnMouseDown() {
        shopkeeper.Say(MakeBillMessage());
    }

    /// <summary>
    /// Constructs bill message from boughtItems
    /// </summary>
    /// <returns> The bill string for the shopkeeper to say. </returns>
    private string MakeBillMessage() {
        int boughtItemsCount = counter.BoughtItems.Count;
        //Return standard message if no items were bought
        if ( boughtItemsCount == 0) return zeroItemsBoughtMsg;
        float totalPrice = 0f;
        //Count occurences of items in dict and calculate price
        var itemCountDict = new Dictionary<string, int>(5);
        foreach (BuyableObject buyableObject in counter.BoughtItems) {
            var tmpName = buyableObject.ItemName;
            if(itemCountDict.ContainsKey(tmpName)) {
                itemCountDict[tmpName] += 1;
            } else {
                itemCountDict.Add(tmpName, 1);
            }
            totalPrice += buyableObject.Price;
        }
        
        //Build bill Message
        StringBuilder billText = new StringBuilder("You selected:\n");
        string itemSuffix;
        foreach (string key in itemCountDict.Keys) {
            int curItemCount = itemCountDict[key];
            itemSuffix = curItemCount == 1 ? "" : "s";
            int praiseIndex = Random.Range(0, counter.MaxBuyableItems);
            billText.Append($"{curItemCount} {praisingAttributesList[praiseIndex]} {key}{itemSuffix}!!\n");
        }
        string summaryWord = counter.BoughtItems.Count == 1 ? "it" : "everything";
        billText.Append($"\n For only {totalPrice} Robobucks you can take {summaryWord} with you.");
        return billText.ToString();
    }
    

}
