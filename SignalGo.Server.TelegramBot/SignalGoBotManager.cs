using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SignalGo.Server.TelegramBot
{
    public class SignalGoBotManager
    {
        private TelegramBotClient _botClient;
        private ServerBase _serverBase;

        private Dictionary<string, Type> Services { get; set; } = new Dictionary<string, Type>();
        private List<List<KeyboardButton>> ServicesButtons { get; set; }
        public async void Start(string token, ServerBase serverBase)
        {
            _serverBase = serverBase;
            _botClient = new TelegramBotClient(token);

            User me = await _botClient.GetMeAsync();

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Text != null)
                {

                    //Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");
                    if (Services.TryGetValue(e.Message.Text, out Type service))
                    {
                        ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                        {
                            Keyboard = GetServiceMethodsButtons(service)
                        };
                        await _botClient.SendTextMessageAsync(
                             chatId: e.Message.Chat,
                             text: "You said:\n" + e.Message.Text,
                             replyMarkup: replyMarkup
                           );
                    }
                    else
                    {
                        ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup
                        {
                            Keyboard = GetListOfServicesButtons()
                        };
                        await _botClient.SendTextMessageAsync(
                             chatId: e.Message.Chat,
                             text: "You said:\n" + e.Message.Text,
                             replyMarkup: replyMarkup
                           );
                    }

                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private List<List<KeyboardButton>> GetListOfServicesButtons()
        {
            if (ServicesButtons != null)
                return ServicesButtons;
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>()
            {

            };

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
            ServicesButtons = rows;
            return rows;
        }

        private List<List<KeyboardButton>> GetServiceMethodsButtons(Type service)
        {
            List<KeyboardButton> columns = new List<KeyboardButton>();

            List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>()
            {

            };

            int columnIndex = 0;
            var methods = service.GetListOfMethods().Where(x=>x.IsPublic && !x.IsStatic).ToList();
            for (int i = 0; i < methods.Count; i++)
            {
                var item = methods[i];
                if (columnIndex == 2)
                {
                    columnIndex = 0;
                    rows.Add(columns.ToList());
                    columns.Clear();
                }
                columns.Add(item.Name);
                columnIndex++;
            }

            return rows;
        }
    }
}
