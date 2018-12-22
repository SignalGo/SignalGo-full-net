using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// validation builder extensions
    /// </summary>
    public static class ValidationBuilderExtensions
    {
        /// <summary>
        /// add properties
        /// </summary>
        /// <param name="validationBuilder"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static void Add<T>(ValidationBuilder validationBuilder, string property)
        {
            AddValidation(validationBuilder, typeof(T), property);
        }

        private static void AddValidation(ValidationBuilder validationBuilder, object typeOrInstance, string property)
        {
            if (typeOrInstance == null)
                throw new Exception($"your object cannot be null to add as attribute");
            else if (typeOrInstance is Type type && !type.GetAllInheritances().Contains(typeof(BaseValidationRuleInfoAttribute)))
                throw new Exception($"Type of T is not a BaseValidationRuleInfoAttribute Type T is {type.FullName}");

            if (!validationBuilder.PropertiesValidations.ContainsKey(property))
                validationBuilder.PropertiesValidations[property] = new List<object>();

            if (!validationBuilder.PropertiesValidations[property].Contains(typeOrInstance))
                validationBuilder.PropertiesValidations[property].Add(typeOrInstance);
        }

        /// <summary>
        /// add validation by instance
        /// </summary>
        /// <param name="validationBuilder"></param>
        /// <param name="validation"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static ValidationBuilder Add(this ValidationBuilder validationBuilder, BaseValidationRuleInfoAttribute validation, params string[] properties)
        {
            foreach (string item in properties)
            {
                AddValidation(validationBuilder, validation, item);
            }
            return validationBuilder;
        }

        public static ValidationBuilder Add(this ValidationBuilder validationBuilder, BaseValidationRuleInfoAttribute validation1, BaseValidationRuleInfoAttribute validation2, params string[] properties)
        {
            foreach (string item in properties)
            {
                AddValidation(validationBuilder, validation1, item);
                AddValidation(validationBuilder, validation2, item);
            }
            return validationBuilder;
        }

        public static ValidationBuilder Add(this ValidationBuilder validationBuilder, BaseValidationRuleInfoAttribute validation1, BaseValidationRuleInfoAttribute validation2, BaseValidationRuleInfoAttribute validation3, params string[] properties)
        {
            foreach (string item in properties)
            {
                AddValidation(validationBuilder, validation1, item);
                AddValidation(validationBuilder, validation2, item);
                AddValidation(validationBuilder, validation3, item);
            }
            return validationBuilder;
        }

        public static ValidationBuilder Add(this ValidationBuilder validationBuilder, BaseValidationRuleInfoAttribute[] validations, params string[] properties)
        {
            foreach (string item in properties)
            {
                foreach (BaseValidationRuleInfoAttribute validation in validations)
                {
                    AddValidation(validationBuilder, validation, item);
                }
            }
            return validationBuilder;
        }

        /// <summary>
        /// add validation by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validationBuilder"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static ValidationBuilder Add<T>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            foreach (string item in properties)
            {
                Add<T>(validationBuilder, item);
            }
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1>(validationBuilder, properties);
            Add<T2>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2>(validationBuilder, properties);
            Add<T3>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3>(validationBuilder, properties);
            Add<T4>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4>(validationBuilder, properties);
            Add<T5>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5>(validationBuilder, properties);
            Add<T6>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6>(validationBuilder, properties);
            Add<T7>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7, T8>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6, T7>(validationBuilder, properties);
            Add<T8>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6, T7, T8>(validationBuilder, properties);
            Add<T9>(validationBuilder, properties);
            return validationBuilder;
        }
        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6, T7, T8, T9>(validationBuilder, properties);
            Add<T10>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(validationBuilder, properties);
            Add<T11>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(validationBuilder, properties);
            Add<T12>(validationBuilder, properties);
            return validationBuilder;
        }
        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(validationBuilder, properties);
            Add<T13>(validationBuilder, properties);
            return validationBuilder;
        }

        public static ValidationBuilder Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(validationBuilder, properties);
            Add<T14>(validationBuilder, properties);
            return validationBuilder;
        }

        /// <summary>
        /// add your rules to your server provider
        /// </summary>
        /// <param name="type"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static ValidationBuilder Validation(this Type type, IValidationRuleInfo provider)
        {
            return new ValidationBuilder(type, provider);
        }

        public static ValidationBuilder Add(this ValidationBuilder validationBuilder, List<Type> types, params string[] properties)
        {
            foreach (Type type in types)
            {
                foreach (string property in properties)
                {
                    AddValidation(validationBuilder, type, property);
                }
            }
            return validationBuilder;
        }

        /// <summary>
        /// build the validation
        /// </summary>
        /// <param name="validationBuilder"></param>
        public static void Build(this ValidationBuilder validationBuilder)
        {
            ValidationRuleInfoManager manager = validationBuilder.Provider.ValidationRuleInfoManager;
            if (!manager.FluentValidationRules.TryGetValue(validationBuilder.Type, out Dictionary<string, List<object>> properties))
            {
                properties = new Dictionary<string, List<object>>();
                manager.FluentValidationRules.TryAdd(validationBuilder.Type, properties);
            }
            foreach (KeyValuePair<string, List<object>> property in validationBuilder.PropertiesValidations)
            {
                if (!properties.TryGetValue(property.Key, out List<object> attributes))
                {
                    attributes = new List<object>();
                    properties.Add(property.Key, attributes);
                }

                foreach (object attribe in property.Value)
                {
                    if (!attributes.Contains(attribe))
                        attributes.Add(attribe);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ValidationBuilder
    {
        /// <summary>
        /// server provider object
        /// </summary>
        public IValidationRuleInfo Provider { get; set; }
        internal ValidationBuilder(Type type, IValidationRuleInfo provider)
        {
            Provider = provider;
            Type = type;
        }
        /// <summary>
        /// type of validations properties
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// list of rules support
        /// </summary>
        public Dictionary<string, List<object>> PropertiesValidations { get; internal set; } = new Dictionary<string, List<object>>();

    }
    /// <summary>
    /// validation rule infoes
    /// </summary>
    public interface IValidationRuleInfo
    {
        /// <summary>
        /// validation rules manager
        /// </summary>
        ValidationRuleInfoManager ValidationRuleInfoManager { get; set; }
    }
    /// <summary>
    /// manage validation rules add,remove etc
    /// </summary>
    public class ValidationRuleInfoManager
    {
        private ConcurrentDictionary<int, ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>>> CurrentDetectedObjects { get; set; } = new ConcurrentDictionary<int, ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>>>();
        private ConcurrentDictionary<int, List<BaseValidationRuleInfoAttribute>> CurrentValidationRules { get; set; } = new ConcurrentDictionary<int, List<BaseValidationRuleInfoAttribute>>();
        internal ConcurrentDictionary<Type, Dictionary<string, List<object>>> FluentValidationRules { get; set; } = new ConcurrentDictionary<Type, Dictionary<string, List<object>>>();
        /// <summary>
        /// add role to current task
        /// </summary>
        /// <param name="currentTaskId"></param>
        /// <param name="validationRuleInfo"></param>
        public void AddRule(int? currentTaskId, BaseValidationRuleInfoAttribute validationRuleInfo)
        {
            if (!currentTaskId.HasValue)
                return;
            int currentId = currentTaskId.GetValueOrDefault();
            if (CurrentValidationRules.TryGetValue(currentId, out List<BaseValidationRuleInfoAttribute> items))
            {
                items.Add(validationRuleInfo);
            }
            else
            {
                items = new List<BaseValidationRuleInfoAttribute>
                {
                    validationRuleInfo
                };
                CurrentValidationRules.TryAdd(currentId, items);
            }
        }

        /// <summary>
        /// add object properties as check in current task
        /// </summary>
        /// <param name="currentTaskId"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyInfo"></param>
        public void AddObjectPropertyAsChecked(int? currentTaskId, Type type, object instance, string propertyName, PropertyInfo propertyInfo, object currentValue)
        {
            if (!currentTaskId.HasValue || instance == null)
                return;
            int currentId = currentTaskId.GetValueOrDefault();
            if (CurrentDetectedObjects.TryGetValue(currentId, out ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>> types))
            {
                if (types.TryGetValue(type, out ConcurrentDictionary<object, List<string>> objects))
                {
                    if (objects.TryGetValue(instance, out List<string> properties))
                    {
                        if (!properties.Contains(propertyName) && !string.IsNullOrEmpty(propertyName))
                            properties.Add(propertyName);
                    }
                    else
                    {
                        properties = new List<string>();
                        if (!string.IsNullOrEmpty(propertyName))
                            properties.Add(propertyName);
                        objects.TryAdd(instance, properties);
                    }
                }
                else
                {
                    objects = new ConcurrentDictionary<object, List<string>>();
                    List<string> properties = new List<string>();
                    if (!string.IsNullOrEmpty(propertyName))
                        properties.Add(propertyName);
                    objects.TryAdd(instance, properties);
                    types.TryAdd(type, objects);
                }
            }
            else
            {
                types = new ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>>();
                ConcurrentDictionary<object, List<string>> objects = new ConcurrentDictionary<object, List<string>>();
                List<string> properties = new List<string>();
                if (!string.IsNullOrEmpty(propertyName))
                    properties.Add(propertyName);
                objects.TryAdd(instance, properties);
                types.TryAdd(type, objects);
                CurrentDetectedObjects.TryAdd(currentId, types);
            }

            if (propertyName != null && FluentValidationRules.TryGetValue(type, out Dictionary<string, List<object>> validations))
            {
                if (validations.TryGetValue(propertyName, out List<object> attributes))
                {
                    foreach (object attribute in attributes)
                    {
                        BaseValidationRuleInfoAttribute attributeInstance = null;
                        if (attribute is Type attributeType)
                        {
                            try
                            {
                                attributeInstance = (BaseValidationRuleInfoAttribute)Activator.CreateInstance(attributeType);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"I went to create instance of your attribute by type {attributeType.FullName} but it had Exception, are you made constructor for that? see the inner exception for more details", ex);
                            }
                        }
                        else
                        {
                            attributeInstance = (BaseValidationRuleInfoAttribute)attribute;
                        }

                        attributeInstance.PropertyInfo = propertyInfo;
                        attributeInstance.Object = instance;
                        attributeInstance.CurrentValue = currentValue;
                        AddRule(currentTaskId, attributeInstance);
                    }
                }
            }
        }
        /// <summary>
        /// remove validations from a taskId
        /// </summary>
        /// <param name="taskId"></param>
        public void Remove(int taskId)
        {
            CurrentValidationRules.Remove(taskId);
            CurrentDetectedObjects.Remove(taskId);
        }

        /// <summary>
        /// calculate all validations
        /// </summary>
        /// <returns>return list of validation errors</returns>
        public IEnumerable<BaseValidationRuleInfoAttribute> CalculateValidationsOfTask(Action<string, object> changeParameterValueAction, Action<BaseValidationRuleInfoAttribute> fillValidationParameterAction)
        {
            if (!Task.CurrentId.HasValue)
                throw new Exception("cannot calculate rules without any task!");
            int currentId = Task.CurrentId.GetValueOrDefault();
            if (CurrentValidationRules.TryGetValue(currentId, out List<BaseValidationRuleInfoAttribute> validationRuleInfoAttributes))
            {
                foreach (BaseValidationRuleInfoAttribute validation in validationRuleInfoAttributes)
                {
                    fillValidationParameterAction(validation);
                    if (validation.TaskType == ValidationRuleInfoTaskType.Error)
                    {
                        if (!BaseValidationRuleInfoAttribute.CheckIsValidate(validation))
                            yield return validation;
                    }
                    else if (validation.TaskType == ValidationRuleInfoTaskType.ChangeValue)
                    {
                        if (!BaseValidationRuleInfoAttribute.CheckIsValidate(validation))
                        {
                            object changedValue = BaseValidationRuleInfoAttribute.GetChangedValue(validation);
                            if (validation.PropertyInfo != null)
                            {
                                System.Reflection.PropertyInfo findProperty = validation.Object.GetType().GetPropertyInfo(validation.PropertyInfo.Name);
                                findProperty.SetValue(validation.Object, changedValue, null);
                            }
                            else
                            {
                                changeParameterValueAction(validation.ParameterInfo.Name, changedValue);
                            }
                        }
                    }
                    else
                        throw new NotSupportedException();
                }
            }

            if (CurrentDetectedObjects.TryGetValue(currentId, out ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>> types))
            {
                foreach (KeyValuePair<Type, ConcurrentDictionary<object, List<string>>> tkv in types)
                {
                    Type type = tkv.Key;
                    if (types.TryGetValue(type, out ConcurrentDictionary<object, List<string>> objects))
                    {
                        foreach (KeyValuePair<object, List<string>> okv in objects)
                        {
                            if (objects.TryGetValue(okv.Key, out List<string> properties))
                            {
                                foreach (BaseValidationRuleInfoAttribute item in CalculateArrays(okv.Key, properties, currentId))
                                {
                                    yield return item;
                                }
                            }
                        }
                    }
                }

            }
        }

        private IEnumerable<BaseValidationRuleInfoAttribute> CalculateArrays(object instance, List<string> properties, int? currentTaskId)
        {
            bool isArray = typeof(IEnumerable).GetIsAssignableFrom(instance.GetType()) && !(instance is string);
            bool isDictionary = typeof(IDictionary).GetIsAssignableFrom(instance.GetType());
            if (isArray)
            {
                foreach (object item in (IEnumerable)instance)
                {
                    if (CalculateArrays(item, properties, currentTaskId) is BaseValidationRuleInfoAttribute validation)
                        yield return validation;
                }
            }
            else if (isDictionary)
            {
                foreach (DictionaryEntry item in (IDictionary)instance)
                {
                    if (CalculateArrays(item.Key, properties, currentTaskId) is BaseValidationRuleInfoAttribute validation)
                        yield return validation;
                    if (CalculateArrays(item.Value, properties, currentTaskId) is BaseValidationRuleInfoAttribute validation2)
                        yield return validation2;
                }
            }
            else
            {
                foreach (BaseValidationRuleInfoAttribute validation in CalculateObject(instance, properties, currentTaskId))
                {
                    yield return validation;
                }
            }
        }

        private IEnumerable<BaseValidationRuleInfoAttribute> CalculateObject(object instance, List<string> properties, int? currentTaskId)
        {
            foreach (System.Reflection.PropertyInfo property in instance.GetType().GetListOfProperties())
            {
                if (properties.Contains(property.Name))
                    continue;
                object currentValue = property.GetValue(instance, null);
                foreach (BaseValidationRuleInfoAttribute validation in GetPropertyBaseValidationRuleInfoAttributes(instance.GetType(), property, instance, currentValue, currentTaskId))
                {
                    if (validation.TaskType == ValidationRuleInfoTaskType.Error)
                    {
                        if (!BaseValidationRuleInfoAttribute.CheckIsValidate(validation))
                        {
                            validation.PropertyInfo = property;
                            validation.Object = instance;
                            validation.CurrentValue = currentValue;
                            yield return validation;
                        }
                    }
                    else if (validation.TaskType == ValidationRuleInfoTaskType.ChangeValue)
                    {
                        if (!BaseValidationRuleInfoAttribute.CheckIsValidate(validation))
                        {
                            object changedValue = BaseValidationRuleInfoAttribute.GetChangedValue(validation);
                            System.Reflection.PropertyInfo findProperty = instance.GetType().GetPropertyInfo(property.Name);
                            findProperty.SetValue(instance, changedValue, null);
                        }
                    }
                    else
                        throw new NotSupportedException();
                }
            }
        }

        private IEnumerable<BaseValidationRuleInfoAttribute> GetPropertyBaseValidationRuleInfoAttributes(Type type, PropertyInfo propertyInfo, object instance, object currentValue, int? currentTaskId)
        {
            foreach (BaseValidationRuleInfoAttribute item in propertyInfo.GetCustomAttributes<BaseValidationRuleInfoAttribute>(true))
            {
                yield return item;
            }

            foreach (BaseValidationRuleInfoAttribute item in GetPropertyFluentBaseValidationRuleInfoAttributes(type, propertyInfo, instance, currentValue, currentTaskId))
            {
                yield return item;
            }
        }

        private IEnumerable<BaseValidationRuleInfoAttribute> GetPropertyFluentBaseValidationRuleInfoAttributes(Type type, PropertyInfo propertyInfo, object instance, object currentValue, int? currentTaskId)
        {
            if (propertyInfo != null)
            {
                if (FluentValidationRules.TryGetValue(type, out Dictionary<string, List<object>> validations))
                {
                    if (validations.TryGetValue(propertyInfo.Name, out List<object> attributes))
                    {
                        foreach (object attribute in attributes)
                        {
                            BaseValidationRuleInfoAttribute attributeInstance = null;
                            if (attribute is Type attributeType)
                            {
                                try
                                {
                                    attributeInstance = (BaseValidationRuleInfoAttribute)Activator.CreateInstance(attributeType);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"I went to create instance of your attribute by type {attributeType.FullName} but it had Exception, are you made constructor for that? see the inner exception for more details", ex);
                                }
                            }
                            else
                            {
                                attributeInstance = (BaseValidationRuleInfoAttribute)attribute;
                            }

                            attributeInstance.PropertyInfo = propertyInfo;
                            attributeInstance.Object = instance;
                            attributeInstance.CurrentValue = currentValue;
                            AddRule(currentTaskId, attributeInstance);
                            yield return attributeInstance;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// remove tasks from memory
        /// </summary>
        /// <param name="TaskId"></param>
        public void RemoveTask(int TaskId)
        {
            CurrentDetectedObjects.Remove(TaskId);
            CurrentValidationRules.Remove(TaskId);
        }
    }
}
