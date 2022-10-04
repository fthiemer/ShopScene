using System.Collections;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.EditMode {
    public class shopkeeper_edit_mode {
        private GameObject shopkeeperObject;
        private Shopkeeper shopkeeperComponent;
        private Animator shopkeeperAnimator;
        private int usedLayerIndex;
    
        [SetUp]
        public void SetUp() {
            // ARRANGE 1 - Load Scene
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            shopkeeperObject = GameObject.FindWithTag(Tags.Shopkeeper);
            shopkeeperComponent = shopkeeperObject.GetComponent<Shopkeeper>();
            reflection_helper.InvokePrivateMethod(shopkeeperComponent, "Awake");
            shopkeeperAnimator = shopkeeperObject.GetComponent<Animator>();
            usedLayerIndex = shopkeeperAnimator.GetLayerIndex("Base Layer");
        }

        [TearDown]
        public void CustomTearDown() {
            reflection_helper.SetPrivateBoolField(shopkeeperComponent, "isWaving", false);
        }
        
        
        [Test]
        public void Wave_sets_Shopkeeper_isWaving_to_true() {
            //ARRANGE - happened in SetUp
            //ACT
            reflection_helper.InvokePrivateMethod(shopkeeperComponent, "Wave");

            //ASSERT
            Assert.IsTrue(shopkeeperComponent.IsWaving);
            
            //CLEANUP - see CustomTearDown
        }
    }
}
