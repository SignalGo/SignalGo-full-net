using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SignalGo.Shared.Log;
using System;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// convert date time to utc
    /// </summary>
    public class ToUniverseDateTimeConvertor : DateTimeConverterBase
    {
        public AutoLogger AutoLogger { get; set; }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.Value is DateTime dateTime)
                {
                    if (dateTime.Kind != DateTimeKind.Utc)
                        return dateTime.ToUniversalTime();
                    else
                        return dateTime;
                }
                return default(DateTime);
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ToUniverseDateTimeConvertor ReadJson");
                AutoLogger.LogText(reader.Value == null ? "null" : reader.Value.ToString());
            }
            return default(DateTime);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                if (value is DateTime dateTime)
                {
                    if (dateTime.Kind != DateTimeKind.Utc)
                        writer.WriteValue(dateTime.ToUniversalTime());
                    else
                        writer.WriteValue(dateTime);
                }
                else
                    writer.WriteValue(default(DateTime));
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "ToUniverseDateTimeConvertor WriteJson");
                AutoLogger.LogText(value == null ? "null" : value.ToString());
            }
        }
    }

    /// <summary>
    /// convert datetime to localtime
    /// </summary>
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
                    DateTime dt = ((DateTime)value).ToLocalTime();
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

    /// <summary>
    /// json serialize and deserialize error handling
    /// </summary>
    public class JsonSettingHelper
    {
        public DateTimeConverterBase CurrentDateTimeSetting { get; set; }
        /// <summary>
        /// log erros and warnings
        /// </summary>
        public AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "JsonSettingHelper Logs.log" };
        public void Initialize()
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings setting = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = new EventHandler<ErrorEventArgs>(HandleDeserializationError),
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                if (CurrentDateTimeSetting == null)
                    setting.Converters.Add(new ToLocalDateTimeConvertor() { AutoLogger = AutoLogger });
                else
                    setting.Converters.Add(CurrentDateTimeSetting);
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
