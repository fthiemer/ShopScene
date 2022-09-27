using System.Collections;
using System.Linq;
using NSubstitute;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class buyable_object_input_test : InputTestFixture {
    private GameObject[] buyableItems;
    private BuyableObject[] buyableObjectComponents;
    private Camera camera;

    /// <summary>
    /// Make sure, all items are clickable in current scene.
    /// Do so by simulating mouse click on their center.
    /// </summary>
    [UnityTest]
    public IEnumerator mouse_click_on_items_calls_ICounter_PlaceOnCounter() {
        base.Setup();
        //ARRANGE 1 - Setup Scene
        SceneManager.LoadScene("Assets/Scenes/ShopScene.unity");
        for (int i = 0; i < 10; i++) {
            yield return null;
        }
        buyableItems = GameObject.FindGameObjectsWithTag(Tags.Item);
        buyableObjectComponents = buyableItems.Select(x => x.GetComponent<BuyableObject>()).ToArray();
            // Replace Counter component of every item with substitute, so that it can be tested for received calls
        for (var i = 0; i < buyableObjectComponents.Length; i++) {
            var buyableObjectComponent = buyableObjectComponents[i];
            var counterSubstitute = Substitute.For<ICounter>();
            buyableObjectComponent.Counter = counterSubstitute;
            // Give PlaceOnCounter() return behaviour, as if it always places the item
            buyableObjectComponent.Counter.PlaceOnCounter(buyableItems[i])
                .Returns(buyableItems[i]);
        }
            //get camera object
        camera = GameObject.FindWithTag(Tags.MainCamera).GetComponent<Camera>();
        
        //ARRANGE 2 - somehow get usable mouse prepared -> easy with Input System \o/
        Mouse mouse = InputSystem.AddDevice<Mouse>();

        //ACT - click on screen pos, that correlates to world pos
        for (int i=0; i<buyableItems.Length; i++) {
            var worldPos = buyableItems[i].transform.position;
            Vector2 screenPos = camera.WorldToScreenPoint(worldPos);
            Set(mouse.position, screenPos, queueEventOnly:false);
            Press(mouse.leftButton);
            yield return null;
            
            Release(mouse.leftButton);
            for (int j = 0; j < 5; j++) {
                yield return null;
            }
                
            //ASSERT
            //Use Received method of Substitute to confirm exactly one call to PlaceOnCounter
            buyableObjectComponents[i].Counter.Received(1).PlaceOnCounter(buyableItems[i]);
                
            //CLEANUP - 1
            buyableObjectComponents[i].Counter.ClearReceivedCalls();
        }

        //CLEANUP - 2
        TearDown();
    }
}
