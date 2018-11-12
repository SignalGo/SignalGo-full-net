using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Server.ServiceManager.Providers;
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
        private TelegramBotClient _botClient;
        private ServerBase _serverBase;

        private Dictionary<string, Type> Services { get; set; } = new Dictionary<string, Type>();
        private List<List<KeyboardButton>> ServicesButtons { get; set; }
        private ConcurrentDictionary<int, TelegramClientInfo> ConnectedClients { get; set; } = new ConcurrentDictionary<int, TelegramClientInfo>();
        public async void Start(string token, ServerBase serverBase,System.Net.Http.HttpClient httpClient = null)
        {
            _serverBase = serverBase;
            _botClient = new TelegramBotClient(token, httpClient);

            User me = await _botClient.GetMeAsync();

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();

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
                        clientInfo = new TelegramClientInfo
                        {
                            ConnectedDateTime = DateTime.Now,
                            ClientId = Guid.NewGuid().ToString()
                        };
                        _serverBase.Clients.TryAdd(clientInfo.ClientId, clientInfo);
                        ConnectedClients.TryAdd(e.Message.From.Id, clientInfo);
                    }

                    if (e.Message.Text == "/Cancel")
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
                    else if (e.Message.Text == "/Send" && !string.IsNullOrEmpty(clientInfo.CurrentServiceName) && !string.IsNullOrEmpty(clientInfo.CurrentMethodName))
                    {
                        if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
                        {
                            Shared.Models.CallMethodResultInfo<OperationContext> result = await BaseProvider.CallMethod(clientInfo.CurrentServiceName, Guid.NewGuid().ToString(), clientInfo.CurrentMethodName, clientInfo.ParameterInfoes.ToArray()
                            , null, clientInfo, null, _serverBase, null, x => true);
                            MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentMethodName, StringComparison.OrdinalIgnoreCase));
                            await ShowResultValue(result, method, clientInfo, e);
                        }
                    }
                    else if (string.IsNullOrEmpty(clientInfo.CurrentServiceName))
                    {
                        if (Services.ContainsKey(e.Message.Text))
                        {
                            await ShowServiceMethods(e.Message.Text, clientInfo, e);
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
                            MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(e.Message.Text, StringComparison.OrdinalIgnoreCase));
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
                            ParameterInfo parameter = method.GetParameters().FirstOrDefault(x => x.Name.Equals(e.Message.Text, StringComparison.OrdinalIgnoreCase));
                            await GetParameterValueFromClient(method, parameter, clientInfo, e);
                        }
                    }
                    else
                    {
                        if (Services.TryGetValue(clientInfo.CurrentServiceName, out Type service))
                        {
                            MethodInfo method = service.GetFullServiceLevelMethods().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentMethodName, StringComparison.OrdinalIgnoreCase));
                            ParameterInfo parameter = method.GetParameters().FirstOrDefault(x => x.Name.Equals(clientInfo.CurrentParameterName, StringComparison.OrdinalIgnoreCase));
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

        public async Task ShowResultValue(Shared.Models.CallMethodResultInfo<OperationContext> callMethodResultInfo, MethodInfo methodInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            string value = e.Message.Text;
            clientInfo.CurrentParameterName = null;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetMethodParametersButtons(methodInfo)
            };
            if (callMethodResultInfo.CallbackInfo.Data.Length > 4000)
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(callMethodResultInfo.CallbackInfo.Data)))
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

        public async Task SetParameterValueFromClient(MethodInfo methodInfo, ParameterInfo parameterInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            string value = e.Message.Text;
            clientInfo.CurrentParameterName = null;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetMethodParametersButtons(methodInfo)
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
                text: $"Parameter value changed, please click on Send button or another parameter",
                replyMarkup: replyMarkup
               );
        }

        public async Task GetParameterValueFromClient(MethodInfo methodInfo, ParameterInfo parameterInfo, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            if (parameterInfo == null)
            {
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = GetMethodParametersButtons(methodInfo)
                };
                await _botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: $"Parameter {e.Message.Text} not found!",
                    replyMarkup: replyMarkup
                   );
            }
            else
            {
                clientInfo.CurrentParameterName = parameterInfo.Name;
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = GetMethodParametersButtons(methodInfo)
                };
                await _botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: $"Please Send {parameterInfo.Name} Value:",
                    replyMarkup: replyMarkup
                   );
            }
        }

        public async Task ShowServiceMethods(MethodInfo method, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            clientInfo.CurrentMethodName = method.Name;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetMethodParametersButtons(method)
            };
            await _botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: $"Method {e.Message.Text} Selected!",
                replyMarkup: replyMarkup
               );
        }

        public async Task ShowServiceMethods(string serviceName, TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            clientInfo.CurrentServiceName = serviceName;
            clientInfo.CurrentMethodName = null;
            if (Services.TryGetValue(serviceName, out Type service))
            {
                ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                {
                    Keyboard = GetServiceMethodsButtons(service)
                };
                await _botClient.SendTextMessageAsync(
                     chatId: e.Message.Chat,
                     text: "Service Selected:\n" + e.Message.Text,
                     replyMarkup: replyMarkup
                   );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: $"Service {serviceName} not found"
                  );
            }
        }

        public async Task ShowServices(TelegramClientInfo clientInfo, MessageEventArgs e)
        {
            clientInfo.CurrentServiceName = null;
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
            {
                Keyboard = GetListOfServicesButtons()
            };
            await _botClient.SendTextMessageAsync(
                 chatId: e.Message.Chat,
                 text: "Services Generated!",
                 replyMarkup: replyMarkup
               );
        }

        /// <summary>
        /// get buttons of all services
        /// </summary>
        /// <returns></returns>
        private List<List<KeyboardButton>> GetListOfServicesButtons()
        {
            if (ServicesButtons != null)
                return ServicesButtons;
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();

            int columnIndex = 0;
            List<System.Type> services = _serverBase.GetListOfRegistredTypes().ToList();
            for (int i = 0; i < services.Count; i++)
            {
                System.Type item = services[i];
                ServiceContractAttribute attribute = item.GetCustomAttribute<ServiceContractAttribute>();
                if (attribute.ServiceType != ServiceType.HttpService || Services.ContainsKey(attribute.Name))
                    continue;
                Services.Add(attribute.Name, item);
                if (columnIndex == 3)
                {
                    columnIndex = 0;
                    rows.Add(columns.ToList());
                    columns.Clear();
                }
                columns.Add(item.GetCustomAttribute<ServiceContractAttribute>().Name);
                columnIndex++;
            }
            if (rows.Count == 0)
                rows.Add(columns);
            ServicesButtons = rows;
            return rows;
        }

        /// <summary>
        /// get buttons of service
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private List<List<KeyboardButton>> GetServiceMethodsButtons(Type service)
        {
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();

            int columnIndex = 0;

            List<MethodInfo> methods = service.GetFullServiceLevelMethods().ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                MethodInfo item = methods[i];
                if (columnIndex == 2)
                {
                    columnIndex = 0;
                    rows.Add(columns.ToList());
                    columns.Clear();
                }
                columns.Add(item.Name);
                columnIndex++;
            }
            if (rows.Count == 0)
                rows.Add(columns);
            rows.Add(new List<KeyboardButton>() { new KeyboardButton("/Cancel") });
            return rows;
        }

        private List<List<KeyboardButton>> GetMethodParametersButtons(MethodInfo method)
        {
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();

            int columnIndex = 0;

            List<ParameterInfo> parameters = method.GetParameters().ToList();
            for (int i = 0; i < parameters.Count; i++)
            {
                ParameterInfo item = parameters[i];
                if (columnIndex == 2)
                {
                    columnIndex = 0;
                    rows.Add(columns.ToList());
                    columns.Clear();
                }
                columns.Add(item.Name);
                columnIndex++;
            }
            if (rows.Count == 0)
                rows.Add(columns);
            rows.Add(new List<KeyboardButton>() { new KeyboardButton("/Send") });
            rows.Add(new List<KeyboardButton>() { new KeyboardButton("/Cancel") });
            return rows;
        }
    }
}
