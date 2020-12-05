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

        /// <summary>
        /// custom inline buttons callbacks
        /// </summary>
        private Dictionary<string, Action<TelegramClientInfo>> CustomInlineButtons { get; set; } = new Dictionary<string, Action<TelegramClientInfo>>();

        private Dictionary<string, Type> Services { get; set; } = new Dictionary<string, Type>();
        private Dictionary<Type, Dictionary<string, Delegate>> OverridedMethodResponses { get; set; } = new Dictionary<Type, Dictionary<string, Delegate>>();
        //private List<List<KeyboardButton>> ServicesButtons { get; set; }
        private ConcurrentDictionary<int, TelegramClientInfo> ConnectedClients { get; set; } = new ConcurrentDictionary<int, TelegramClientInfo>();

        /// <summary>
        /// current generated buttons for user and user can click on them
        /// </summary>
        public ConcurrentDictionary<TelegramClientInfo, Dictionary<string, BotButtonInfo>> UsersBotButtons { get; set; } = new ConcurrentDictionary<TelegramClientInfo, Dictionary<string, BotButtonInfo>>();

        public async Task Start(string token, ServerBase serverBase, IBotStructureInfo botStructureInfo = null, System.Net.Http.HttpClient httpClient = null)
        {
            if (botStructureInfo == null)
                CurrentBotStructureInfo = new BotStructureInfo();
            else
                CurrentBotStructureInfo = botStructureInfo;
            _serverBase = serverBase;
            _botClient = new TelegramBotClient(token, httpClient);

            User me = await _botClient.GetMeAsync();
            _botClient.OnCallbackQuery += _botClient_OnCallbackQuery;
            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();
            CurrentBotStructureInfo.OnStarted(this);
        }

        private void _botClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                if (!ConnectedClients.TryGetValue(e.CallbackQuery.Message.From.Id, out TelegramClientInfo clientInfo))
                {
                    clientInfo = new TelegramClientInfo(_serverBase)
                    {
                        ConnectedDateTime = DateTime.Now,
                        ClientId = Guid.NewGuid().ToString(),
                        Message = e.CallbackQuery.Message,
                        SignalGoBotManager = this
                    };
                    _serverBase.Clients.TryAdd(clientInfo.ClientId, clientInfo);
                    ConnectedClients.TryAdd(e.CallbackQuery.Message.From.Id, clientInfo);
                    CurrentBotStructureInfo.OnClientConnected(clientInfo, this);
                }

                if (CustomInlineButtons.TryGetValue(e.CallbackQuery.Data, out Action<TelegramClientInfo> action))
                {
                    action?.Invoke(clientInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
                            Message = e.Message,
                            SignalGoBotManager = this
                        };
                        _serverBase.Clients.TryAdd(clientInfo.ClientId, clientInfo);
                        ConnectedClients.TryAdd(e.Message.From.Id, clientInfo);
                        CurrentBotStructureInfo.OnClientConnected(clientInfo, this);
                    }
                    if (Services.Count == 0)
                        GetListOfServicesButtons(clientInfo);
                    BotButtonInfo buttonInfo = null;
                    if (UsersBotButtons.TryGetValue(clientInfo, out Dictionary<string, BotButtonInfo> buttons))
                    {
                        if (buttons.TryGetValue(e.Message.Text, out buttonInfo))
                        {
                            if (buttonInfo.ServiceName != null)
                                clientInfo.CurrentServiceName = buttonInfo.ServiceName;
                            if (buttonInfo.MethodName != null)
                                clientInfo.CurrentMethodName = buttonInfo.MethodName;
                        }
                    }
                    if (buttonInfo != null && buttonInfo.Click != null)
                    {
                        buttonInfo.Click(clientInfo);
                    }
                    else if (e.Message.Text == CurrentBotStructureInfo.GetCancelButtonText(clientInfo))
                    {
                        clientInfo.ParameterInfoes.Clear();
                        if (!string.IsNullOrEmpty(clientInfo.CurrentMethodName) && !string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                        {
                            await ShowServiceMethods(clientInfo.CurrentServiceName, clientInfo, e.Message);
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
                                Shared.Models.CallMethodResultInfo<OperationContext> result = await CallMethod(clientInfo);
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
                                        await ShowResultValue(result, method, clientInfo, e.Message);
                                }
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                    {
                        string serviceName = GetServiceNameByCaption(e.Message.Text);
                        if (Services.ContainsKey(serviceName))
                        {
                            await ShowServiceMethods(serviceName, clientInfo, e.Message);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(clientInfo.CurrentMethodName) && !string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                            {
                                await ShowServiceMethods(clientInfo.CurrentServiceName, clientInfo, e.Message);
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
                                await ShowServiceMethods(method, clientInfo, e.Message);
                            }
                            else
                            {
                                await ShowServiceMethods(clientInfo.CurrentServiceName, clientInfo, e.Message);
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(clientInfo.CurrentParameterName))
                    {
                        if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
                        {
                            MethodInfo method = FindMethod(service, clientInfo.CurrentMethodName);
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
                            MethodInfo method = FindMethod(service, clientInfo.CurrentMethodName);
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

        private async Task<Shared.Models.CallMethodResultInfo<OperationContext>> CallMethod(TelegramClientInfo clientInfo)
        {
            Shared.Models.CallMethodResultInfo<OperationContext> result = await BaseProvider.CallMethod(clientInfo.CurrentServiceName, Guid.NewGuid().ToString(), clientInfo.CurrentMethodName, clientInfo.CurrentMethodName, clientInfo.ParameterInfoes.ToArray()
                               , null, clientInfo, null, _serverBase, null, x => true);
            return result;
        }

        public async Task<T> CallServerMethod<T>(TelegramClientInfo clientInfo)
        {
            Shared.Models.CallMethodResultInfo<OperationContext> result = await CallMethod(clientInfo);

            if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
            {
                MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentMethodName, StringComparison.OrdinalIgnoreCase));
                if (OverridedMethodResponses.TryGetValue(service, out Dictionary<string, Delegate> methods) && methods.TryGetValue(clientInfo.CurrentMethodName, out Delegate function))
                {
                    BotCustomResponse botCustomResponse = new BotCustomResponse();
                    BotResponseInfoBase response = (BotResponseInfoBase)function.DynamicInvoke(result.Context, botCustomResponse, result.Result);
                    //await ShowResultValue(response.Message, method, clientInfo, clientInfo.Message);
                    botCustomResponse.OnAfterComeplete?.Invoke();
                }
                //else
                //{
                //    string customResponse = CurrentBotStructureInfo.OnCustomResponse(_serverBase, clientInfo, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo.ParameterInfoes, result, out bool responseChanged);
                //    //if (responseChanged)
                //    //    await ShowResultValue(customResponse, method, clientInfo, message);
                //    //else
                //    //    await ShowResultValue(result, method, clientInfo, e);
                //}
                return (T)result.Result;
            }
            return default(T);
        }

        private async Task ShowResultValue(Shared.Models.CallMethodResultInfo<OperationContext> callMethodResultInfo, MethodInfo methodInfo, TelegramClientInfo clientInfo, Message message)
        {
            clientInfo.CurrentParameterName = null;
            List<List<BotButtonInfo>> buttons = GetMethodParametersButtons(methodInfo, clientInfo);
            CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Parameters, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo);
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
            };
            if (callMethodResultInfo.CallbackInfo.Data.Length > 4000)
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(callMethodResultInfo.CallbackInfo.Data)))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream);
                    inputOnlineFile.FileName = "reponse.txt";
                    await _botClient.SendDocumentAsync(
                        chatId: message.Chat,
                        document: inputOnlineFile,
                        caption: $"Response of {methodInfo.Name}",
                        replyMarkup: replyMarkup
                       );
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                  chatId: message.Chat,
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
            List<List<BotButtonInfo>> buttons = GetMethodParametersButtons(methodInfo, clientInfo);
            CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Parameters, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo);
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
            };
            await _botClient.SendTextMessageAsync(
              chatId: e.Message.Chat,
              text: response,
              replyMarkup: replyMarkup
             );
        }

        public void ChangeParameterValue(MethodInfo methodInfo, ParameterInfo parameterInfo, TelegramClientInfo clientInfo, string value)
        {
            Shared.Models.ParameterInfo find = clientInfo.ParameterInfoes.FirstOrDefault(x => x.Name.Equals(parameterInfo.Name, StringComparison.OrdinalIgnoreCase));
            if (find == null)
            {
                find = new Shared.Models.ParameterInfo() { Name = parameterInfo.Name };
                clientInfo.ParameterInfoes.Add(find);
            }
            find.Value = value;
        }

        public void ChangeParameterValue(TelegramClientInfo clientInfo, string parameterName, string value)
        {
            Shared.Models.ParameterInfo find = clientInfo.ParameterInfoes.FirstOrDefault(x => x.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
            if (find == null)
            {
                find = new Shared.Models.ParameterInfo() { Name = parameterName };
                clientInfo.ParameterInfoes.Add(find);
            }
            find.Value = value;
        }

        private async Task SetParameterValueFromClient(MethodInfo methodInfo, ParameterInfo parameterInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            string value = e.Message.Text;
            clientInfo.CurrentParameterName = null;
            List<List<BotButtonInfo>> buttons = GetMethodParametersButtons(methodInfo, clientInfo);
            CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Parameters, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo);
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
            };
            ChangeParameterValue(methodInfo, parameterInfo, clientInfo, value);
            await _botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: CurrentBotStructureInfo.GetParameterValueChangedText(GetParameterCaption(methodInfo, parameterInfo), clientInfo),
                replyMarkup: replyMarkup
               );
        }

        private async Task GetParameterValueFromClient(MethodInfo methodInfo, ParameterInfo parameterInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            if (!CurrentBotStructureInfo.OnParameterSelecting(methodInfo, parameterInfo, clientInfo, e.Message.Text))
            {
                if (parameterInfo == null)
                {
                    List<List<BotButtonInfo>> buttons = GetMethodParametersButtons(methodInfo, clientInfo);
                    CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Parameters, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo);
                    ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                    {
                        Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
                    };
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: CurrentBotStructureInfo.GetParameterNotFoundText(e.Message.Text, clientInfo),
                        replyMarkup: replyMarkup
                       );
                }
                else
                {
                    clientInfo.CurrentParameterName = parameterInfo.Name;
                    List<List<BotButtonInfo>> buttons = GetMethodParametersButtons(methodInfo, clientInfo);
                    CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Parameters, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo);
                    ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                    {
                        Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
                    };
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: CurrentBotStructureInfo.GetParameterSelectedText(GetParameterCaption(methodInfo, parameterInfo), clientInfo),
                        replyMarkup: replyMarkup
                       );
                }
            }
        }

        public async void SendText(string text, TelegramClientInfo clientInfo, List<List<BotButtonInfo>> botButtons)
        {
            try
            {
                clientInfo.CurrentServiceName = null;
                CurrentBotStructureInfo.OnButtonsGenerating(botButtons, BotLevelType.Services, null, null, clientInfo);
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = BotButtonsToKeyboardButtons(botButtons, clientInfo)
                };
                await _botClient.SendTextMessageAsync(
                     chatId: clientInfo.Message.Chat,
                     text: text,
                     replyMarkup: replyMarkup
                   );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task ShowServiceMethods(MethodInfo method, TelegramClientInfo clientInfo, Message message, string customText = null)
        {
            clientInfo.CurrentMethodName = method.Name;
            List<List<BotButtonInfo>> buttons = GetMethodParametersButtons(method, clientInfo);
            CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Parameters, clientInfo.CurrentServiceName, clientInfo.CurrentMethodName, clientInfo);
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
            };
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: string.IsNullOrEmpty(customText) ? CurrentBotStructureInfo.GetMethodSelectedText(method.Name, clientInfo) : customText,
                replyMarkup: replyMarkup
               );
        }

        private async Task ShowServiceMethods(string serviceName, TelegramClientInfo clientInfo, Message message)
        {
            clientInfo.CurrentServiceName = serviceName;
            clientInfo.CurrentMethodName = null;
            if (Services.TryGetValue(serviceName, out Type service))
            {
                List<List<BotButtonInfo>> buttons = GetServiceMethodsButtons(service, clientInfo);
                CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Services, serviceName, null, clientInfo);
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
                };
                await _botClient.SendTextMessageAsync(
                     chatId: message.Chat,
                     text: CurrentBotStructureInfo.GetServiceSelectedText(GetServiceName(service), GetServiceCaption(service), service, clientInfo),
                     replyMarkup: replyMarkup
                   );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: CurrentBotStructureInfo.GetServiceNotFoundText(serviceName, clientInfo)
                  );
            }
        }

        private async Task ShowServices(TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            string text = CurrentBotStructureInfo.GetServicesGeneratedText(clientInfo);
            clientInfo.CurrentServiceName = null;
            List<List<BotButtonInfo>> buttons = GetListOfServicesButtons(clientInfo);

            CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Services, null, null, clientInfo);
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
            };
            await _botClient.SendTextMessageAsync(
                 chatId: e.Message.Chat,
                 text: text,
                 replyMarkup: replyMarkup
               );
        }

        public async void ShowServices(TelegramClientInfo clientInfo, string message = null)
        {
            try
            {
                clientInfo.CurrentServiceName = null;
                List<List<BotButtonInfo>> buttons = GetListOfServicesButtons(clientInfo);
                CurrentBotStructureInfo.OnButtonsGenerating(buttons, BotLevelType.Services, null, null, clientInfo);
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = BotButtonsToKeyboardButtons(buttons, clientInfo)
                };
                await _botClient.SendTextMessageAsync(
                    chatId: clientInfo.Message.Chat,
                    text: message == null ? CurrentBotStructureInfo.GetServicesGeneratedText(clientInfo) : message,
                    replyMarkup: replyMarkup
                  );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async void ShowServices(TelegramClientInfo clientInfo, List<List<BotButtonInfo>> inlineButtonInfos, string message = null)
        {
            try
            {
                clientInfo.CurrentServiceName = null;
                InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(BotButtonsToKeyboardButtons(inlineButtonInfos));
                await _botClient.SendTextMessageAsync(
                    chatId: clientInfo.Message.Chat,
                    text: message == null ? CurrentBotStructureInfo.GetServicesGeneratedText(clientInfo) : message,
                    replyMarkup: replyMarkup
                  );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        /// <summary>
        /// show service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="clientInfo"></param>
        public async void ShowService(string serviceName, TelegramClientInfo clientInfo)
        {
            try
            {
                await ShowServiceMethods(serviceName, clientInfo, clientInfo.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public MethodInfo FindMethod(Type service, string name)
        {
            MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (method == null)
                return FindMethodByCaption(service, name);
            return method;
        }

        public async void ShowMethod(string serviceName, string methodName, TelegramClientInfo clientInfo, bool clearParameters, string customText = null)
        {
            try
            {
                if (clearParameters)
                    clientInfo.ParameterInfoes.Clear();
                if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(methodName))
                {
                    if (!string.IsNullOrEmpty(customText))
                        SendText(customText, clientInfo, GetListOfServicesButtons(clientInfo));
                    return;
                }
                if (Services.TryGetValue(serviceName, out Type service))
                {
                    clientInfo.CurrentServiceName = serviceName;
                    MethodInfo method = FindMethod(service, methodName);
                    await ShowServiceMethods(method, clientInfo, clientInfo.Message, customText);
                }
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
        private List<List<BotButtonInfo>> GetListOfServicesButtons(TelegramClientInfo clientInfo)
        {
            List<BotButtonInfo> columns = new List<BotButtonInfo>();

            List<List<BotButtonInfo>> rows = new List<List<BotButtonInfo>>();

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
        private List<List<BotButtonInfo>> GetServiceMethodsButtons(Type service, TelegramClientInfo clientInfo)
        {
            List<BotButtonInfo> columns = new List<BotButtonInfo>();

            List<List<BotButtonInfo>> rows = new List<List<BotButtonInfo>>();

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
            rows.Add(new List<BotButtonInfo>() { new BotButtonInfo(CurrentBotStructureInfo.GetCancelButtonText(clientInfo)) });
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

        private string GetServiceName(Type service)
        {
            ServiceContractAttribute serviceAttribute = service.GetCustomAttribute<ServiceContractAttribute>();
            return serviceAttribute.Name;
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

        private List<List<BotButtonInfo>> GetMethodParametersButtons(MethodInfo method, TelegramClientInfo clientInfo)
        {
            List<BotButtonInfo> columns = new List<BotButtonInfo>();

            List<List<BotButtonInfo>> rows = new List<List<BotButtonInfo>>();

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
            rows.Add(new List<BotButtonInfo>() { new BotButtonInfo(CurrentBotStructureInfo.GetSendButtonText(clientInfo)) });
            rows.Add(new List<BotButtonInfo>() { new BotButtonInfo(CurrentBotStructureInfo.GetCancelButtonText(clientInfo)) });
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

        internal IEnumerable<IEnumerable<KeyboardButton>> BotButtonsToKeyboardButtons(List<List<BotButtonInfo>> botButtonInfos, TelegramClientInfo telegramClientInfo)
        {
            if (!UsersBotButtons.TryGetValue(telegramClientInfo, out Dictionary<string, BotButtonInfo> buttons))
                buttons = UsersBotButtons[telegramClientInfo] = new Dictionary<string, BotButtonInfo>();

            buttons.Clear();

            List<List<KeyboardButton>> keyboardButtons = new List<List<KeyboardButton>>();

            foreach (List<BotButtonInfo> columns in botButtonInfos)
            {
                List<KeyboardButton> rows = new List<KeyboardButton>();
                foreach (BotButtonInfo row in columns)
                {
                    rows.Add(new KeyboardButton() { Text = row.Key });
                    buttons.Add(row.Key, row);
                }
                keyboardButtons.Add(rows);
            }
            return keyboardButtons;
        }


        internal IEnumerable<IEnumerable<InlineKeyboardButton>> BotButtonsToKeyboardButtons(List<List<BotButtonInfo>> botButtonInfos)
        {
            List<List<InlineKeyboardButton>> keyboardButtons = new List<List<InlineKeyboardButton>>();

            foreach (List<BotButtonInfo> columns in botButtonInfos)
            {
                List<InlineKeyboardButton> rows = new List<InlineKeyboardButton>();
                foreach (BotButtonInfo row in columns)
                {
                    CustomInlineButtons.Add(row.Key, row.Click);
                    rows.Add(new InlineKeyboardButton()
                    {
                        Text = row.Caption,
                        CallbackData = row.Key
                    });
                }
                keyboardButtons.Add(rows);
            }
            return keyboardButtons;
        }
    }
}
