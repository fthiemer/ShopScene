using System;
using System.Collections;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode {
    public class shopkeeper : InputTestFixture {
        private Camera camera;
        private bool sceneIsLoaded;
        private GameObject shopkeeperObject;
        private Shopkeeper shopkeeperComponent;
        private Animator shopkeeperAnimator;
        private Func<bool> animationFinished;
        private bool referencesAreSetUp;
        private int usedLayerIndex;
        private Vector3 shopkeeperClickPos;

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
            //get camera object
            camera = GameObject.FindWithTag(Tags.MainCamera).GetComponent<Camera>();
            shopkeeperObject = GameObject.FindWithTag(Tags.Shopkeeper);
            shopkeeperComponent = shopkeeperObject.GetComponent<Shopkeeper>();
            // Set position to click at the upper body of the shopkeeper
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
        public IEnumerator Shopkeeper_starts_with_idle_animation() {
            //ARRANGE 1 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpCommonReferences();
            Assert.IsTrue(shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex).IsName("Idle"));
        }
        
        
        [UnityTest]
        public IEnumerator Single_Click_on_Shopkeeper_triggers_Wave_Animation_transitioning_to_idle_on_end() {
            //ARRANGE 1 - wait for scene to load in OneTimeSetup, then set up references if not done yet
            yield return new WaitUntil(() => sceneIsLoaded);
            SetUpCommonReferences();
            //ARRANGE 2 - Prepare mouse
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            
            //ACT - Click on Shopkeeper
            Vector2 screenPos = camera.WorldToScreenPoint(shopkeeperClickPos);
            Set(mouse.position, screenPos, queueEventOnly: false);
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);
            yield return null;

            //ASSERT 1 - Waving Animation is played after transition time
            yield return new WaitForSeconds(shopkeeperComponent.ToWaveTransitionDuration);
            yield return new WaitUntil(() => !shopkeeperAnimator.IsInTransition(usedLayerIndex));
            var curAnimatorState = shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex);
            Assert.IsTrue(curAnimatorState.IsName("Waving"));
            
            //ASSERT 2 - Idle Animation is played after transition time
            //  wait for Waving Animation to finish
            yield return new WaitUntil(animationFinished);
            yield return new WaitForSeconds(shopkeeperComponent.ToIdleTransitionDuration);
            curAnimatorState = shopkeeperAnimator.GetCurrentAnimatorStateInfo(usedLayerIndex);
            Assert.IsTrue(curAnimatorState.IsName("Idle"));
        }
    }
}