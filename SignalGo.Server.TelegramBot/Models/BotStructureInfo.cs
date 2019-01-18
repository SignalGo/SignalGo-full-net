using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;

namespace SignalGo.Server.TelegramBot.Models
{
    public class BotStructureInfo : IBotStructureInfo
    {
        public bool InitializeServicesFromAttributes { get; set; } = false;

        public string GetCancelButtonText(TelegramClientInfo clientInfo)
        {
            return "/Cancel";
        }

        public string GetMethodSelectedText(string methodName, TelegramClientInfo clientInfo)
        {
            return $"Method {methodName} Selected!";
        }

        public string GetParameterNotFoundText(string parameterName, TelegramClientInfo clientInfo)
        {
            return $"Parameter {parameterName} not found!";
        }

        public string GetParameterSelectedText(string parameterName, TelegramClientInfo clientInfo)
        {
            return $"Please Send {parameterName} Value:";
        }

        public string GetParameterValueChangedText(string parameterName, TelegramClientInfo clientInfo)
        {
            return $"Parameter {parameterName} value changed, please click on Send button or another parameter";
        }

        public string GetSendButtonText(TelegramClientInfo clientInfo)
        {
            return "/Send";
        }

        public string GetServiceNotFoundText(string serviceName, TelegramClientInfo clientInfo)
        {
            return $"Service {serviceName} not found";
        }

        public string GetServiceSelectedText(string serviceName, string caption, Type serviceType, TelegramClientInfo clientInfo)
        {
            return "Service Selected:\n" + serviceName;
        }

        public string GetServicesGeneratedText(TelegramClientInfo clientInfo)
        {
            return "Services Generated!";
        }

        public void OnStarted(SignalGoBotManager signalGoBotManager)
        {

        }

        public bool OnBeforeMethodCall(ServerBase serverBase, TelegramClientInfo clientInfo, string serviceName, string methodName, List<ParameterInfo> parameters)
        {
            return true;
        }

        public string OnCustomResponse(ServerBase serverBase, TelegramClientInfo clientInfo, string serviceName, string methodName, List<ParameterInfo> parameters, CallMethodResultInfo<OperationContext> currentResponse, out bool changed)
        {
            changed = false;
            return null;
        }

        public bool OnServiceGenerating(string serviceName, TelegramClientInfo clientInfo)
        {
            return true;
        }

        public void OnButtonsGenerating(List<List<BotButtonInfo>> buttons, BotLevelType botLevelType, string serviceName, string methodName, TelegramClientInfo clientInfo)
        {

        }

        public void OnClientConnected(TelegramClientInfo clientInfo, SignalGoBotManager signalGoBotManager)
        {

        }

        public bool OnParameterSelecting(System.Reflection.MethodInfo methodInfo, System.Reflection.ParameterInfo parameterInfo, TelegramClientInfo clientInfo, string value)
        {
            return false;
        }
    }
}
