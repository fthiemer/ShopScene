using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using NSubstitute;
using Object = UnityEngine.Object;


namespace Tests.EditMode {
    public class buyable_object {
        private GameObject[] buyableItems;
        private Collider[] colliders;
        private BuyableObject[] buyableObjectComponents;

        /// <summary>
        /// Load ShopScene and put a substitute into all Counter fields, so all calls to its methods
        /// can be monitored.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetup() {
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
            colliders = buyableItems.Select(x => x.GetComponent<Collider>()).ToArray();
            buyableObjectComponents = buyableItems.Select(x => x.GetComponent<BuyableObject>()).ToArray();
            // Replace Counter component of every item with substitute, so that it can be tested for received calls
            for (var i = 0; i < buyableObjectComponents.Length; i++) {
                var buyableObjectComponent = buyableObjectComponents[i];
                //Call Awake, as it would not be called in edit mode
                test_helper.InvokePrivateMethod(buyableObjectComponent, "Awake");
                var counterSubstitute = Substitute.For<ICounter>();
                buyableObjectComponent.Counter = counterSubstitute;
                // Give PlaceOnCounter() return behaviour, as if it always places the item
                buyableObjectComponent.Counter.PlaceOnCounter(buyableItems[i])
                    .Returns(buyableItems[i]);
            }
        }

        /// <summary>
        /// Make sure, all items are clickable in current scene.
        /// Do so by simulating mouse click on their center
        /// </summary>
        [UnityTest]
        public IEnumerator mouse_click_on_items_calls_ICounter_PlaceOnCounter() {
            //ARRANGE - somehow get usable mouse prepared
            //ACT - click on screen pos, that correlates to world pos
            //var mouse = InputSystem.AddDevice<Mouse
            yield return null;
            //ASSERT - take from below
        }
        
        
        [Test]
        public void OnMouseDown_triggers_ICounter_PlaceOnCounter() {
            //ARRANGE - see OneTimeSetup()
            
            for (int i=0; i<buyableItems.Length; i++) {
                //ACT
                // call private OnMouseDown function of current BuyableObject component
                test_helper.InvokePrivateMethod(buyableObjectComponents[i], "OnMouseDown");

                //ASSERT
                //Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
                buyableObjectComponents[i].Counter.Received(1).PlaceOnCounter(buyableItems[i]);
                //CLEANUP
                buyableObjectComponents[i].Counter.ClearReceivedCalls();
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
                //ACT - call Buy
                curComponent.Buy();
                //ASSERT
                Assert.IsTrue(curComponent.IsAlreadyBought);
                //CLEANUP
                test_helper.SetPrivateBoolField(curComponent, "isAlreadyBought", false);
                Assert.IsFalse(curComponent.IsAlreadyBought);
            }
        }

        [Test]
        public void Buy_prevents_PlaceOnCounter_call_on_following_OnMouseDown_call() {
            //ARRANGE - happens in OneTimeSetup()
            for (int i = 0; i < buyableItems.Length; i++) {
                var curComponent = buyableObjectComponents[i];
                //ACT 1 - call Buy
                curComponent.Buy();

                //ACT 2 - call private OnMouseDown of curComponent
                test_helper.InvokePrivateMethod(curComponent, "OnMouseDown");

                //ASSERT
                //Use Received method of Substitute to confirm ZERO calls to PlaceOnCounter
                curComponent.Counter.Received(0).PlaceOnCounter(buyableItems[i]);
                //CLEANUP
                curComponent.Counter.ClearReceivedCalls();
                test_helper.SetPrivateBoolField(curComponent, "isAlreadyBought", false);
                Assert.IsFalse(curComponent.IsAlreadyBought);
            }
        }
    }
}
