using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Helpers
{
    public class ToLocalDateTimeConvertor : DateTimeConverterBase
    {
        public AutoLogger AutoLogger { get; set; }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.Value == null)
                    return default(DateTime);
                return ((DateTime)reader.Value).ToLocalTime();
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ToLocalDateTimeConvertor ReadJson");
                AutoLogger.LogText(reader.Value == null ? "null" : reader.Value.ToString());
            }
            return default(DateTime);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                if (value == null)
                    writer.WriteValue(default(DateTime).ToLocalTime());
                else
                {
                    var dt = ((DateTime)value).ToLocalTime();
                    writer.WriteValue(dt);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ToLocalDateTimeConvertor WriteJson");
                AutoLogger.LogText(value == null ? "null" : value.ToString());
            }

        }
    }

    public class JsonSettingHelper
    {
        public AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "JsonSettingHelper Logs.log" };
        public void Initialize()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var setting = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = new EventHandler<ErrorEventArgs>(HandleDeserializationError),
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                setting.Converters.Add(new ToLocalDateTimeConvertor() { AutoLogger = AutoLogger });
                return setting;
            };
        }

        public void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            if (errorArgs != null && errorArgs.ErrorContext != null && errorArgs.ErrorContext.Error != null && errorArgs.ErrorContext.Error.GetType() == typeof(JsonSerializationException))
            {
                AutoLogger.LogError(errorArgs.ErrorContext.Error, $"HandleDeserializationError :{(errorArgs.ErrorContext.OriginalObject == null ? "null" : errorArgs.ErrorContext.OriginalObject.GetType().FullName)} member: {errorArgs.ErrorContext.Member} path: {errorArgs.ErrorContext.Path}");
            }
            else
            {
                if (errorArgs != null)
                {
                    if (errorArgs.ErrorContext != null)
                    {
                        if (errorArgs.ErrorContext.Error != null)
                        {
                            AutoLogger.LogError(errorArgs.ErrorContext.Error, $"HandleDeserializationError2 :{(errorArgs.ErrorContext.OriginalObject == null ? "null" : errorArgs.ErrorContext.OriginalObject.GetType().FullName)} member: {errorArgs.ErrorContext.Member} path: {errorArgs.ErrorContext.Path}");
                        }
                        else
                        {
                            AutoLogger.LogText("json error ErrorContext.Error null");
                            AutoLogger.LogText("json error path: " + errorArgs.ErrorContext.Path);
                        }
                    }
                    else
                        AutoLogger.LogText("json error ErrorContext null");

                }
                else
                    AutoLogger.LogText("json error null");
            }
            errorArgs.ErrorContext.Handled = true;

        }

    }
}
