using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode {
    public class buyable_object_play_mode : InputTestFixture {
        private GameObject[] buyableItems;
        private BuyableObject[] buyableObjectComponents;
        private Camera camera;
        private Mouse mouse;
        private bool sceneIsLoaded;
        private bool referencesAreSetUp;

        [SetUp]
        public override void Setup() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Assets/Scenes/ShopScene.unity", LoadSceneMode.Single);
            base.Setup();
            mouse = InputSystem.AddDevice<Mouse>();
        }

        [TearDown]
        public override void TearDown() {
            base.TearDown();
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
            buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
            buyableObjectComponents = buyableItems.Select(x => x.GetComponent<BuyableObject>()).ToArray();
            // Get camera object
            camera = GameObject.FindWithTag(Tags.MainCamera).GetComponent<Camera>();
            referencesAreSetUp = true;
        }
        

        /// <summary>
        /// Make sure, all items are clickable in current scene. Do so by simulating mouse click on their center.
        /// </summary>
        [UnityTest]
        public IEnumerator ShopScene_mouse_click_on_items_calls_ICounterPlaceOnCounter() {
            //ARRANGE 2 - wait for scene to load in Setup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            // SetUp Substitute for the counter
            for (var i = 0; i < buyableObjectComponents.Length; i++) {
                var buyableObjectComponent = buyableObjectComponents[i];
                var counterSubstitute = Substitute.For<ICounter>();
                buyableObjectComponent.Counter = counterSubstitute;
                buyableObjectComponent.Counter.PlaceOnCounter(buyableItems[i])
                    .Returns(buyableItems[i]);
            }

            //ARRANGE 3 - Prepare usable mouse -> easy with Input System \o/
            mouse = InputSystem.AddDevice<Mouse>();

            //ACT - click on screen pos, that correlates to items world pos
            for (int i = 0; i < buyableItems.Length; i++) {
                var worldPos = buyableItems[i].transform.position;
                Vector2 screenPos = camera.WorldToScreenPoint(worldPos);
                Set(mouse.position, screenPos, queueEventOnly: false);
                Press(mouse.leftButton);
                yield return null;
                Release(mouse.leftButton);
                yield return null;

                //ASSERT
                // Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
                buyableObjectComponents[i].Counter.Received(1).PlaceOnCounter(buyableItems[i]);

                //CLEANUP
                buyableObjectComponents[i].Counter.ClearReceivedCalls();
            }
        }

        
        /// <summary>
        /// Make sure, all bought items are clickable in current scene and trigger
        /// Counter.RemoveItemFromCounter on click.
        /// </summary>
        [UnityTest]
        public IEnumerator ShopScene_click_on_bought_items_calls_RemoveItemFromCounter_with_items_ID() {
            //ARRANGE 2 - wait for scene to load in Setup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ARRANGE 3 - Prepare usable mouse -> easy with Input System \o/
            mouse = InputSystem.AddDevice<Mouse>();
            //ARRANGE 4 - Buy all items once
            var boughtItems = new List<GameObject>();
            var boughtItemComponents = new List<BuyableObject>();
            foreach (var item in buyableItems) {
                var counter = item.GetComponent<Counter>();
                var placedItem = counter.PlaceOnCounter(item);
                //Add only successfully placed items (for the case, there are
                // more items to buy than slots on the counter)
                if (placedItem != null) {
                    boughtItems.Add(placedItem);
                }
            }
            foreach (var item in boughtItems) {
                //ARRANGE 5 - Place Substitute
                var buyableObjectComponent = item.GetComponent<BuyableObject>();
                buyableObjectComponent.Counter = Substitute.For<ICounter>();
                
                //ACT - click on screen pos, that correlates to items world pos
                var worldPos = item.transform.position;
                Vector2 screenPos = camera.WorldToScreenPoint(worldPos);
                Set(mouse.position, screenPos, queueEventOnly: false);
                Press(mouse.leftButton);
                yield return null;
                Release(mouse.leftButton);
                yield return null;

                //ASSERT
                // Confirm exactly one call to RemoveItemFromCounter with correct ID
                buyableObjectComponent.Counter.Received(1).RemoveItemFromCounter(buyableObjectComponent.UniqueID);

                //CLEANUP - done in TearDown and next Setup which reloads scene from scratch
            }
        }
    }
}