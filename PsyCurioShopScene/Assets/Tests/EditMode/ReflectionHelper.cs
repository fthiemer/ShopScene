using System;
using System.Diagnostics;
using System.Reflection;

namespace Tests.EditMode {
    /// <summary>
    /// Contains Methods for invoking/setting non-public instance methods and fields via reflection.
    /// ONLY USE FOR TESTS!
    /// Used to test Awake functions and other internal stuff.
    /// Code taken from
    /// https://stackoverflow.com/questions/672501/is-there-an-easy-way-to-use-internalsvisibletoattribute
    /// and https://stackoverflow.com/questions/249847/how-do-you-test-private-methods-with-nunit 
    /// </summary>
    public static class ReflectionHelper {

        /// <summary>
        /// Invoke non-void, non-public method with methodName on given instance.
        /// Attention: Does only work on instances of classes and all output is cast to an Object
        /// </summary>
        /// <param name="instance"> The class instance holding the method to invoke.</param>
        /// <param name="methodName"> The name of the method to invoke. </param>
        /// <param name="parameters"> Leave empty when the method doesnt take parameters. </param>
        public static void InvokePrivateVoidMethod(Object instance, string methodName, object[] parameters=null) {
            MethodInfo methodInfo = instance.GetType().GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(methodInfo != null, nameof(methodInfo) + " != null");
            methodInfo.Invoke(instance, parameters);
        }

        /// <summary>
        /// Invoke non-void, non-public method with methodName on given instance.
        /// Attention: Does only work on instances of classes and all output is cast to an Object
        /// </summary>
        /// <param name="instance"> The class instance holding the method to invoke.</param>
        /// <param name="methodName"> The name of the method to invoke. </param>
        /// <param name="parameters"> Leave empty when the method doesnt take parameters. </param>
        public static Object InvokePrivateNonVoidMethod<T>(Object instance, string methodName, object[] parameters=null) {
            MethodInfo methodInfo = instance.GetType().GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(methodInfo != null, nameof(methodInfo) + " != null");
            return Convert.ChangeType(methodInfo.Invoke(instance, parameters), typeof(T));

        }

        /// <summary>
        /// Set non-public bool Field on given instance to targetValue.
        /// Attention: Does only work on instances of classes
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <param name="targetValue"></param>
        public static void SetPrivateFieldOfType<T>(Object instance, string fieldName, T targetValue) {
            FieldInfo fieldInfo = instance.GetType().GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(fieldInfo != null, nameof(fieldInfo) + " != null");
            fieldInfo.SetValue(instance, targetValue);
        }

        public static object GetPrivateFieldOfType<T>(Object instance, string fieldName) {
            FieldInfo fieldInfo = instance.GetType().GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(fieldInfo != null, nameof(fieldInfo) + " != null");
            return Convert.ChangeType(fieldInfo.GetValue(instance), typeof(T));
        }
    }
}