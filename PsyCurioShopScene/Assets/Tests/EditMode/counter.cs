using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NSubstitute;

public class counter
{

    [Test]
    public void max_buyable_items_equals_number_of_child_objects() {
        int numChildrenToAdd = 5; 
        //ARRANGE
        var counterObject = new GameObject();
        GameObject emptyGameObject = new GameObject();
        for (int i = 0; i < numChildrenToAdd; i++) {
            Object.Instantiate(emptyGameObject, counterObject.transform);
        }
        //Awake is called when Counter component is added
        // thus add it after adding children
        counterObject.AddComponent<Counter>();
        
        
        
        
        //ACT
        //yield return null;


        //ASSERT
        var counter = counterObject.GetComponent<Counter>();
        Assert.AreEqual(numChildrenToAdd, counter.MaxBuyableItems);
    }
}
