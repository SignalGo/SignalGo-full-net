using System;
using System.Collections.Generic;
using System.Text;
using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Models;

namespace SignalGo.Server.TelegramBot.Models
{
    public class BotStructureInfo : IBotStructureInfo
    {
        public bool InitializeServicesFromAttributes { get; set; } = false;

        public string GetCancelButtonText()
        {
            return "/Cancel";
        }

        public string GetMethodSelectedText(string methodName)
        {
            return $"Method {methodName} Selected!";
        }

        public string GetParameterNotFoundText(string parameterName)
        {
            return $"Parameter {parameterName} not found!";
        }

        public string GetParameterSelectedText(string parameterName)
        {
            return $"Please Send {parameterName} Value:";
        }

        public string GetParameterValueChangedText(string parameterName)
        {
            return $"Parameter {parameterName} value changed, please click on Send button or another parameter";
        }

        public string GetSendButtonText()
        {
            return "/Send";
        }

        public string GetServiceNotFoundText(string serviceName)
        {
            return $"Service {serviceName} not found";
        }

        public string GetServiceSelectedText(string serviceName)
        {
            return "Service Selected:\n" + serviceName;
        }

        public string GetServicesGeneratedText()
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
    }
}
