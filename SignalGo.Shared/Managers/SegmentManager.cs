using Newtonsoft.Json;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Managers
{
    public class SegmentManager
    {
        internal ConcurrentDictionary<string, List<ISegment>> Segments { get; set; } = new ConcurrentDictionary<string, List<ISegment>>();

        void AddToSegment(string guid, ISegment segment)
        {
            if (Segments.ContainsKey(guid))
            {
                Segments[guid].Add(segment);
            }
            else
            {
                Segments.TryAdd(guid, new List<ISegment>() { segment });
            }
        }
        /// <summary>
        /// generate segments, part number 0 = no any parts and part number -1 = end of parts
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public ISegment GenerateAndMixSegments(ISegment segment)
        {
            if (segment == null)
                throw new Exception("segment is null!");
            if (segment is MethodCallInfo)
            {
                var callInfo = (MethodCallInfo)segment;
                AddToSegment(callInfo.Guid, callInfo);
                if (segment.PartNumber == -1)
                {
                    StringBuilder data = new StringBuilder();
                    foreach (MethodCallInfo item in Segments[callInfo.Guid])
                    {
                        data.Append(item.Data.ToString());
                    }
                    Segments.Remove(callInfo.Guid);
                    return JsonConvert.DeserializeObject<MethodCallInfo>(data.ToString());
                }
                else
                    return null;
            }
            else if (segment is MethodCallbackInfo)
            {
                var callbackInfo = (MethodCallbackInfo)segment;
                AddToSegment(callbackInfo.Guid, callbackInfo);
                if (segment.PartNumber == -1)
                {
                    StringBuilder data = new StringBuilder();
                    foreach (MethodCallbackInfo item in Segments[callbackInfo.Guid])
                    {
                        data.Append(item.Data.ToString());
                    }
                    Segments.Remove(callbackInfo.Guid);
                    return JsonConvert.DeserializeObject<MethodCallbackInfo>(data.ToString());
                }
                else
                    return null;
            }
            else
                throw new Exception("segment not support: " + segment.ToString());
        }
    }
}
