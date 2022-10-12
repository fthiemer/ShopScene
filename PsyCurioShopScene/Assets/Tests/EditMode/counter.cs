using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
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
        [SetUp]
        public void SetUp() {
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
                var tmpBuyableObject = itemsPlacedOnCounterWasCalledOn[i].GetComponent<Buyable>();
                Assert.AreEqual(tmpBuyableObject.ItemName, boughtItems[i].buyable.ItemName);
                Assert.AreEqual(tmpBuyableObject.Price, boughtItems[i].buyable.Price);
            }
        
            //CLEANUP
            RemovePlacedItems(itemsToRemoveInCleanUp);
            ResetCounterComponent();
        }
        
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void PlaceOnCounter_places_every_item_with_correct_y_offset_in_first_counter_slot(int itemIndex) {
            //ARRANGE - Happens in OneTimeSetUp()
            //ACT
            GameObject placedItem = counterComponent.PlaceOnCounter(buyableItems[itemIndex]);
        
            //ASSERT
            var curBuyableObject = placedItem.GetComponent<Buyable>();
            var placementPosition = counterObject.transform.GetChild(0).transform.position;
            var expectedPosition = Vector3.up * curBuyableObject.YOffset + placementPosition;
            var actualPosition = placedItem.transform.position;
            Debug.Log("Ladada");
            Assert.AreEqual(expectedPosition, actualPosition);
        
            //CLEANUP - Scene reloaded in Setup
        }

        [Test]
        public void PlaceOnCounter_sets_isBought_to_true() {
            //ARRANGE
            var item = buyableItems[^1];
        
            //ACT
            GameObject placedItem = counterComponent.PlaceOnCounter(item);
            
            //ASSERT
            var curBuyableObject = placedItem.GetComponent<Buyable>();
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
                var curBuyableObject = placedItem.GetComponent<Buyable>();
                Assert.AreEqual(curBuyableObject.ItemName, counterComponent.BoughtItems[i].buyable.ItemName);
                Assert.AreEqual(curBuyableObject.Price, counterComponent.BoughtItems[i].buyable.Price);
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

        [Test]
        public void PlaceOnCounter_places_maxBuyableItems_in_correct_order() {
            //ARRANGE
            var placedItems = new List<GameObject>(counterComponent.MaxBuyableItems);

            //ACT - buy maxBuyableItems
            int i;
            for (i = 0; i < counterComponent.MaxBuyableItems; i++) {
                // If MaxBuyableItems is bigger than 
                int itemIndex = i % buyableItems.Length;
                placedItems.Add(counterComponent.PlaceOnCounter(buyableItems[itemIndex]));
            }
            
            //ASSERT - iterate over ItemSlots (child of countergameobject) sorted after name
            // and items in the ItemSlot component should be in the same order as in placedItems
            i = 0;
            foreach (Transform child in counterObject.transform.Cast<Transform>()
                                                            .OrderBy(t => t.name)) {
                var itemSlot = child.GetComponent<ItemSlot>();
                Assert.AreSame(placedItems[i], itemSlot.ObjectInSlot);
                i++;
            }
            //CLEANUP - not necessary as scene is reloaded in Setup
        }

        [Test]
        public void Awake_fills_ItemSlots_list_in_order_of_childrenNames() {
            //ARRANGE - Awake was called in Setup
            //ACT - not necessary
            //ASSERT
            var expectedItemSlotObjects = new List<GameObject>(counterComponent.MaxBuyableItems);
            var expectedItemSlots = new List<ItemSlot>(counterComponent.MaxBuyableItems);
            int currentSlotIndex = 0;
            foreach (Transform child in counterObject.transform.Cast<Transform>().OrderBy(t=>t.name)) {
                var curItemSlot = child.GetComponent<ItemSlot>();
                expectedItemSlots.Add(curItemSlot);
                expectedItemSlotObjects.Add(child.gameObject);
            }

            for (int i = 0; i < counterComponent.ItemSlots.Count; i++) {
                Assert.AreSame(expectedItemSlotObjects[i], counterComponent.ItemSlots[i].gameObject);
                Assert.AreSame(expectedItemSlots[i], counterComponent.ItemSlots[i]);
            }
            //CLEANUP - done by teardown and setup
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
