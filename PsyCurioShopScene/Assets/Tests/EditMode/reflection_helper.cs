using System.Reflection;
using UnityEngine;

namespace Tests.EditMode {
    /// <summary>
    /// Contains Methods for invoking/setting non-public instance methods and fields via reflection.
    /// ONLY USE FOR TESTS!
    /// Used to test Awake functions and other internal stuff.
    /// Code taken from
    /// https://stackoverflow.com/questions/672501/is-there-an-easy-way-to-use-internalsvisibletoattribute
    /// and https://stackoverflow.com/questions/249847/how-do-you-test-private-methods-with-nunit 
    /// </summary>
    public static class reflection_helper {
        /// <summary>
        /// Invoke non-public method with methodName on given instance.
        /// Attention: Does only work on instances of classes
        /// </summary>
        /// <param name="instance"> </param>
        /// <param name="methodName"> </param>
        public static void InvokePrivateMethod(Object instance, string methodName) {
            MethodInfo methodInfo = instance.GetType().GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(instance, null);
        }

        /// <summary>
        /// Set non-public bool Field on given instance to targetValue.
        /// Attention: Does only work on instances of classes
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <param name="targetValue"></param>
        public static void SetPrivateBoolField(Object instance, string fieldName, bool targetValue) {
            FieldInfo fieldInfo = instance.GetType().GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(instance, targetValue);
        }
    }
}