using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using NSubstitute;


namespace Tests.EditMode {
    public class buyable_object {
        private GameObject[] buyableItems;
        private BuyableObject[] buyableObjectComponents;

        /// <summary>
        /// Load ShopScene and put a substitute into all Counter fields, so all calls to its methods
        /// can be monitored.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetup() {
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
            buyableObjectComponents = buyableItems.Select(x => x.GetComponent<BuyableObject>()).ToArray();
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


        [Test]
        public void OnPointerClick_triggers_ICounter_PlaceOnCounter() {
            //ARRANGE - see OneTimeSetup()
            for (int i=0; i<buyableItems.Length; i++) {
                //ACT - Call OnPointerClick
                var curComponent = buyableObjectComponents[i];
                curComponent.OnPointerClick(null);

                //ASSERT
                //Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
                curComponent.Counter.Received(1).PlaceOnCounter(buyableItems[i]);
                //CLEANUP
                curComponent.Counter.ClearReceivedCalls();
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
        public void Buy_sets_isAlreadyBought_to_true() {
            //ARRANGE - happens in OneTimeSetup()
            for (int i = 0; i < buyableItems.Length; i++) {
                var curComponent = buyableObjectComponents[i];
                //ACT - call MarkAsBought
                curComponent.MarkAsBought();
                //ASSERT
                Assert.IsTrue(curComponent.IsAlreadyBought);
                //CLEANUP
                ReflectionHelper.SetPrivateFieldOfType(curComponent, "isAlreadyBought", false);
                Assert.IsFalse(curComponent.IsAlreadyBought);
            }
        }

        [Test]
        public void Buy_prevents_PlaceOnCounter_call_on_following_OnPointerClick_call() {
            //ARRANGE - happens in OneTimeSetup()
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
                ReflectionHelper.SetPrivateFieldOfType(curComponent, "isAlreadyBought", false);
                Assert.IsFalse(curComponent.IsAlreadyBought);
            }
        }
    }
}
