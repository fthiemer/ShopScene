using System.Collections;
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
        private bool sceneIsLoaded;
        private bool referencesAreSetUp;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Assets/Scenes/ShopScene.unity");
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
        private void SetUpCommonReferences() {
            if (referencesAreSetUp) return;
            buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
            buyableObjectComponents = buyableItems.Select(x => x.GetComponent<BuyableObject>()).ToArray();
            //get camera object
            camera = GameObject.FindWithTag(Tags.MainCamera).GetComponent<Camera>();
            //ARRANGE - Setup Substitute for the counter
            for (var i = 0; i < buyableObjectComponents.Length; i++) {
                var buyableObjectComponent = buyableObjectComponents[i];
                var counterSubstitute = Substitute.For<ICounter>();
                buyableObjectComponent.Counter = counterSubstitute;
                // Give PlaceOnCounter() return behaviour, as if it always places the item
                buyableObjectComponent.Counter.PlaceOnCounter(buyableItems[i])
                    .Returns(buyableItems[i]);
            }
            referencesAreSetUp = true;
        }
        

        /// <summary>
        /// Make sure, all items are clickable in current scene. Do so by simulating mouse click on their center.
        /// </summary>
        [UnityTest]
        public IEnumerator mouse_click_on_items_calls_ICounter_PlaceOnCounter() {
            //ARRANGE 2 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpCommonReferences();
            
            //ARRANGE 3 - Prepare usable mouse -> easy with Input System \o/
            Mouse mouse = InputSystem.AddDevice<Mouse>();

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
                //Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
                buyableObjectComponents[i].Counter.Received(1).PlaceOnCounter(buyableItems[i]);

                //CLEANUP
                buyableObjectComponents[i].Counter.ClearReceivedCalls();
            }
        }
    }
}