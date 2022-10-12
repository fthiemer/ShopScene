using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Creates bill message on click and lets shopkeeper say it.
/// Needs to access counter for list of bought items.
/// </summary>
public class CashRegister : MonoBehaviour, IPointerClickHandler {
    public static UnityEvent OnBoughtItemsChange = new UnityEvent();
    public ICounter counter;
    public IShopkeeper shopkeeper;
    [SerializeField] private string zeroItemsBoughtMsg = "You chose: \n\nNothing. Nothing at all..";
    private bool billRequested;
    
    private void Awake() {
        gameObject.tag = Tags.CashRegister;
    }

    private void Start() {
        counter = GameObject.FindWithTag(Tags.Counter).GetComponent<Counter>();
        shopkeeper = GameObject.FindWithTag(Tags.Shopkeeper).GetComponent<Shopkeeper>();
    }

    public void OnPointerClick (PointerEventData eventData) {
        ShowBill();
    }


    private void ShowBill() {
        shopkeeper.Say(ConstructBillMessage());
        if (billRequested) return;
        OnBoughtItemsChange.AddListener(ShowBill);
        billRequested = true;
    }
    
    
    
    /// <summary>
    /// Constructs bill message from boughtItems.
    /// </summary>
    /// <returns> The bill string for the shopkeeper to say. </returns>
    private string ConstructBillMessage() {
        int boughtItemsCount = counter.BoughtItems.Count;
        //Return standard message if no items were bought
        if ( boughtItemsCount == 0) return zeroItemsBoughtMsg;
        Debug.Assert(boughtItemsCount <= counter.MaxBuyableItems);
        float totalPrice = 0f;
        //Count occurences of items in dict and calculate price
        //Store mapping of itemName to (pieces bought, price of one instance) in dict
        var itemCountDict = new Dictionary<string, (int, float)>(counter.MaxBuyableItems); 
        foreach (Buyable buyable in counter.BoughtItems.Values.Select(t=>t.buyable)) {
            var tmpName = buyable.ItemName;
            if(itemCountDict.ContainsKey(tmpName)) {
                var countPriceTuple = itemCountDict[tmpName];
                countPriceTuple.Item1 += 1;
                itemCountDict[tmpName] = countPriceTuple;
            } else {
                itemCountDict.Add(tmpName, (1, buyable.Price));
            }
            totalPrice += buyable.Price;
        }
        
        //Build bill Message - item order is not guaranteed
        // CultureInfo.. is used, so that message uses floating points instead of comma
        StringBuilder billText = new StringBuilder("You selected:\n");
        string itemSuffix;
        foreach (string key in itemCountDict.Keys) {
            int curItemCount = itemCountDict[key].Item1;
            float curItemPrice = itemCountDict[key].Item2;
            float combinedPrice = curItemCount * curItemPrice;
            itemSuffix = curItemCount == 1 ? "" : "s";
            billText.Append($"{curItemCount} {key}{itemSuffix}" +
                            $" for {combinedPrice.ToString("#.##",CultureInfo.InvariantCulture)}!!\n");
        }
        string summaryWord = counter.BoughtItems.Count == 1 ? "it" : "everything";
        billText.Append($"\nFor only {totalPrice.ToString("#.##",CultureInfo.InvariantCulture)}" +
                        $" Robodollars you can take {summaryWord} with you.");
        return billText.ToString();
    }


}
