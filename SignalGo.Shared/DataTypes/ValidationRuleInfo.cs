using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalGo.Shared.DataTypes
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
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
    public class BaseValidationRuleInfoAttribute : Attribute
    {
        /// <summary>
        /// last state checked
        /// this is current state for call CheckIsValidate after CheckStateIsSuccess
        /// </summary>
        internal object LastStateChecked { get; set; }
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
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="methodInfo"></param>
        /// <param name="parametersValue"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="parameterInfo"></param>
        /// <param name="obj"></param>
        public void Initialize(object service, MethodInfo methodInfo, Dictionary<string, object> parametersValue, PropertyInfo propertyInfo, ParameterInfo parameterInfo, object obj, object currentValue)
        {
            Service = service;
            MethodInfo = methodInfo;
            ParametersValue = parametersValue;
            if (obj != null)
                Object = obj;
            if (propertyInfo != null)
                PropertyInfo = propertyInfo;
            if (parameterInfo != null)
                ParameterInfo = parameterInfo;
            if (currentValue != null)
                CurrentValue = currentValue;
        }

        /// <summary>
        /// check if validation data is validate
        /// </summary>
        /// <param name="baseValidation"></param>
        /// <returns></returns>
        public static bool CheckIsValidate(BaseValidationRuleInfoAttribute baseValidation)
        {
            if (baseValidation is IValidationRuleInfoAttribute validationRuleInfoAttribute)
            {
                return validationRuleInfoAttribute.CheckIsValidate();
            }
            else
            {
                MethodInfo checkIsValidateMethod = baseValidation.GetType().FindMethod("CheckIsValidate");
                if (checkIsValidateMethod == null)
                    throw new Exception($"CheckIsValidate method not found! are you shure you inheritance IValidationRuleInfoAttribute or IValidationRuleInfoAttribute<T> on your attribute {baseValidation.GetType().FullName}");
                object state = checkIsValidateMethod.Invoke(baseValidation, null);
                baseValidation.LastStateChecked = state;
                MethodInfo checkStateIsSuccessMethod = baseValidation.GetType().FindMethod("CheckStateIsSuccess");
                if (checkStateIsSuccessMethod == null)
                    throw new Exception($"CheckStateIsSuccess method not found! are you shure you inheritance IValidationRuleInfoAttribute<T> on your attribute {baseValidation.GetType().FullName}");
                return (bool)checkStateIsSuccessMethod.Invoke(baseValidation, new object[] { state });
            }
        }

        /// <summary>
        /// get changed value
        /// </summary>
        /// <param name="baseValidation"></param>
        /// <returns></returns>
        public static object GetChangedValue(BaseValidationRuleInfoAttribute baseValidation)
        {
            if (baseValidation is IValidationRuleInfoAttribute validationRuleInfoAttribute)
            {
                return validationRuleInfoAttribute.GetChangedValue();
            }
            else
            {
                MethodInfo getChangedValueMethod = baseValidation.GetType().FindMethod("GetChangedValue");
                if (getChangedValueMethod == null)
                    throw new Exception($"GetChangedValue method not found! are you shure you inheritance IValidationRuleInfoAttribute or IValidationRuleInfoAttribute<T> on your attribute {baseValidation.GetType().FullName}");
                object state = baseValidation.GetType().GetProperty("LastStateChecked", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseValidation, null);
                return getChangedValueMethod.Invoke(baseValidation, new object[] { state });
            }
        }
        /// <summary>
        /// get error value
        /// </summary>
        /// <param name="baseValidation"></param>
        /// <returns></returns>
        public static object GetErrorValue(BaseValidationRuleInfoAttribute baseValidation)
        {
            if (baseValidation is IValidationRuleInfoAttribute validationRuleInfoAttribute)
            {
                return validationRuleInfoAttribute.GetErrorValue();
            }
            else
            {
                MethodInfo getErrorValueMethod = baseValidation.GetType().FindMethod("GetErrorValue");
                if (getErrorValueMethod == null)
                    throw new Exception($"GetErrorValue method not found! are you shure you inheritance IValidationRuleInfoAttribute or IValidationRuleInfoAttribute<T> on your attribute {baseValidation.GetType().FullName}");
                object state = baseValidation.GetType().GetProperty("LastStateChecked", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseValidation, null);
                return getErrorValueMethod.Invoke(baseValidation, new object[] { state });
            }
        }
    }

    /// <summary>
    /// validation rule witout custom check states
    /// </summary>
    public abstract class ValidationRuleInfoAttribute : BaseValidationRuleInfoAttribute, IValidationRuleInfoAttribute
    {
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
    }

    /// <summary>
    /// vaidation interface for make better generic template results
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValidationRuleInfoAttribute<T>
    {
        /// <summary>
        /// check if state is success
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        bool CheckStateIsSuccess(T state);
        /// <summary>
        /// check if your object is validate
        /// </summary>
        /// <returns>if task is error and you return false you could return object of error if task is changevalue and you return false changevalue method will call and changes the value if you return true not happening</returns>
        T CheckIsValidate();
        /// <summary>
        /// change value when tasktype is changevalue and checkisvalidate return false
        /// </summary>
        /// <returns>any object you want to change the current value of property</returns>
        object GetChangedValue(T checkState);
        /// <summary>
        /// return value when tasktype is error and checkisvalidate return false
        /// </summary>
        /// <returns>any object you want to send to client as exception or custom result</returns>
        object GetErrorValue(T checkState);
    }

    /// <summary>
    /// vaidation interface without custom states
    /// </summary>
    public interface IValidationRuleInfoAttribute
    {
        /// <summary>
        /// check if your object is validate
        /// </summary>
        /// <returns>if task is error and you return false you could return object of error if task is changevalue and you return false changevalue method will call and changes the value if you return true not happening</returns>
        bool CheckIsValidate();
        /// <summary>
        /// change value when tasktype is changevalue and checkisvalidate return false
        /// </summary>
        /// <returns>any object you want to change the current value of property</returns>
        object GetChangedValue();
        /// <summary>
        /// return value when tasktype is error and checkisvalidate return false
        /// </summary>
        /// <returns>any object you want to send to client as exception or custom result</returns>
        object GetErrorValue();
    }
}
