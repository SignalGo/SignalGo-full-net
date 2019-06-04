using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Http;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalGo.Shared.Models
{
    public class CallMethodResultInfo<T>
    {
        public CallMethodResultInfo(MethodCallbackInfo callbackInfo, IStreamInfo streamInfo, List<HttpKeyAttribute> httpKeyAttributees, Type serviceType, MethodInfo method, object serviceInstance, FileActionResult fileActionResult, T context, object result)
        {
            CallbackInfo = callbackInfo;
            StreamInfo = streamInfo;
            HttpKeyAttributees = httpKeyAttributees;
            ServiceType = serviceType;
            Method = method;
            ServiceInstance = serviceInstance;
            FileActionResult = fileActionResult;
            Context = context;
            Result = result;
        }

        public MethodCallbackInfo CallbackInfo { get; set; }
        public IStreamInfo StreamInfo { get; set; }
        public List<HttpKeyAttribute> HttpKeyAttributees { get; set; }
        public Type ServiceType { get; set; }
        public MethodInfo Method { get; set; }
        public object ServiceInstance { get; set; }
        public FileActionResult FileActionResult { get; set; }
        public T Context { get; set; }
        public object Result { get; set; }
    }
}
