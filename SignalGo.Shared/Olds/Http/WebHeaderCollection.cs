using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Http
{
    public class WebHeaderCollection : IDictionary<string, string[]>, ICollection<KeyValuePair<string, string[]>>, IEnumerable<KeyValuePair<string, string[]>>, IEnumerable
    {

        /// <summary>
        /// get http header from response
        /// </summary>
        /// <param name="lines">lines of headers</param>
        /// <returns>http headers</returns>
        public static Shared.Http.WebHeaderCollection GetHttpHeaders(string[] lines)
        {
            Shared.Http.WebHeaderCollection result = new Shared.Http.WebHeaderCollection();
            foreach (string item in lines)
            {
                string[] keyValues = item.Split(new[] { ':' }, 2);
                if (keyValues.Length > 1)
                {
                    result.Add(keyValues[0], keyValues[1].TrimStart());
                }
            }
            return result;
        }
        private ConcurrentDictionary<string, string[]> Items { get; set; } = new ConcurrentDictionary<string, string[]>();
        //
        // Summary:
        //     Gets or sets the specified response header.
        //
        // Parameters:
        //   name:
        //     The name of the specified request header.
        //
        // Returns:
        //     The specified response header.
        public string this[string key]
        {
            get
            {
                key = key.ToLower();
                Items.TryGetValue(key, out string[] values);
                return values.FirstOrDefault();
            }
            set
            {
                key = key.ToLower();
                Add(key, new string[] { value });

            }
        }


        string[] IDictionary<string, string[]>.this[string key]
        {
            get
            {
                key = key.ToLower();
                Items.TryGetValue(key, out string[] values);
                return values;
            }

            set
            {
                Add(key, value);
            }
        }
        //
        // Summary:
        //     Gets the number of headers in the collection.
        //
        // Returns:
        //     An System.Int32 indicating the number of headers in a request.
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return Items.Keys;
            }
        }

        public ICollection<string[]> Values
        {
            get
            {
                return Items.Values;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }


        //
        // Summary:
        //     Removes the specified header from the collection.
        //
        // Parameters:
        //   name:
        //     The name of the header to remove from the collection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     name is nullSystem.String.Empty.
        //
        //   T:System.ArgumentException:
        //     name is a restricted header.-or- name contains invalid characters.
        public void Remove(string name)
        {
            Items.Remove(name.ToLower());
        }

        public bool ExistHeader(string header)
        {
            return Items.ContainsKey(header.ToLower());
        }

        public void Add(string key, string value)
        {
            Add(key, value.Split(','));
            //Items.AddOrUpdate(header.ToLower(), new KeyValuePair<string, string>(header, value), (x, old) => new KeyValuePair<string, string>(header, value));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string[]> item in Items)
            {
                if (item.Value == null)
                    builder.AppendLine(item.Key + ": ");
                else
                    builder.AppendLine(item.Key + ": " + string.Join(",", item.Value));
            }
            builder.AppendLine();
            return builder.ToString();
        }

        public bool ContainsKey(string key)
        {
            return Items.ContainsKey(key.ToLower());
        }

        public void Add(string key, string[] value)
        {
            key = key.ToLower();
            if (Items.TryGetValue(key, out string[] values))
            {
                Items[key] = value;
                //Array.Resize(ref values, values.Length + value.Length);
                //int count = value.Length;
                //for (int i = 0; i < count; i++)
                //{
                //    values[values.Length - value.Length + i] = value[i];
                //}
            }
            else
            {
                Items.TryAdd(key, value);
            }
        }

        bool IDictionary<string, string[]>.Remove(string key)
        {
            return Items.Remove(key.ToLower());
        }

        public bool TryGetValue(string key, out string[] value)
        {
            return Items.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string[]> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(KeyValuePair<string, string[]> item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string[]> item)
        {
            return Items.Remove(item.Key.ToLower());
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
