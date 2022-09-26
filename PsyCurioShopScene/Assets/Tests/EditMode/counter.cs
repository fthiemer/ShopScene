using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class counter {
    private GameObject counterObject;
    private Counter counterComponent;
    
    
    /// <summary>
    /// Open ShopScene and get Counter Object.
    /// Doing this once instead of only instantiating objects for each test as needed has the advantage of
    /// testing the scene for correct setup
    /// </summary>
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
    
    /*More than maxBuyableItems bought Items (calls to Counter.PlaceOnCounter) still lead to only
     maxBuyableItems on the Counter and the boughtItems List. - Test this for 1 item and 6 items. 
     Test 6 items with 6 same and 1 2 3 (1 test for each). Get buyable Object via a prefab and 
     adressables if possible. -> TestCase()*/
    [TestCase()]
    public void PlaceOnCounter_removes_BuyableObject_component_after_cloning() {
        
    }

    [Test]
    public void PlaceOnCounter_places_bought_item_with_correct_y_offset() {
        //ARRANGE -> happens in SetUp()
        //ACT
        //get 
        //ASSERT
    }

    [OneTimeTearDown]
    public void TearDown() {
        EditorSceneManager.CloseScene(SceneManager.GetSceneByPath("Assets/Scenes/ShopScene.unity"), 
            false);
    }
}
