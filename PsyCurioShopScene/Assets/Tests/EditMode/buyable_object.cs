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
        /// Load ShopScene and put a substitute into all ICounter fields, so all calls to its methods
        /// can be monitored.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetup() {
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
            colliders = buyableItems.Select(x => x.GetComponent<Collider>()).ToArray();
            buyableObjectComponents = buyableItems.Select(x => x.GetComponent<BuyableObject>()).ToArray();
            // Replace ICounter component of every item with substitute, so that it can be tested for received calls
            for (var i = 0; i < buyableObjectComponents.Length; i++) {
                var buyableObjectComponent = buyableObjectComponents[i];
                var counterSubstitute = Substitute.For<ICounter>();
                buyableObjectComponent.ICounter = counterSubstitute;
                buyableObjectComponent.ICounter.PlaceOnCounter(buyableItems[i])
                    .Returns(buyableItems[i]);
            }
        }

        [Test]
        public void PlaceBuyableObject_is_called_when_OnMouseDown_is_triggered() {
            //ARRANGE - happens in OneTimeSetup()
            
            for (int i=0; i<buyableItems.Length; i++) {
                //ACT
                    //trigger OnMouseDown event
                    // Code taken from https://stackoverflow.com/questions/672501/is-there-an-easy-way-to-use-internalsvisibletoattribute
                    // and https://stackoverflow.com/questions/249847/how-do-you-test-private-methods-with-nunit
                var onMouseDown = buyableObjectComponents[i].GetType().GetMethod("OnMouseDown", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                onMouseDown.Invoke(buyableObjectComponents[i], null);
                
                // Hm, stellt sicher, dass Implementierung sich nicht ändert. Aber ist das das Ziel eines Tests?
                
                //ASSERT
                    //Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
                buyableObjectComponents[i].ICounter.Received(1).PlaceOnCounter(buyableItems[i]);
            }
        }
    }
}
