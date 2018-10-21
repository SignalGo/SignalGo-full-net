using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            Add(validationBuilder, typeof(T), property);
        }

        private static void Add(ValidationBuilder validationBuilder, Type type, string property)
        {
            if (!validationBuilder.PropertiesValidations.ContainsKey(property))
                validationBuilder.PropertiesValidations[property] = new List<Type>();

            if (!validationBuilder.PropertiesValidations[property].Contains(type))
                validationBuilder.PropertiesValidations[property].Add(type);
        }

        /// <summary>
        /// add validation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validationBuilder"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static ValidationBuilder Add<T>(this ValidationBuilder validationBuilder, params string[] properties)
        {
            if (!typeof(T).GetAllInheritances().Contains(typeof(ValidationRuleInfoAttribute)))
                throw new Exception($"Type of T is not a ValidationRuleInfoAttribute Type T is {typeof(T).FullName}");
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

        public static ValidationBuilder Validation(this Type type)
        {
            return new ValidationBuilder(type);
        }

        public static ValidationBuilder Add(this ValidationBuilder validationBuilder, List<Type> types, params string[] properties)
        {
            foreach (Type type in types)
            {
                foreach (var property in properties)
                {
                    Add(validationBuilder, type, property);
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

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ValidationBuilder
    {
        internal ValidationBuilder(Type type)
        {
            Type = type;
        }
        /// <summary>
        /// type of validations properties
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// list of rules support
        /// </summary>
        public Dictionary<string, List<Type>> PropertiesValidations { get; internal set; } = new Dictionary<string, List<Type>>();

    }

    /// <summary>
    /// manage validation rules add,remove etc
    /// </summary>
    public class ValidationRuleInfoManager
    {
        private ConcurrentDictionary<int, ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>>> CurrentDetectedObjects { get; set; } = new ConcurrentDictionary<int, ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>>>();
        private ConcurrentDictionary<int, List<ValidationRuleInfoAttribute>> CurrentValidationRules { get; set; } = new ConcurrentDictionary<int, List<ValidationRuleInfoAttribute>>();
        /// <summary>
        /// add role to current task
        /// </summary>
        /// <param name="currentTaskId"></param>
        /// <param name="validationRuleInfo"></param>
        public void AddRule(int? currentTaskId, ValidationRuleInfoAttribute validationRuleInfo)
        {
            if (!currentTaskId.HasValue)
                return;
            int currentId = currentTaskId.GetValueOrDefault();
            if (CurrentValidationRules.TryGetValue(currentId, out List<ValidationRuleInfoAttribute> items))
            {
                items.Add(validationRuleInfo);
            }
            else
            {
                items = new List<ValidationRuleInfoAttribute>
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
        public void AddObjectPropertyAsChecked(int? currentTaskId, Type type, object instance, string propertyName)
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
        public IEnumerable<ValidationRuleInfoAttribute> CalculateValidationsOfTask(Action<string, object> changeParameterValueAction, Action<ValidationRuleInfoAttribute> fillValidationParameterAction)
        {
            if (!Task.CurrentId.HasValue)
                throw new Exception("cannot calculate rules without any task!");
            int currentId = Task.CurrentId.GetValueOrDefault();
            if (CurrentValidationRules.TryGetValue(currentId, out List<ValidationRuleInfoAttribute> validationRuleInfoAttributes))
            {
                foreach (ValidationRuleInfoAttribute validation in validationRuleInfoAttributes)
                {
                    fillValidationParameterAction(validation);
                    if (validation.TaskType == ValidationRuleInfoTaskType.Error)
                    {
                        if (!validation.CheckIsValidate())
                            yield return validation;
                    }
                    else if (validation.TaskType == ValidationRuleInfoTaskType.ChangeValue)
                    {
                        if (!validation.CheckIsValidate())
                        {
                            object changedValue = validation.GetChangedValue();
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
                                foreach (ValidationRuleInfoAttribute item in CalculateArrays(okv.Key, properties))
                                {
                                    yield return item;
                                }
                            }
                        }
                    }
                }

            }
        }

        private static IEnumerable<ValidationRuleInfoAttribute> CalculateArrays(object instance, List<string> properties)
        {
            bool isArray = typeof(IEnumerable).GetIsAssignableFrom(instance.GetType()) && !(instance is string);
            bool isDictionary = typeof(IDictionary).GetIsAssignableFrom(instance.GetType());
            if (isArray)
            {
                foreach (object item in (IEnumerable)instance)
                {
                    if (CalculateArrays(item, properties) is ValidationRuleInfoAttribute validation)
                        yield return validation;
                }
            }
            else if (isDictionary)
            {
                foreach (DictionaryEntry item in (IDictionary)instance)
                {
                    if (CalculateArrays(item.Key, properties) is ValidationRuleInfoAttribute validation)
                        yield return validation;
                    if (CalculateArrays(item.Value, properties) is ValidationRuleInfoAttribute validation2)
                        yield return validation2;
                }
            }
            else
            {
                foreach (ValidationRuleInfoAttribute validation in CalculateObject(instance, properties))
                {
                    yield return validation;
                }
            }
        }

        private static IEnumerable<ValidationRuleInfoAttribute> CalculateObject(object instance, List<string> properties)
        {
            foreach (System.Reflection.PropertyInfo property in instance.GetType().GetListOfProperties())
            {
                if (properties.Contains(property.Name))
                    continue;
                object currentValue = property.GetValue(instance, null);
                foreach (ValidationRuleInfoAttribute validation in property.GetCustomAttributes<ValidationRuleInfoAttribute>(true))
                {
                    if (validation.TaskType == ValidationRuleInfoTaskType.Error)
                    {
                        if (!validation.CheckIsValidate())
                        {
                            validation.PropertyInfo = property;
                            validation.Object = instance;
                            validation.CurrentValue = currentValue;
                            yield return validation;
                        }
                    }
                    else if (validation.TaskType == ValidationRuleInfoTaskType.ChangeValue)
                    {
                        if (!validation.CheckIsValidate())
                        {
                            object changedValue = validation.GetChangedValue();
                            System.Reflection.PropertyInfo findProperty = instance.GetType().GetPropertyInfo(property.Name);
                            findProperty.SetValue(instance, changedValue, null);
                        }
                    }
                    else
                        throw new NotSupportedException();

                }
            }
        }

        public static ValidationBuilder Add(Type type)
        {
            return new ValidationBuilder(type);
        }

        public static ValidationBuilder Add<T>()
        {
            return Add(typeof(T));
        }
    }
}
