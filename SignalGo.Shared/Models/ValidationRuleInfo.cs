using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// type of your validation plan
    /// </summary>
    public enum ValidationRuleInfoTaskType : byte
    {
        /// <summary>
        /// send errors to client and skip to call method
        /// </summary>
        Error = 1,
        /// <summary>
        /// change the value of Rule
        /// </summary>
        ChangeValue = 2
    }


    /// <summary>
    /// validation rule to check or change properties before call server methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ValidationRuleInfoAttribute : Attribute
    {
        /// <summary>
        /// task type of validation
        /// </summary>
        public ValidationRuleInfoTaskType TaskType { get; set; } = ValidationRuleInfoTaskType.Error;
        /// <summary>
        /// property of object
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }
        /// <summary>
        /// base object
        /// </summary>
        public object Object { get; internal set; }
        /// <summary>
        /// parameter when that is parameters of method
        /// </summary>
        public ParameterInfo ParameterInfo { get; internal set; }
        /// <summary>
        /// method of service
        /// </summary>
        public MethodInfo MethodInfo { get; internal set; }
        /// <summary>
        /// service that you called method
        /// </summary>
        public object Service { get; internal set; }
        /// <summary>
        /// value of property or parameter came from client
        /// </summary>
        public object CurrentValue { get; internal set; }
        /// <summary>
        /// value of parameters of method
        /// </summary>
        public Dictionary<string, object> ParametersValue { get; internal set; }
        /// <summary>
        /// check if your object is validate
        /// </summary>
        /// <returns>if task is error and you return false you could return object of error if task is changevalue and you return false changevalue method will call and changes the value if you return true not happening</returns>
        public abstract bool CheckIsValidate();
        /// <summary>
        /// change value when tasktype is changevalue and checkisvalidate return false
        /// </summary>
        /// <returns>any object you want to change the current value of property</returns>
        public abstract object GetChangedValue();
        /// <summary>
        /// return value when tasktype is error and checkisvalidate return false
        /// </summary>
        /// <returns>any object you want to send to client as exception or custom result</returns>
        public abstract object GetErrorValue();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="methodInfo"></param>
        /// <param name="parametersValue"></param>
        public void Initialize(object service, MethodInfo methodInfo, Dictionary<string, object> parametersValue)
        {
            Service = service;
            MethodInfo = methodInfo;
            ParametersValue = parametersValue;
        }
    }
}
