using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    public class UltraMapDictionary
    {
        private Hashtable Items { get; set; } = new Hashtable();
        object lockTable = new object();

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public void Add(object key, object value)
        {
            lock (lockTable)
            {
                Items.Add(key, value);
            }
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            lock(lockTable)
            {
                if (Items.ContainsKey(key))
                {
                    value = (T)Items[key];
                    return true;
                }
                else
                {
                    value = default(T);
                    return false;
                }
            }
        }


        public void Remove(object key)
        {
            lock (lockTable)
            {
                Items.Remove(key);
            }
        }
    }
}
