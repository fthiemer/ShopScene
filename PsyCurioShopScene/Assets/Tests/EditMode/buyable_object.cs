using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using NSubstitute;
using UnityEngine.TestTools;


namespace Tests.EditMode {
    public class buyable_object {
        private GameObject[] buyableItems;
        private BuyableObject[] buyableObjectComponents;

        /// <summary>
        /// Load ShopScene and put a substitute into all Counter fields, so all calls to its methods
        /// can be monitored.
        /// </summary>
        [SetUp]
        public void Setup() {
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
            buyableObjectComponents = buyableItems.Select(x => x.GetComponent<BuyableObject>()).ToArray();
        }

        [Test]
        public void OnPointerClick_of_unbought_item_triggers_ICounter_PlaceOnCounter() {
            //ARRANGE
            SetUpCounterSubstitutes();
            for (int i=0; i<buyableItems.Length; i++) {
                //ACT - Call OnPointerClick
                var curComponent = buyableObjectComponents[i];
                curComponent.OnPointerClick(null);

                //ASSERT
                // Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
                curComponent.Counter.Received(1).PlaceOnCounter(buyableItems[i]);
                //CLEANUP - not necessary as scene is loaded again in setup
            }
        }
        
        [Test]
        public void Awake_sets_yOffset_to_half_y_bounds() {
            //ARRANGE - Happened in OneTimeSetup()
            for (var i = 0; i < buyableObjectComponents.Length; i++) {
                //ACT - not neccessary, Awake called in OneTimeSetup
                //ASSERT
                float actualYOffset = buyableObjectComponents[i].YOffset;
                float expectedYOffset = buyableItems[i].GetComponent<MeshRenderer>().bounds.extents.y;
                Assert.AreEqual(expectedYOffset, actualYOffset);
            }
        }

        [Test]
        public void MarkAsBought_sets_isBought_to_true() {
            //ARRANGE - happens in OneTimeSetup()
            for (int i = 0; i < buyableItems.Length; i++) {
                var curComponent = buyableObjectComponents[i];
                //ACT - call MarkAsBought
                curComponent.MarkAsBought();
                //ASSERT
                Assert.IsTrue(curComponent.IsBought);
                //CLEANUP
                ReflectionHelper.SetPrivateFieldOfType(curComponent, "isBought", false);
                Assert.IsFalse(curComponent.IsBought);
            }
        }

        [Test]
        public void Buy_prevents_PlaceOnCounter_call_on_following_OnPointerClick_call() {
            //ARRANGE
            SetUpCounterSubstitutes();
            for (int i = 0; i < buyableItems.Length; i++) {
                var curComponent = buyableObjectComponents[i];
                
                //ACT 1 - call MarkAsBought
                curComponent.MarkAsBought();

                //ACT 2 - call private OnPointerClick of curComponent
                curComponent.OnPointerClick(null);
                
                //ASSERT
                //Use Received method of Substitute to confirm ZERO calls to PlaceOnCounter
                curComponent.Counter.Received(0).PlaceOnCounter(buyableItems[i]);
                //CLEANUP
                curComponent.Counter.ClearReceivedCalls();
                ReflectionHelper.SetPrivateFieldOfType(curComponent, "isBought", false);
                Assert.IsFalse(curComponent.IsBought);
            }
        }

        [UnityTest]
        public IEnumerator RemoveFromCounter_destroys_correct_item() {
            //ARRANGE - buy each buyable item once and add it to tracking list
            ICounter counterComponent = buyableObjectComponents[0].Counter;
            var boughtItems = new List<GameObject>(counterComponent.MaxBuyableItems);
            for (var i = 0; i < buyableItems.Length; i++) {
                boughtItems.Add(counterComponent.PlaceOnCounter(buyableItems[i]));
            }
            
            //ACT - call remove object with their IDs and let 1 frame pass
            for (var i = 0; i < boughtItems.Count; i++) {
                int iD = boughtItems[i].GetComponent<BuyableObject>().UniqueID;
                counterComponent.RemoveItemFromCounter(iD);
                yield return null;
                //ASSERT
                Assert.IsNull(boughtItems[i]);
            }
        }
        

        private void SetUpCounterSubstitutes() {
            // Replace Counter component of every item with substitute, so that it can be tested for received calls
            for (var i = 0; i < buyableObjectComponents.Length; i++) {
                var buyableObjectComponent = buyableObjectComponents[i];
                //Call Awake, as it would not be called in edit mode
                ReflectionHelper.InvokePrivateVoidMethod(buyableObjectComponent, "Awake");
                var counterSubstitute = Substitute.For<ICounter>();
                buyableObjectComponent.Counter = counterSubstitute;
                // Give PlaceOnCounter() return behaviour, as if it always places the item
                buyableObjectComponent.Counter.PlaceOnCounter(buyableItems[i])
                    .Returns(buyableItems[i]);
            }
        }
    }
}
