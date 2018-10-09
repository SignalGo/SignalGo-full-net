using SignalGo.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// manage validation rules add,remove etc
    /// </summary>
    public class ValidationRuleInfoManager
    {
        private ConcurrentDictionary<int, ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>>> CurrentDetectedObjects { get; set; } = new ConcurrentDictionary<int, ConcurrentDictionary<Type, ConcurrentDictionary<object, List<string>>>>();
        private ConcurrentDictionary<int, List<ValidationRuleInfoAttribute>> CurrentValidationRules { get; set; } = new ConcurrentDictionary<int, List<ValidationRuleInfoAttribute>>();

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
                                foreach (var item in CalculateArrays(okv.Key, properties))
                                {
                                    yield return item;
                                }
                            }
                        }
                    }
                }

            }
        }

        static IEnumerable<ValidationRuleInfoAttribute> CalculateArrays(object instance, List<string> properties)
        {
            bool isArray = typeof(IEnumerable).GetIsAssignableFrom(instance.GetType()) && !(instance is string);
            bool isDictionary = typeof(IDictionary).GetIsAssignableFrom(instance.GetType());
            if (isArray)
            {
                foreach (var item in (IEnumerable)instance)
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
                foreach (var validation in CalculateObject(instance, properties))
                {
                    yield return validation;
                }
            }
        }

        static IEnumerable<ValidationRuleInfoAttribute> CalculateObject(object instance, List<string> properties)
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
    }
}
