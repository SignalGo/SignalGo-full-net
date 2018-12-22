using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Server.TelegramBot.DataTypes;
using SignalGo.Server.TelegramBot.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace SignalGo.Server.TelegramBot
{
    public class SignalGoBotManager
    {
        private IBotStructureInfo CurrentBotStructureInfo { get; set; }
        private TelegramBotClient _botClient;
        private ServerBase _serverBase;

        private Dictionary<string, Type> Services { get; set; } = new Dictionary<string, Type>();
        private Dictionary<Type, Dictionary<string, Delegate>> OverridedMethodResponses { get; set; } = new Dictionary<Type, Dictionary<string, Delegate>>();
        //private List<List<KeyboardButton>> ServicesButtons { get; set; }
        private ConcurrentDictionary<int, TelegramClientInfo> ConnectedClients { get; set; } = new ConcurrentDictionary<int, TelegramClientInfo>();
        public async Task Start(string token, ServerBase serverBase, IBotStructureInfo botStructureInfo = null, System.Net.Http.HttpClient httpClient = null)
        {
            if (botStructureInfo == null)
                CurrentBotStructureInfo = new BotStructureInfo();
            else
                CurrentBotStructureInfo = botStructureInfo;
            _serverBase = serverBase;
            _botClient = new TelegramBotClient(token, httpClient);

            User me = await _botClient.GetMeAsync();

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();
            CurrentBotStructureInfo.OnStarted(this);
        }

        public void Stop()
        {
            _botClient.StopReceiving();
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Text != null)
                {
                    if (!ConnectedClients.TryGetValue(e.Message.From.Id, out TelegramClientInfo clientInfo))
                    {
                        clientInfo = new TelegramClientInfo(_serverBase)
                        {
                            ConnectedDateTime = DateTime.Now,
                            ClientId = Guid.NewGuid().ToString(),
                            Message = e.Message
                        };
                        _serverBase.Clients.TryAdd(clientInfo.ClientId, clientInfo);
                        ConnectedClients.TryAdd(e.Message.From.Id, clientInfo);
                    }

                    if (e.Message.Text == CurrentBotStructureInfo.GetCancelButtonText(clientInfo))
                    {
                        if (!string.IsNullOrEmpty(clientInfo.CurrentMethodName) && !string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                        {
                            await ShowServiceMethods(clientInfo.CurrentServiceName, clientInfo, e);
                        }
                        else
                        {
                            await ShowServices(clientInfo, e);
                        }
                    }
                    else if (e.Message.Text == CurrentBotStructureInfo.GetSendButtonText(clientInfo) && !string.IsNullOrEmpty(clientInfo.CurrentServiceName) && !string.IsNullOrEmpty(clientInfo.CurrentMethodName))
                    {
                        if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
                        {
                            if (CurrentBotStructureInfo.OnBeforeMethodCall(_serverBase, clientInfo, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo.ParameterInfoes))
                            {
                                Shared.Models.CallMethodResultInfo<OperationContext> result = await BaseProvider.CallMethod(clientInfo.CurrentServiceName, Guid.NewGuid().ToString(), clientInfo.CurrentMethodName, clientInfo.ParameterInfoes.ToArray()
                                , null, clientInfo, null, _serverBase, null, x => true);
                                MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentMethodName, StringComparison.OrdinalIgnoreCase));
                                if (OverridedMethodResponses.TryGetValue(service, out Dictionary<string, Delegate> methods) && methods.TryGetValue(clientInfo.CurrentMethodName, out Delegate function))
                                {
                                    BotCustomResponse botCustomResponse = new BotCustomResponse();
                                    BotResponseInfoBase response = (BotResponseInfoBase)function.DynamicInvoke(result.Context, botCustomResponse, result.Result);
                                    await ShowResultValue(response.Message, method, clientInfo, e);
                                    botCustomResponse.OnAfterComeplete?.Invoke();
                                }
                                else
                                {

                                    string customResponse = CurrentBotStructureInfo.OnCustomResponse(_serverBase, clientInfo, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo.ParameterInfoes, result, out bool responseChanged);
                                    if (responseChanged)
                                        await ShowResultValue(customResponse, method, clientInfo, e);
                                    else
                                        await ShowResultValue(result, method, clientInfo, e);
                                }
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                    {
                        string serviceName = GetServiceNameByCaption(e.Message.Text);
                        if (Services.ContainsKey(serviceName))
                        {
                            await ShowServiceMethods(serviceName, clientInfo, e);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(clientInfo.CurrentMethodName) && !string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                            {
                                await ShowServiceMethods(clientInfo.CurrentServiceName, clientInfo, e);
                            }
                            else if (string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                            {
                                await ShowServices(clientInfo, e);
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(clientInfo.CurrentMethodName))
                    {
                        if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
                        {
                            //MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(e.Message.Text, StringComparison.OrdinalIgnoreCase));
                            MethodInfo method = GetMethodByCaption(service, e.Message.Text);
                            if (method != null)
                            {
                                await ShowServiceMethods(method, clientInfo, e);
                            }
                            else
                            {
                                await ShowServiceMethods(clientInfo.CurrentServiceName, clientInfo, e);
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(clientInfo.CurrentParameterName))
                    {
                        if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
                        {
                            MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentMethodName, StringComparison.OrdinalIgnoreCase));
                            //MethodInfo method = FindMethodByName(service, clientInfo.CurrentMethodName);
                            //ParameterInfo parameter = method.GetParameters().FirstOrDefault(x => x.Name.Equals(e.Message.Text, StringComparison.OrdinalIgnoreCase));
                            ParameterInfo parameter = FindParameterByName(method, e.Message.Text, true);
                            await GetParameterValueFromClient(method, parameter, clientInfo, e);
                        }
                    }
                    else
                    {
                        if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
                        {
                            MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentMethodName, StringComparison.OrdinalIgnoreCase));
                            //MethodInfo method = FindMethodByName(service, clientInfo.CurrentMethodName);
                            // ParameterInfo parameter = method.GetParameters().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentParameterName, StringComparison.OrdinalIgnoreCase));
                            ParameterInfo parameter = FindParameterByName(method, clientInfo.CurrentParameterName, false);
                            await SetParameterValueFromClient(method, parameter, clientInfo, e);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task ShowResultValue(Shared.Models.CallMethodResultInfo<OperationContext> callMethodResultInfo, MethodInfo methodInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            clientInfo.CurrentParameterName = null;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetMethodParametersButtons(methodInfo, clientInfo)
            };
            if (callMethodResultInfo.CallbackInfo.Data.Length > 4000)
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(callMethodResultInfo.CallbackInfo.Data)))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream);
                    inputOnlineFile.FileName = "reponse.txt";
                    await _botClient.SendDocumentAsync(
                        chatId: e.Message.Chat,
                        document: inputOnlineFile,
                        caption: $"Response of {methodInfo.Name}",
                        replyMarkup: replyMarkup
                       );
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                  chatId: e.Message.Chat,
                  text: callMethodResultInfo.CallbackInfo.Data,
                  replyMarkup: replyMarkup
                 );
            }
        }

        private async Task ShowResultValue(string response, MethodInfo methodInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(response))
                return;
            clientInfo.CurrentParameterName = null;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetMethodParametersButtons(methodInfo, clientInfo)
            };
            await _botClient.SendTextMessageAsync(
              chatId: e.Message.Chat,
              text: response,
              replyMarkup: replyMarkup
             );
        }

        private async Task SetParameterValueFromClient(MethodInfo methodInfo, ParameterInfo parameterInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            string value = e.Message.Text;
            clientInfo.CurrentParameterName = null;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetMethodParametersButtons(methodInfo, clientInfo)
            };
            Shared.Models.ParameterInfo find = clientInfo.ParameterInfoes.FirstOrDefault(x => x.Name.Equals(parameterInfo.Name, StringComparison.OrdinalIgnoreCase));
            if (find == null)
            {
                find = new Shared.Models.ParameterInfo() { Name = parameterInfo.Name };
                clientInfo.ParameterInfoes.Add(find);
            }
            find.Value = value;
            await _botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: CurrentBotStructureInfo.GetParameterValueChangedText(GetParameterCaption(methodInfo, parameterInfo), clientInfo),
                replyMarkup: replyMarkup
               );
        }

        private async Task GetParameterValueFromClient(MethodInfo methodInfo, ParameterInfo parameterInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            if (parameterInfo == null)
            {
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = GetMethodParametersButtons(methodInfo, clientInfo)
                };
                await _botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: CurrentBotStructureInfo.GetParameterNotFoundText(e.Message.Text, clientInfo),
                    replyMarkup: replyMarkup
                   );
            }
            else
            {
                clientInfo.CurrentParameterName = parameterInfo.Name;// GetParameterCaption(methodInfo, parameterInfo);
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = GetMethodParametersButtons(methodInfo, clientInfo)
                };
                await _botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: CurrentBotStructureInfo.GetParameterSelectedText(GetParameterCaption(methodInfo, parameterInfo), clientInfo),
                    replyMarkup: replyMarkup
                   );
            }
        }

        private async Task ShowServiceMethods(MethodInfo method, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            clientInfo.CurrentMethodName = method.Name;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetMethodParametersButtons(method, clientInfo)
            };
            await _botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: CurrentBotStructureInfo.GetMethodSelectedText(e.Message.Text, clientInfo),
                replyMarkup: replyMarkup
               );
        }

        private async Task ShowServiceMethods(string serviceName, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            clientInfo.CurrentServiceName = serviceName;
            clientInfo.CurrentMethodName = null;
            if (Services.TryGetValue(serviceName, out Type service))
            {
                //clientInfo.CurrentServiceName = GetServiceCaption(service);
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = GetServiceMethodsButtons(service, clientInfo)
                };
                await _botClient.SendTextMessageAsync(
                     chatId: e.Message.Chat,
                     text: CurrentBotStructureInfo.GetServiceSelectedText(GetServiceCaption(service), clientInfo),
                     replyMarkup: replyMarkup
                   );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: CurrentBotStructureInfo.GetServiceNotFoundText(serviceName, clientInfo)
                  );
            }
        }

        private async Task ShowServices(TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            clientInfo.CurrentServiceName = null;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetListOfServicesButtons(clientInfo)
            };
            await _botClient.SendTextMessageAsync(
                 chatId: e.Message.Chat,
                 text: CurrentBotStructureInfo.GetServicesGeneratedText(clientInfo),
                 replyMarkup: replyMarkup
               );
        }

        public async void ShowServices(TelegramClientInfo clientInfo)
        {
            try
            {
                clientInfo.CurrentServiceName = null;
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = GetListOfServicesButtons(clientInfo)
                };
                await _botClient.SendTextMessageAsync(
                     chatId: clientInfo.Message.Chat,
                     text: CurrentBotStructureInfo.GetServicesGeneratedText(clientInfo),
                     replyMarkup: replyMarkup
                   );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// get buttons of all services
        /// </summary>
        /// <returns></returns>
        private List<List<KeyboardButton>> GetListOfServicesButtons(TelegramClientInfo clientInfo)
        {
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();

            int columnIndex = 0;
            List<System.Type> services = _serverBase.GetListOfRegistredTypes().ToList();
            for (int i = 0; i < services.Count; i++)
            {
                System.Type item = services[i];
                ServiceContractAttribute attribute = item.GetCustomAttribute<ServiceContractAttribute>();
                if (attribute.ServiceType != ServiceType.HttpService)
                    continue;
                string serviceName = "";
                if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
                {
                    BotDisplayNameAttribute nameAttribute = item.GetCustomAttribute<BotDisplayNameAttribute>();
                    if (nameAttribute == null)
                        continue;
                    //serviceName = nameAttribute.Content;
                }

                serviceName = attribute.Name;
                if (!CurrentBotStructureInfo.OnServiceGenerating(serviceName, clientInfo))
                    continue;
                if (!Services.ContainsKey(attribute.Name))
                    Services.Add(serviceName, item);
                if (columnIndex == 3)
                {
                    columnIndex = 0;
                    rows.Add(columns.ToList());
                    columns.Clear();
                }
                columns.Add(GetServiceCaption(item));
                columnIndex++;
            }
            if (rows.Count == 0)
                rows.Add(columns);
            return rows;
        }

        /// <summary>
        /// get buttons of service
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private List<List<KeyboardButton>> GetServiceMethodsButtons(Type service, TelegramClientInfo clientInfo)
        {
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();

            int columnIndex = 0;

            List<MethodInfo> methods = service.GetFullServiceLevelMethods().ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                MethodInfo item = methods[i];
                string methodName = "";
                if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
                {
                    BotDisplayNameAttribute nameAttribute = item.GetCustomAttribute<BotDisplayNameAttribute>();
                    if (nameAttribute == null)
                        continue;
                    methodName = nameAttribute.Content;
                }
                else
                    methodName = item.Name;
                //create new row after 2 columns
                if (columnIndex == 2)
                {
                    columnIndex = 0;
                    rows.Add(columns.ToList());
                    columns.Clear();
                }
                columns.Add(methodName);
                columnIndex++;
            }
            if (rows.Count == 0)
                rows.Add(columns);
            rows.Add(new List<KeyboardButton>() { new KeyboardButton(CurrentBotStructureInfo.GetCancelButtonText(clientInfo)) });
            return rows;
        }

        private string GetMethodCaption(MethodInfo method)
        {
            if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
            {
                BotDisplayNameAttribute nameAttribute = method.GetCustomAttribute<BotDisplayNameAttribute>();
                if (nameAttribute == null)
                    return "null";
                return nameAttribute.Content;
            }
            else
                return method.Name;
        }

        private MethodInfo GetMethodByCaption(Type service, string caption)
        {
            foreach (MethodInfo method in service.GetFullServiceLevelMethods())
            {
                string name = GetMethodCaption(method);
                if (caption.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return method;
            }
            return null;
        }

        private string GetParameterCaption(MethodInfo method, ParameterInfo parameter)
        {
            BotDisplayNameAttribute nameAttribute = method.GetCustomAttribute<BotDisplayNameAttribute>();
            string parameterName = "";
            if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
            {
                if (nameAttribute != null)
                {
                    parameterName = nameAttribute.FindValue(parameter.Name);
                    //for take parameter attribute
                    if (parameterName == null)
                        nameAttribute = null;
                }
                //take attribute of parameter
                if (nameAttribute == null)
                    nameAttribute = parameter.GetCustomAttribute<BotDisplayNameAttribute>();
                if (nameAttribute == null)
                {
                    return "null";
                }
                //if parametername not found
                if (string.IsNullOrEmpty(parameterName))
                    parameterName = nameAttribute.Content;
            }
            else
                parameterName = parameter.Name;
            return parameterName;
        }

        private string GetServiceCaption(Type service)
        {
            ServiceContractAttribute serviceAttribute = service.GetCustomAttribute<ServiceContractAttribute>();
            if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
            {
                BotDisplayNameAttribute nameAttribute = service.GetCustomAttribute<BotDisplayNameAttribute>();
                if (nameAttribute == null)
                    return serviceAttribute.Name;
                return nameAttribute.Content;
            }
            else
            {
                return serviceAttribute.Name;
            }
        }

        private string GetServiceNameByCaption(string caption)
        {
            if (!CurrentBotStructureInfo.InitializeServicesFromAttributes)
            {
                if (Services.TryGetValue(caption, out Type service))
                {
                    ServiceContractAttribute serviceAttribute = service.GetCustomAttribute<ServiceContractAttribute>();
                    return serviceAttribute.Name;
                }
                else
                    return caption;
            }

            foreach (Type service in Services.Values)
            {
                ServiceContractAttribute serviceAttribute = service.GetCustomAttribute<ServiceContractAttribute>();
                BotDisplayNameAttribute nameAttribute = service.GetCustomAttribute<BotDisplayNameAttribute>();
                if (nameAttribute == null)
                    continue;
                else if (nameAttribute.Content.Equals(caption, StringComparison.OrdinalIgnoreCase))
                    return serviceAttribute.Name;
            }
            return caption;
        }

        private MethodInfo FindMethodByCaption(Type service, string caption)
        {
            foreach (MethodInfo item in service.GetFullServiceLevelMethods())
            {
                if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
                {
                    BotDisplayNameAttribute nameAttribute = item.GetCustomAttribute<BotDisplayNameAttribute>();
                    if (nameAttribute == null)
                        continue;
                    if (nameAttribute.Content.Equals(caption, StringComparison.OrdinalIgnoreCase))
                        return item;
                }
                else
                {
                    if (item.Name.Equals(caption, StringComparison.OrdinalIgnoreCase))
                        return item;
                }
            }
            return null;
        }

        private ParameterInfo FindParameterByName(MethodInfo methodInfo, string name, bool nameIsCaption)
        {
            BotDisplayNameAttribute nameAttribute = methodInfo.GetCustomAttribute<BotDisplayNameAttribute>();
            foreach (ParameterInfo item in methodInfo.GetParameters())
            {
                if (!nameIsCaption)
                {
                    if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return item;
                }
                else
                {
                    string parameterName = "";
                    if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
                    {
                        if (nameAttribute != null)
                        {
                            parameterName = nameAttribute.FindValue(item.Name);
                            //for take parameter attribute
                            if (parameterName == null)
                                nameAttribute = null;
                        }
                        //take attribute of parameter
                        if (nameAttribute == null)
                            nameAttribute = item.GetCustomAttribute<BotDisplayNameAttribute>();
                        if (nameAttribute == null)
                        {
                            continue;
                        }
                        //if parametername not found
                        if (string.IsNullOrEmpty(parameterName))
                            parameterName = nameAttribute.Content;
                    }
                    else
                        parameterName = item.Name;
                    if (parameterName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return item;
                }
            }
            return null;
        }

        private List<List<KeyboardButton>> GetMethodParametersButtons(MethodInfo method, TelegramClientInfo clientInfo)
        {
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();

            int columnIndex = 0;

            List<ParameterInfo> parameters = method.GetParameters().ToList();
            BotDisplayNameAttribute nameAttribute = method.GetCustomAttribute<BotDisplayNameAttribute>();
            for (int i = 0; i < parameters.Count; i++)
            {
                ParameterInfo item = parameters[i];
                string parameterName = "";
                if (CurrentBotStructureInfo.InitializeServicesFromAttributes)
                {
                    if (nameAttribute != null)
                    {
                        parameterName = nameAttribute.FindValue(item.Name);
                        //for take parameter attribute
                        if (parameterName == null)
                            nameAttribute = null;
                    }
                    //take attribute of parameter
                    if (nameAttribute == null)
                        nameAttribute = item.GetCustomAttribute<BotDisplayNameAttribute>();
                    if (nameAttribute == null)
                    {
                        continue;
                    }
                    //if parametername not found
                    if (string.IsNullOrEmpty(parameterName))
                        parameterName = nameAttribute.Content;
                }
                else
                    parameterName = item.Name;
                if (columnIndex == 2)
                {
                    columnIndex = 0;
                    rows.Add(columns.ToList());
                    columns.Clear();
                }
                columns.Add(parameterName);
                columnIndex++;
            }
            if (rows.Count == 0)
                rows.Add(columns);
            rows.Add(new List<KeyboardButton>() { new KeyboardButton(CurrentBotStructureInfo.GetSendButtonText(clientInfo)) });
            rows.Add(new List<KeyboardButton>() { new KeyboardButton(CurrentBotStructureInfo.GetCancelButtonText(clientInfo)) });
            return rows;
        }

        /// <summary>
        /// override response of service method
        /// </summary>
        public void OverrideServiceMethodResponse<T, TMethodResponse, TResult>(string methodName, Func<OperationContext, BotCustomResponse, TMethodResponse, BotResponseInfo<TResult>> customizeResponse)
        {
            Type serviceType = typeof(T);

            if (!OverridedMethodResponses.TryGetValue(serviceType, out Dictionary<string, Delegate> responses))
                responses = OverridedMethodResponses[serviceType] = new Dictionary<string, Delegate>();
            if (responses.ContainsKey(methodName))
                throw new Exception($"method {methodName} is exist and you ar adding duplicate");
            responses.Add(methodName, customizeResponse);
        }

        public void OverrideServiceMethodResponse<T, TMethodResponse, TResult>(Func<OperationContext, BotCustomResponse, TMethodResponse, BotResponseInfo<TResult>> customizeResponse, params string[] methods)
        {
            Type serviceType = typeof(T);

            if (!OverridedMethodResponses.TryGetValue(serviceType, out Dictionary<string, Delegate> responses))
                responses = OverridedMethodResponses[serviceType] = new Dictionary<string, Delegate>();

            foreach (string methodName in methods)
            {
                if (responses.ContainsKey(methodName))
                    throw new Exception($"method {methodName} is exist and you ar adding duplicate");
                responses.Add(methodName, customizeResponse);
            }
        }
    }
}
