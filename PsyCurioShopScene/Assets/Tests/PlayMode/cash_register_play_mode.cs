using System.Collections;
using NSubstitute;
using NUnit.Framework;
using Tests.EditMode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode {
    public class cash_register_play_mode : InputTestFixture {
        private Camera camera;
        private GameObject cashRegisterObject;
        private CashRegister cashRegisterComponent;
        private IShopkeeper shopkeeper;
        private Mouse mouse;
        
        private bool sceneIsLoaded;
        private bool referencesAreSetUp;

        /// <summary>
        /// Set sceneLoaded to true, so tests can use WaitUntil and SetUp can be used to load scene
        /// even though its not an IEnumerator.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="loadingMode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadingMode) {
            sceneIsLoaded = true;
        }

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
        /// Non-Input System related setup. Only use after Scene was loaded (guarantee with WaitUntil)
        /// </summary>
        private void SetUpSharedReferences() {
            if (referencesAreSetUp) return;
            //get camera object
            camera = GameObject.FindWithTag(Tags.MainCamera).GetComponent<Camera>();
            cashRegisterObject = GameObject.FindWithTag(Tags.CashRegister);
            cashRegisterComponent = cashRegisterObject.GetComponent<CashRegister>();
            var shopkeeperSubstitute = Substitute.For<IShopkeeper>();
            cashRegisterComponent.shopkeeper = shopkeeperSubstitute;
            referencesAreSetUp = true;
        }

        /// <summary>
        /// Click on CashRegister triggers Shopkeeper.Say once with message formulated by ConstructBillMessage
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ShopScene_click_on_CashRegister_calls_ShopkeeperSay_with_text_for_speechbubble() {
            //ARRANGE 1 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            // Get bill message corresponding to current scene state (no items bought)
            string billMessage = ReflectionHelper.InvokePrivateNonVoidMethod<string>(cashRegisterComponent, 
                "ConstructBillMessage") as string;

            //ACT - Click on Cash Register
            Vector2 screenPos = camera.WorldToScreenPoint(cashRegisterObject.transform.position);
            Set(mouse.position, screenPos, queueEventOnly: false);
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);
            yield return null;

            //ASSERT
            cashRegisterComponent.shopkeeper.Received(1).Say(billMessage);
            
            //CLEANUP
            cashRegisterComponent.shopkeeper.ClearReceivedCalls();
        }
        
        [UnityTest]
        public IEnumerator Counter_RemoveItemFromCounter_if_CashRegister_clicked_triggers_speechbubble_update() {
            //ARRANGE 1 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ARRANGE 2 - Set up substitute, place item and click cash register to set into bill update mode
            var shopkeeperSubstitute = Substitute.For<IShopkeeper>();
            cashRegisterComponent.shopkeeper = shopkeeperSubstitute;
            var itemToBuy = GameObject.FindWithTag(Tags.Item);
            var boughtItem = cashRegisterComponent.counter.PlaceOnCounter(itemToBuy);
            cashRegisterComponent.OnPointerClick(null);
            //ACT 
            cashRegisterComponent.counter.RemoveItemFromCounter(boughtItem.GetComponent<Buyable>());
            yield return null;
            //ASSERT
            // Get bill message corresponding to current scene state (no items bought)
            string billMessage = ReflectionHelper.InvokePrivateNonVoidMethod<string>(cashRegisterComponent, 
                "ConstructBillMessage") as string;
            cashRegisterComponent.shopkeeper.Received(1).Say(billMessage);
            //CLEANUP - Setup Reinitializes Scene
            cashRegisterComponent.shopkeeper.ClearReceivedCalls();
        }
        
        [UnityTest]
        public IEnumerator Counter_RemoveItemFromCounter_if_CashRegister_not_clicked_doesnt_trigger_speechbubble_update() {
            //ARRANGE 1 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ARRANGE 2 - Set up substitute, place item and click cash register to set into bill update mode
            var shopkeeperSubstitute = Substitute.For<IShopkeeper>();
            cashRegisterComponent.shopkeeper = shopkeeperSubstitute;
            var itemToBuy = GameObject.FindWithTag(Tags.Item);
            var boughtItem = cashRegisterComponent.counter.PlaceOnCounter(itemToBuy);
            //ACT 
            cashRegisterComponent.counter.RemoveItemFromCounter(boughtItem.GetComponent<Buyable>());
            yield return null;
            //ASSERT
            // Get bill message corresponding to current scene state (no items bought)
            string billMessage = ReflectionHelper.InvokePrivateNonVoidMethod<string>(cashRegisterComponent, 
                "ConstructBillMessage") as string;
            cashRegisterComponent.shopkeeper.Received(0).Say(billMessage);
            //CLEANUP - Setup Reinitializes Scene
            cashRegisterComponent.shopkeeper.ClearReceivedCalls();
        }
    }
}
