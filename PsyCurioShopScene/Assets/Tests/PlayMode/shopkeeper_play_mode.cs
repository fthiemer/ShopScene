using System;
using System.Collections;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode {
    public class shopkeeper_play_mode : InputTestFixture {
        private Camera camera;
        private bool sceneIsLoaded;
        private GameObject shopkeeperObject;
        private Shopkeeper shopkeeperComponent;
        private Animator shopkeeperAnimator;
        private Func<bool> animationFinished;
        private bool referencesAreSetUp;
        private int usedLayerIndex;
        private Vector3 shopkeeperClickPos;
        private Mouse mouse;

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
        public void SetUp() {
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
        /// Non-Input System related setup. Depends on the scene already being loaded
        /// </summary>
        private void SetUpSharedReferences() {
            if (referencesAreSetUp) return;
            // Get camera object
            camera = GameObject.FindWithTag(Tags.MainCamera).GetComponent<Camera>();
            shopkeeperObject = GameObject.FindWithTag(Tags.Shopkeeper);
            shopkeeperComponent = shopkeeperObject.GetComponent<Shopkeeper>();
            // Set position to click at the upper body of the Shopkeeper
            var shopKeeperYPos = 1.5f * shopkeeperObject.GetComponent<Collider>().bounds.extents.y;
            var tmpPos = shopkeeperObject.transform.position;
            shopkeeperClickPos = new Vector3(tmpPos.x, shopKeeperYPos, tmpPos.z);
            shopkeeperAnimator = shopkeeperObject.GetComponent<Animator>();
            usedLayerIndex = shopkeeperAnimator.GetLayerIndex("Base Layer");
            animationFinished =
                new Func<bool>(() => shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).normalizedTime > 1);
            referencesAreSetUp = true;
        }


        [UnityTest]
        public IEnumerator Shopkeeper_Animator_starts_with_idle_animation() {
            //ARRANGE - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            //ACT
            //ASSERT
            Assert.IsTrue(shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).IsName("Idle"));
            //CLEANUP - in TearDown
        }
        
        
        [UnityTest]
        public IEnumerator Single_Click_on_Shopkeeper_triggers_Wave_Animation_transitioning_to_idle_on_end() {
            //ARRANGE 1 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();
            Assert.IsFalse(shopkeeperComponent.IsWaving);
            Assert.IsTrue(shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).IsName("Idle"));
            
            //ACT - Click on Shopkeeper
            Vector2 screenPos = camera.WorldToScreenPoint(shopkeeperClickPos);
            yield return ClickAt(screenPos);

            //ASSERT 1 - Waving Animation is played after transition time
            yield return new WaitForSeconds(shopkeeperComponent.ToWaveTransitionDuration + 0.05f);
            Assert.IsTrue(shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).IsName("Waving"));
            
            //ASSERT 2 - Idle Animation is played after transition time
            //  wait for Waving Animation to finish
            yield return new WaitUntil(animationFinished);
            yield return new WaitForSeconds(shopkeeperComponent.ToIdleTransitionDuration + 0.05f);
            Assert.IsTrue(shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).IsName("Idle"));
            
            //CLEANUP - in TearDown
        }
        
        [UnityTest]
        public IEnumerator repeated_clicks_on_waving_shopkeeper_dont_restart_Wave_Animation() {
            //ARRANGE 1 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpSharedReferences();

            //ACT 1 - Click on Shopkeeper and start Waving animation
            Vector2 screenPos = camera.WorldToScreenPoint(shopkeeperClickPos);
            Set(mouse.position, screenPos, queueEventOnly: false);
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);
            yield return new WaitForSeconds(shopkeeperComponent.ToWaveTransitionDuration + 0.05f);
            Assert.IsTrue(shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).IsName("Waving"));
            
            //ACT 2 - Repeatedly click Shopkeeper during Waving animation until transition happens
            // as long as animator does not transition to next animation and current animation is not finished
            while (!shopkeeperAnimator.IsInTransition(usedLayerIndex) && 
                   shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).normalizedTime % 1f < 0.95f) {
                yield return ClickAt(screenPos);
            }
            
            // ASSERT - Make sure transition is to idle not waving again
            yield return new WaitUntil(animationFinished);
            yield return new WaitForSeconds(shopkeeperComponent.ToIdleTransitionDuration + 0.05f);
            Assert.IsTrue(shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).IsName("Idle"));
            
            //CLEANUP - in TearDown
        }

        /// <summary>
        /// Simulate click at target screen position
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        private IEnumerator ClickAt(Vector2 screenPos) {
            Set(mouse.position, screenPos, queueEventOnly: false);
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);
            yield return null;
        }
    }
}