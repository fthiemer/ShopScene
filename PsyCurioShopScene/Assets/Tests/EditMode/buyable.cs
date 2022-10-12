using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using NSubstitute;


namespace Tests.EditMode {
    public class buyable {
        private GameObject[] buyableObjects;
        private Buyable[] buyableComponents;

        /// <summary>
        /// Load ShopScene and put a substitute into all Counter fields, so all calls to its methods
        /// can be monitored.
        /// </summary>
        [SetUp]
        public void Setup() {
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            buyableObjects = GameObject.FindGameObjectsWithTag(Tags.Item);
            buyableComponents = buyableObjects.Select(x => x.GetComponent<Buyable>()).ToArray();
        }

        [Test]
        public void OnPointerClick_of_unbought_item_triggers_ICounter_PlaceOnCounter() {
            //ARRANGE
            SetUpCounterSubstitutes();
            for (int i=0; i<buyableObjects.Length; i++) {
                //ACT - Call OnPointerClick
                var curComponent = buyableComponents[i];
                curComponent.OnPointerClick(null);

                //ASSERT
                // Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
                curComponent.ResponsibleCounter.Received(1).PlaceOnCounter(buyableObjects[i]);
                //CLEANUP - not necessary as scene is loaded again in setup
            }
        }
        
        [Test]
        public void Awake_sets_yOffset_to_half_y_bounds() {
            //ARRANGE - Happened in OneTimeSetup()
            for (var i = 0; i < buyableComponents.Length; i++) {
                //ACT
                ReflectionHelper.InvokePrivateVoidMethod(buyableComponents[i],"Awake");
                //ASSERT
                float actualYOffset = buyableComponents[i].YOffset;
                float expectedYOffset = buyableObjects[i].GetComponent<MeshRenderer>().bounds.extents.y;
                Assert.AreEqual(expectedYOffset, actualYOffset);
            }
        }

        [Test]
        public void MarkAsBought_sets_isBought_to_true() {
            //ARRANGE - happens in OneTimeSetup()
            for (int i = 0; i < buyableObjects.Length; i++) {
                var curComponent = buyableComponents[i];
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
            for (int i = 0; i < buyableObjects.Length; i++) {
                var curComponent = buyableComponents[i];
                
                //ACT 1 - call MarkAsBought
                curComponent.MarkAsBought();

                //ACT 2 - call private OnPointerClick of curComponent
                curComponent.OnPointerClick(null);
                
                //ASSERT
                //Use Received method of Substitute to confirm ZERO calls to PlaceOnCounter
                curComponent.ResponsibleCounter.Received(0).PlaceOnCounter(buyableObjects[i]);
                //CLEANUP
                curComponent.ResponsibleCounter.ClearReceivedCalls();
                ReflectionHelper.SetPrivateFieldOfType(curComponent, "isBought", false);
                Assert.IsFalse(curComponent.IsBought);
            }
        }

        private void SetUpCounterSubstitutes() {
            // Replace Counter component of every item with substitute, so that it can be tested for received calls
            for (var i = 0; i < buyableComponents.Length; i++) {
                var buyableObjectComponent = buyableComponents[i];
                //Call Awake, as it would not be called in edit mode
                ReflectionHelper.InvokePrivateVoidMethod(buyableObjectComponent, "Awake");
                var counterSubstitute = Substitute.For<ICounter>();
                buyableObjectComponent.ResponsibleCounter = counterSubstitute;
                // Give PlaceOnCounter() return behaviour, as if it always places the item
                buyableObjectComponent.ResponsibleCounter.PlaceOnCounter(buyableObjects[i])
                    .Returns(buyableObjects[i]);
            }
        }
    }
}
