using System.Collections;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.EditMode {
    public class shopkeeper {
        private GameObject shopkeeperObject;
        private Shopkeeper shopkeeperComponent;
        private Animator shopkeeperAnimator;
        private TMP_Text speechbubbleTMP;
        private int usedLayerIndex;
    
        [OneTimeSetUp]
        public void OneTimeSetUp() {
            //Load Scene
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            //Set Up References
            shopkeeperObject = GameObject.FindWithTag(Tags.Shopkeeper);
            shopkeeperComponent = shopkeeperObject.GetComponent<Shopkeeper>();
            ReflectionHelper.InvokePrivateVoidMethod(shopkeeperComponent, "Awake");
            ReflectionHelper.InvokePrivateVoidMethod(shopkeeperComponent, "Start");
            shopkeeperAnimator = shopkeeperObject.GetComponent<Animator>();
            usedLayerIndex = shopkeeperAnimator.GetLayerIndex("Base Layer");
            var speechbubbleTMPObject = GameObject.FindWithTag(Tags.SpeechbubbleTMP);
            speechbubbleTMP = speechbubbleTMPObject.GetComponent<TMP_Text>();
        }


        [Test]
        public void Wave_sets_Shopkeeper_isWaving_to_true() {
            //ARRANGE - happened in OneTimeSetUp
            //ACT
            ReflectionHelper.InvokePrivateVoidMethod(shopkeeperComponent, "Wave");

            //ASSERT
            Assert.IsTrue(shopkeeperComponent.IsWaving);
            
            //CLEANUP - see CustomTearDown
            ReflectionHelper.SetPrivateFieldOfType(shopkeeperComponent, "isWaving", false);}

        [Test]
        public void Say_sets_speechbubbleTMP_text() {
            //ARRANGE
            string speechbubbleText = "You selected:\n1 Big M&M for 1!!\n1 Cylinder for 8.88!!" +
                                      "\n1 Capsule for 3!!\n1 Sphere for 5!!\n1 Cube for 14!!" +
                                      "\n\nFor only 31.88 Robodollars you can take everything with you.";
            //ACT
            shopkeeperComponent.Say(speechbubbleText);
            
            //ASSERT
            Assert.AreEqual(speechbubbleText, speechbubbleTMP.text);
        }
    }
}
