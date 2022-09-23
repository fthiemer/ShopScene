using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NSubstitute;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class counter {
    //private int numChildrenToAdd;
    private GameObject counterObject;
    private Counter counterComponent;
    
    [OneTimeSetUp]
    public void SetUp() {
        EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
        counterObject = GameObject.FindWithTag(Tags.Counter);
        counterComponent = counterObject.GetComponent<Counter>();
    }
    [Test]
    public void max_buyable_items_equals_number_of_child_objects_after_awake() {
        //ARRANGE -> happens in SetUp()
        //ACT -> happens in SetUp(), when Awake of the Counter component is called (thus Counter is [ExecuteAlways])
        //ASSERT
        Assert.AreEqual(counterObject.transform.childCount, counterComponent.MaxBuyableItems);
    }    
    
    [Test]
    public void max_buyable_items_equals_5_after_awake() {
        //ARRANGE -> happens in SetUp()
        //ACT -> happens in SetUp(), when Awake of the Counter component is called (thus Counter is [ExecuteAlways])
        //ASSERT
        Assert.AreEqual(5, counterComponent.MaxBuyableItems);
    }
    
    

    [Test]
    public void PlaceOnCounter_places_bought_item_with_correct_y_offset() {
    }

    [OneTimeTearDown]
    public void TearDown() {
        EditorSceneManager.CloseScene(SceneManager.GetSceneByPath("Assets/Scenes/ShopScene.unity"), 
            false);
    }
}
