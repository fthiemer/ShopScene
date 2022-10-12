using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text.RegularExpressions;
using NSubstitute;
using UnityEngine.TestTools;

namespace Tests.EditMode {
    public class cash_register {
        private GameObject cashRegisterObject;
        private CashRegister cashRegisterComponent;
        private GameObject[] buyableObjects;
        /// <summary>
        /// Which items to buy for the testcases. E.g. {0,0,0,0,0} would mean to buy the item
        /// at at index 0 in the buyableObjects list 5 times, while {0,1,2,3,4} buys every
        /// item in buyableObjects (given it holds 5 items) once. 
        /// </summary>
        private List<List<int>> itemIndexForTestCases = new List<List<int>>() {
            new List<int>() {0,0,0,0,0}, // 5 times same item
            new List<int>() {0,1,2,3,4}, // 5 different items
            new List<int>() {0,2,0,0,0}, // 2 Items, 1 4 split
            new List<int>() {2,2,0,0,0}, // 2 Items, 2 3 split
            new List<int>() {0,0,0,2,1}, // 3 Items, 1 1 3 split
            new List<int>() {1,2,0,2,0},  // 3 Items, 1 2 2 split
            new List<int>() {1,2,3,2,4}  // 4 items
        };


        [SetUp]
        public void SetUp() {
            EditorSceneManager.OpenScene("Assets/Scenes/ShopScene.unity", OpenSceneMode.Single);
            cashRegisterObject = GameObject.FindWithTag(Tags.CashRegister);
            cashRegisterComponent = cashRegisterObject.GetComponent<CashRegister>();
            ReflectionHelper.InvokePrivateVoidMethod(cashRegisterComponent, "Awake");
            ReflectionHelper.InvokePrivateVoidMethod(cashRegisterComponent, "Start");
            ReflectionHelper.InvokePrivateVoidMethod(cashRegisterComponent.counter, "Awake");
            buyableObjects = GameObject.FindGameObjectsWithTag(Tags.Item);
        }

        [Test]
        public void ConstructBillMessage_if_Counter_BoughtItems_empty_returns_zeroItemsBoughtMsg() {
            //ARRANGE
            //ACT
            string constructedMessage =
                ReflectionHelper.InvokePrivateNonVoidMethod<string>(cashRegisterComponent, 
                    "ConstructBillMessage") as string;
            //ASSERT
            string zeroItemsBoughtMsg = ReflectionHelper.GetPrivateFieldOfType<string>(cashRegisterComponent,
                "zeroItemsBoughtMsg") as string;
            Assert.AreEqual(zeroItemsBoughtMsg, constructedMessage);
            //CLEANUP - not needed as method doesnt change anything & SetUp reinstantiates everything
        }
        
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void ConstructBillMessage_holds_correct_total_price_in_last_message_line(int testCaseIndex) {
            //ARRANGE - Fill boughtItems List depending on TestCase
            List<int> itemsToBuyIndexList = itemIndexForTestCases[testCaseIndex];
            var tmpBoughtItems = new List<Buyable>(itemsToBuyIndexList.Count);
            foreach (int itemIndex in itemsToBuyIndexList) {
                tmpBoughtItems.Add(buyableObjects[itemIndex].GetComponent<Buyable>());
                var placedObject = cashRegisterComponent.counter.PlaceOnCounter(buyableObjects[itemIndex]);
                Assert.NotNull(placedObject);
            }
            // calculate correct total price
            float totalPrice = 0;
            foreach (var boughtItem in tmpBoughtItems) {
                totalPrice += boughtItem.Price;
            }
            
            // Convert to string with decimal point instead of comma and two decimals after the dot
            string totalPriceString = totalPrice.ToString("#.##", CultureInfo.InvariantCulture);

            //ACT - construct message
            string constructedMessage =
                ReflectionHelper.InvokePrivateNonVoidMethod<string>(cashRegisterComponent, 
                    "ConstructBillMessage") as string;
            
            //ASSERT
            // Compare calculated price string with extracted one, from the last line of the msg
            string lastLine = Regex.Split(constructedMessage, "\n")[^1];
            var billMessageTotalPriceString = Regex.Match(lastLine, @"\d+\.?\d?\d?").Value;
            Assert.AreEqual(totalPriceString, billMessageTotalPriceString);

            //CLEANUP - not necessary, as SetUp Reinitializes Scene
        }

        [Test]
        public void Counter_PlaceOnCounter_if_CashRegister_clicked_triggers_speechbubble_update() {
            //ARRANGE - Set up substitute and click cash register to set into bill update mode
            var shopkeeperSubstitute = Substitute.For<IShopkeeper>();
            cashRegisterComponent.shopkeeper = shopkeeperSubstitute;
            cashRegisterComponent.OnPointerClick(null);
            //ACT - place Item on counter
            cashRegisterComponent.counter.PlaceOnCounter(buyableObjects[0]);
            //ASSERT - 
            // Get bill message corresponding to current scene state
            string billMessage = ReflectionHelper.InvokePrivateNonVoidMethod<string>(cashRegisterComponent, 
                "ConstructBillMessage") as string;
            cashRegisterComponent.shopkeeper.Received(1).Say(billMessage);
            //CLEANUP - Setup Reinitializes Scene
            cashRegisterComponent.shopkeeper.ClearReceivedCalls();
        }
        
        [Test]
        public void Counter_PlaceOnCounter_if_CashRegister_not_clicked_doesnt_trigger_speechbubble_update() {
            //ARRANGE - Set up substitute and click cash register to set into bill update mode
            var shopkeeperSubstitute = Substitute.For<IShopkeeper>();
            cashRegisterComponent.shopkeeper = shopkeeperSubstitute;
            //ACT - place Item on counter
            cashRegisterComponent.counter.PlaceOnCounter(buyableObjects[0]);
            //ASSERT - 
            // Get bill message corresponding to current scene state
            string billMessage = ReflectionHelper.InvokePrivateNonVoidMethod<string>(cashRegisterComponent, 
                "ConstructBillMessage") as string;
            cashRegisterComponent.shopkeeper.Received(0).Say(billMessage);
            //CLEANUP - Setup Reinitializes Scene
            cashRegisterComponent.shopkeeper.ClearReceivedCalls();
        }
    }
}
