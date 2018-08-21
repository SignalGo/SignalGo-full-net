using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Http
{
    public class WebHeaderCollection
    {
        private ConcurrentDictionary<string, KeyValuePair<string, string>> Items { get; set; } = new ConcurrentDictionary<string, KeyValuePair<string, string>>();
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
        public string this[string name]
        {
            get
            {
                name = name.ToLower();
                Items.TryGetValue(name, out KeyValuePair<string, string> value);
                return value.Value;
            }
            set
            {
                Items.AddOrUpdate(name.ToLower(), new KeyValuePair<string, string>(name, value), (x, old) => new KeyValuePair<string, string>(name, value));
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

        public void Add(string header, string value)
        {
            Items.AddOrUpdate(header.ToLower(), new KeyValuePair<string, string>(header, value), (x, old) => new KeyValuePair<string, string>(header, value));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, KeyValuePair<string, string>> item in Items)
            {
                builder.AppendLine(item.Value.Key + ": " + item.Value.Value);
            }
            builder.AppendLine();
            return builder.ToString();
        }
    }
}
