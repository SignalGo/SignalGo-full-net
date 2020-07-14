using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Extensions
{
    /// <summary>
    /// Check Many Type's And Variable's And Do Validation. 
    /// Like Null/Empty/Zero/Iteration Check's
    /// </summary>
    public static class ValueCheckerExtensions
    {
        /// <summary>
        /// Check if the string has a value or it's null [Or WhiteSpace]
        /// </summary>
        /// <param name="str">string to Check</param>
        /// <param name="HaveWhiteSpace">Check WhiteSpace?</param>
        /// <returns>True If Not NullOrEmpty/[WhiteSpace]</returns>
        public static bool HasValue(this string str, bool HaveWhiteSpace = false)
        {
            if (HaveWhiteSpace)
            {
                if (string.IsNullOrWhiteSpace(str))
                    return false;
            }
            else
            {
                if (string.IsNullOrEmpty(str))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Check that the number is greater than 0
        /// </summary>
        /// <param name="number">Number to Check</param>
        /// <returns>True If number value Not And Greater than Zero(0)</returns>
        public static bool HasValue(this int number)
        {
            if (number <= 0)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Check if the task is completed or not yet
        /// </summary>
        /// <typeparam name="T">Type of task</typeparam>
        /// <param name="task">Task to check state</param>
        /// <returns>True If the task is completed, otherwise false</returns>
        public static bool HasCompleted<T>(this Task<T> task)
        {
            if (task.IsCompleted)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Check if the Task has a Result/Value
        /// </summary>
        /// <typeparam name="T">Task Type</typeparam>
        /// <param name="obj">Task to check</param>
        /// <returns>True If Task has Result, False If null</returns>
        public static bool HasValue<T>(this Task<T> obj)
        {
            if (obj != null && obj.Result != null)
                return true;
            return false;
        }
        /// <summary>
        /// Check if the object has any value or it's null
        /// </summary>
        /// <param name="obj">object to ckeck</param>
        /// <returns>True If Object was'nt null</returns>
        public static bool HasValue(this object obj)
        {
            if (obj == null)
                return false;
            else
                return true;
        }

        public static bool HasValue(this Services.PublisherServiceProvider provider)
        {
            if (provider == null)
                return false;
            else if (provider.CurrentClientProvider.IsConnected)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Check if the collection contain any item/element
        /// </summary>
        /// <typeparam name="T">type of list</typeparam>
        /// <param name="list"></param>
        /// <returns>true if any item was found, otherwise false</returns>
        public static bool HasValue<T>(this ICollection<T> list)
        {
            if (list != null && list.Any())
                return true;
            return false;
        }
        /// <summary>
        /// Check if the observable collection contain any item/element
        /// </summary>
        /// <typeparam name="T">type of collection</typeparam>
        /// <param name="collection"></param>
        /// <returns>true if any item was found, otherwise false</returns>
        public static bool HasValue<T>(this ObservableCollection<T> collection)
        {
            if (collection != null && collection.Any())
                return true;
            return false;
        }
    }
}
