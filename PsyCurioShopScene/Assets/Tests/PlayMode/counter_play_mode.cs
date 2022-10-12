using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode {
    public class counter_play_mode : InputTestFixture {
        private GameObject[] buyableObjects;
        private Buyable[] buyableComponents;
        private ICounter counterComponent;
        private bool sceneIsLoaded;
        private bool referencesAreSetUp;
        
        [SetUp]
        public override void Setup() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Assets/Scenes/ShopScene.unity", LoadSceneMode.Single);
            base.Setup();
        }

        [TearDown]
        public override void TearDown() {
            base.TearDown();
            // Make sure Scene is loaded and setup again for each test
            sceneIsLoaded = false;
            referencesAreSetUp = false;
        }

        /// <summary>
        /// Set sceneLoaded to true, so tests can use WaitUntil and OneTimeSetup can be used to load scene
        /// even though its not an IEnumerator.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="loadingMode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadingMode) {
            sceneIsLoaded = true;
        }

        /// <summary>
        /// Non-Input System related setup. Depends on the scene already being loaded
        /// </summary>
        private void SetUpSharedReferences() {
            if (referencesAreSetUp) return;
            buyableObjects = GameObject.FindGameObjectsWithTag(Tags.Item);
            buyableComponents = buyableObjects.Select(x => x.GetComponent<Buyable>()).ToArray();
            counterComponent = GameObject.FindGameObjectWithTag(Tags.Counter).GetComponent<Counter>();
            referencesAreSetUp = true;
        }

        [UnityTest]
        public IEnumerator RemoveItemFromCounter_destroys_correct_item() {
            //ARRANGE 0 - wait for scene to load in Setup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ARRANGE 1 - buy each buyable item once and add it to tracking list
            var boughtItems = new List<GameObject>(counterComponent.MaxBuyableItems);
            for (var i = 0; i < buyableObjects.Length; i++) {
                boughtItems.Add(counterComponent.PlaceOnCounter(buyableObjects[i]));
            }

            //ACT - call remove object with their IDs and let 1 frame pass
            for (var i = 0; i < boughtItems.Count; i++) {
                var curBuyable = boughtItems[i].GetComponent<Buyable>();
                counterComponent.RemoveItemFromCounter(curBuyable);
                yield return null;
                //ASSERT
                var removedItem = boughtItems[i];
                Debug.Log(removedItem.ToString());
                Assert.IsTrue(boughtItems[i] == null);
            }
        }

        // holds test cases for following UnityTests
        static IEnumerable CaseSource {
            get { yield return new TestCaseData(0).Returns(null);
                    yield return new TestCaseData(1).Returns(null);
                    yield return new TestCaseData(2).Returns(null);
                    yield return new TestCaseData(3).Returns(null);
                    yield return new TestCaseData(4).Returns(null);
            }
        }

        /// <summary>
        /// Place all items and remove a single one
        /// </summary>
        /// <param name="slotIndexToRemoveFrom"> The slotIndex of the item to remove. 0 to 4 as there are 5 slots.</param>
        [UnityTest, TestCaseSource(nameof(CaseSource))]
        public IEnumerator RemoveItemFromCounter_with_full_counter_doesnt_change_other_items_on_counter(
                                int slotIndexToRemoveFrom) {
            //ARRANGE 0 - wait for scene to load in Setup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ARRANGE 1 - place maxbuyable items, save positions, prices and names
            //  (except of item to remove) and prepare cleanup
            var maxBuyableItems = counterComponent.MaxBuyableItems;
            var notRemovedItems = new List<GameObject>(maxBuyableItems);
            var initialStates =
                new List<(string Name, float Price, Vector3 Pos)>(maxBuyableItems);
            Buyable buyableToRemove = null;
            for (var i = 0; i < maxBuyableItems; i++) {
                var placedItem = counterComponent.PlaceOnCounter(buyableObjects[i]);
                // dont add item that will be removed
                if (i == slotIndexToRemoveFrom) {
                    buyableToRemove = counterComponent.BoughtItems[i].buyable;
                    continue;
                }
                // save to check for changes later
                var curPrice = counterComponent.BoughtItems[i].buyable.Price;
                var curName = counterComponent.BoughtItems[i].buyable.ItemName;
                initialStates.Add((Name: curName,
                    Price: curPrice,
                    Pos: placedItem.transform.position));
                // add to remove list
                notRemovedItems.Add(placedItem);
            }

            //ACT - remove item at slotIndexToRemoveFrom and skip frame so Destroy call gets completed
            counterComponent.RemoveItemFromCounter(buyableToRemove);
            for (int i = 0; i < 5; i++) {
                yield return null;
            }

            //ASSERT - positions, names, prices are the same
            for (var i = 0; i < notRemovedItems.Count; i++) {
                var item = notRemovedItems[i];
                Assert.AreEqual(initialStates[i].Pos, item.transform.position);
                var buyableObjectComponent = item.GetComponent<Buyable>();
                Assert.AreEqual(initialStates[i].Name, buyableObjectComponent.ItemName);
                Assert.AreEqual(initialStates[i].Price, buyableObjectComponent.Price);
            }
        }

        [UnityTest, TestCaseSource(nameof(CaseSource))]
        public IEnumerator PlaceOnCounter_full_counter_after_item_was_removed_uses_freed_slot(int slotIndex) {
            //ARRANGE 0 - wait for scene to load in Setup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ARRANGE 1 - buy each buyable item once and add it to tracking list
            var boughtItems = new List<GameObject>(counterComponent.MaxBuyableItems);
            for (var i = 0; i < buyableObjects.Length; i++) {
                boughtItems.Add(counterComponent.PlaceOnCounter(buyableObjects[i]));
            }

            //ACT - call remove object for item in slot with slotIndex, let 1 frame pass and rebuy
            var curBuyable = boughtItems[slotIndex].GetComponent<Buyable>();
            counterComponent.RemoveItemFromCounter(curBuyable);
            yield return null;
            var placedItem = counterComponent.PlaceOnCounter(buyableObjects[0]);
            
            //ASSERT - make sure item was placed in freed slot
            Assert.AreSame(placedItem, counterComponent.ItemSlots[slotIndex].ObjectInSlot);
        }
        
        static IEnumerable MultipleRemovedCaseSource {
            get {
                var slotIndexes = new List<int>() { 0, 1, 2, 3 };
                yield return new TestCaseData(5, slotIndexes, 4).Returns(null);
                slotIndexes = new List<int>() { 0, 4, 1, 3 };
                yield return new TestCaseData(5, slotIndexes, 4).Returns(null); 
                slotIndexes = new List<int>() { 0, 2, 1 };
                yield return new TestCaseData(4, slotIndexes, 4).Returns(null);
                slotIndexes = new List<int>() { 4, 0 };
                yield return new TestCaseData(5, slotIndexes, 2).Returns(null);
            }
        }
        
        [UnityTest,TestCaseSource(nameof(MultipleRemovedCaseSource))]
        public IEnumerator PlaceOnCounter_on_multiple_removed_items_uses_lowest_indexed_ItemSlot(
                    int numItemsOnFirstBuy, List<int> SlotsToRemoveItemsFrom, int numItemsOnScndBuy) {
            //ARRANGE 0 - wait for scene to load in Setup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ARRANGE 1 - buy numItemsToBuyFirst items
            var boughtItems = new List<GameObject>(counterComponent.MaxBuyableItems);
            for (var i = 0; i < numItemsOnFirstBuy; i++) {
                boughtItems.Add(counterComponent.PlaceOnCounter(
                                                        buyableObjects[i % buyableObjects.Length]));
            }

            //ACT 1 - Remove Items from indexes in SlotsToRemoveItemsFrom
            foreach (var slotIndex in SlotsToRemoveItemsFrom) {
                var curBuyable = boughtItems[slotIndex].GetComponent<Buyable>();
                counterComponent.RemoveItemFromCounter(curBuyable); 
                yield return null;
            }

            //ACT 2 - Place numItemsOnScndBuy items
            GameObject placedItem = null;
            for (int i = 0; i < numItemsOnScndBuy; i++) {
                //Determine the slot that should be used (slot with lowest sorting name in scene has
                // highest priority)
                ItemSlot targetSlot = null;
                foreach (var slot in counterComponent.ItemSlots) {
                    if (slot.SlotUsed) continue;
                    targetSlot = slot;
                    break;
                }
                // Place item
                placedItem = counterComponent.PlaceOnCounter(buyableObjects[i % buyableObjects.Length]);
                
                //ASSERT - lowest indexed ItemSlot should be used
                Assert.IsNotNull(placedItem);
                Assert.IsNotNull(targetSlot);
                Assert.AreSame(targetSlot.ObjectInSlot, placedItem.gameObject);
            }
        }
    }
}
