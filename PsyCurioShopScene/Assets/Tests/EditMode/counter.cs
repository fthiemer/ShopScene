using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Tests.EditMode {
    public class counter {
        private GameObject counterObject;
        private Counter counterComponent;
        private GameObject[] buyableItems;


        /// <summary>
        /// Open ShopScene and get Counter Object + buyable items.
        /// Doing this once instead of only instantiating objects for each test as needed has the advantage of
        /// testing the scene for correct setup as well + being efficient
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp() {
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            counterObject = GameObject.FindWithTag(Tags.Counter);
            counterComponent = counterObject.GetComponent<Counter>();
            ReflectionHelper.InvokePrivateVoidMethod(counterComponent, "Awake");
            buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
        }
    
        [Test]
        public void maxBuyableItems_equals_number_of_child_objects_after_Awake() {
            //ARRANGE -> happens in OneTimeSetUp()
            //ACT -> Awake call in OneTimeSetUp()
            //ASSERT
            Assert.AreEqual(counterObject.transform.childCount, counterComponent.MaxBuyableItems);
            //CLEANUP - nothing changed
        }    
    
        [Test]
        public void maxBuyableItems_equals_5_after_Awake() {
            //ARRANGE -> happens in OneTimeSetUp()
            //ACT -> Awake call in OneTimeSetUp()
            //ASSERT
            Assert.AreEqual(5, counterComponent.MaxBuyableItems);
            //CLEANUP - not necessary
        }
    
        [TestCase("6 3 0")]
        [TestCase("3 4 1")]
        [TestCase("1 2 3")]
        public void PlaceOnCounter_places_correct_and_at_most_maxBuyableItems_on_Counter(string batchSizesString) {
            //ARRANGE - synchronously load items
            //get input into list of ints
            var batchSizes = new List<int>(batchSizesString.Split(" ").Select(x => int.Parse(x)));
            int maxBuyableItems = counterComponent.MaxBuyableItems;
            List<GameObject> itemsToRemoveInCleanUp = new List<GameObject>(maxBuyableItems);
            List<GameObject> itemsPlacedOnCounterWasCalledOn = new List<GameObject>();
        
            //ACT - place each item batch size times
            for (int i = 0; i < batchSizes.Count; i++) {
                for (int j = 0; j < batchSizes[i]; j++) {
                    var placedItem = counterComponent.PlaceOnCounter(buyableItems[i]);
                    if (!ReferenceEquals(placedItem, null)) itemsToRemoveInCleanUp.Add(placedItem);
                    itemsPlacedOnCounterWasCalledOn.Add(buyableItems[i]);
                }
            }

            // ASSERT 1 - only maxBuyableItems should be instantiated on counter in scene
            int additionalItemsInScene = GameObject.FindGameObjectsWithTag(Tags.Item).Length - buyableItems.Length;
            Assert.AreEqual(maxBuyableItems, additionalItemsInScene);
        
            // ASSERT 2 - ensure only correct items (= first maxBuyableItems items)  were placed on counter
            var boughtItems = counterComponent.BoughtItems;
            for (int i = 0; i < boughtItems.Count; i++) {
                var tmpBuyableObject = itemsPlacedOnCounterWasCalledOn[i].GetComponent<BuyableObject>();
                Assert.AreEqual(tmpBuyableObject.ItemName, boughtItems[i].ItemName);
                Assert.AreEqual(tmpBuyableObject.Price, boughtItems[i].Price);
            }
        
            //CLEANUP
            RemovePlacedItems(itemsToRemoveInCleanUp);
            ResetCounterComponent();
        }
        
        [Test]
        public void PlaceOnCounter_places_every_item_with_correct_y_offset_in_first_counter_slot() {
            //ARRANGE - Happens in OneTimeSetUp()
            foreach (var item in buyableItems) {
                //ACT
                GameObject placedItem = counterComponent.PlaceOnCounter(item);
            
                //ASSERT
                var curBuyableObject = placedItem.GetComponent<BuyableObject>();
                var placementPosition = counterObject.transform.GetChild(0).position;
                var expectedPosition = Vector3.up * curBuyableObject.YOffset + placementPosition;
                Assert.AreEqual(expectedPosition, placedItem.transform.position);
            
                //CLEANUP
                //  Destroy placed Item before next gets placed so no interference or lack of slots can happen
                Object.DestroyImmediate(placedItem);
                //  Make sure Objects will be placed at same position again (in case counter positions change)
                //  and reset bought Items list
                ResetCounterComponent();
            }
        }

        [Test]
        public void PlaceOnCounter_sets_isBought_to_true() {
            //ARRANGE
            var item = buyableItems[^1];
        
            //ACT
            GameObject placedItem = counterComponent.PlaceOnCounter(item);
            
            //ASSERT
            var curBuyableObject = placedItem.GetComponent<BuyableObject>();
            Assert.IsTrue(curBuyableObject.IsBought);
        
            //CLEANUP
            Object.DestroyImmediate(placedItem);
            ResetCounterComponent();
        }
    
        [Test]
        public void PlaceOnCounter_correctly_adds_item_to_boughtItem_list() {
            //ARRANGE - choose item to place and prepare clone removal
            var item = buyableItems[0];
            int maxBuyableItems = counterComponent.MaxBuyableItems;
            List<GameObject> itemsToRemoveOnCleanUp = new List<GameObject>(maxBuyableItems);
        
            for (int i=0; i < maxBuyableItems; i++) {
                //ACT
                GameObject placedItem = counterComponent.PlaceOnCounter(item);
                itemsToRemoveOnCleanUp.Add(placedItem);
            
                //ASSERT
                var curBuyableObject = placedItem.GetComponent<BuyableObject>();
                Assert.AreEqual(curBuyableObject.ItemName, counterComponent.BoughtItems[i].ItemName);
                Assert.AreEqual(curBuyableObject.Price, counterComponent.BoughtItems[i].Price);
            }
        
            //CLEANUP
            RemovePlacedItems(itemsToRemoveOnCleanUp);
            ResetCounterComponent();
        }    
    
        [Test]
        public void PlaceOnCounter_when_boughtItems_has_maxBuyableItems_doesnt_place_more_items() {
            //ARRANGE - choose item to place and prepare clone removal
            var item = buyableItems[0];
            int maxBuyableItems = counterComponent.MaxBuyableItems;
            List<GameObject> itemsToRemoveOnCleanUp = new List<GameObject>(maxBuyableItems);
        
            //ACT - call PlaceOnCounter for one more item than maxbuyableItems
            for (int i=0; i < maxBuyableItems+1; i++) {
                GameObject placedItem = counterComponent.PlaceOnCounter(item);
                if(placedItem != null) itemsToRemoveOnCleanUp.Add(placedItem);
            }
        
            //ASSERT
            Assert.AreEqual(maxBuyableItems, counterComponent.BoughtItems.Count);
        
            //CLEANUP
            RemovePlacedItems(itemsToRemoveOnCleanUp);
            ResetCounterComponent();
        }
        
        /// <summary>
        /// Place all items and remove a single one
        /// </summary>
        /// <param name="itemID"> The id of the item to remove. 0 to 4 as those are the first 5 assigned.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void RemoveFromCounter_with_full_counter_doesnt_change_other_items_on_counter(int itemID) {
            //ARRANGE - place maxbuyable items, save positions, prices and names
            //  (except of item to remove) and prepare cleanup
            var maxBuyableItems = counterComponent.MaxBuyableItems;
            var notRemovedItems = new List<GameObject>(maxBuyableItems);
            var initialStates = 
                new List<(string Name, float Price, Vector3 Pos)>(maxBuyableItems);
            for (var i = 0; i < maxBuyableItems; i++) {
                var placedItem = counterComponent.PlaceOnCounter(buyableItems[i]);
                // dont add item that will be removed
                if (i == itemID) continue;
                // save to check for changes later
                var curPrice = counterComponent.BoughtItems[i].Price;
                var curName = counterComponent.BoughtItems[i].ItemName;
                initialStates.Add((Name: curName, 
                                        Price: curPrice, 
                                        Pos: placedItem.transform.position));
                // add to remove list
                notRemovedItems.Add(placedItem);
            }
            //ACT - remove item with itemID, here 
            counterComponent.RemoveItemFromCounter(itemID);
            //ASSERT - positions, names, prices are the same
            for (var i = 0; i < notRemovedItems.Count; i++) {
                var item = notRemovedItems[i];
                Assert.AreEqual( initialStates[i].Pos, item.transform.position);
                var buyableObjectComponent = item.GetComponent<BuyableObject>();
                Assert.AreEqual(initialStates[i].Name, buyableObjectComponent.ItemName);
                Assert.AreEqual(initialStates[i].Price, buyableObjectComponent.Price);
            }
            //CLEANUP 
            RemovePlacedItems(notRemovedItems);
            ResetCounterComponent();
        }
        
        
        /// <summary>
        /// Teardown Helper Method to remove the clone items spawned by PlaceOnCounter calls.
        /// </summary>
        /// <param name="itemsToRemoveInCleanUp"></param>
        private void RemovePlacedItems(List<GameObject> itemsToRemoveInCleanUp) {
            foreach (var item in itemsToRemoveInCleanUp) {
                Object.DestroyImmediate(item);
            }
            //Make sure the scene holds no bought items now
            Assert.AreEqual(buyableItems.Length, GameObject.FindGameObjectsWithTag(Tags.Item).Length);
        }

        /// <summary>
        /// Teardown Helper Method to reset the counter component by destroying and initializing it.
        /// </summary>
        private void ResetCounterComponent() {
            Object.DestroyImmediate(counterComponent);
            counterObject.AddComponent<Counter>();
            counterComponent = counterObject.GetComponent<Counter>();
            ReflectionHelper.InvokePrivateVoidMethod(counterComponent, "Awake");
        }
    }
}
