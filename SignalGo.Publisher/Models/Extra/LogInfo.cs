using System;

namespace SignalGo.Publisher.Models.Extra
{
    /// <summary>
    /// using to store logs and it's properties
    /// </summary>
    public class LogInfo
    {
        #region Fields
        private string _logText;
        private string _logDateTime = string.Empty;
        private LogTypeEnum _logType = LogTypeEnum.System;
        #endregion

        #region Properties and functions
        public string LogText
        {
            get => _logText;
            set => _logText = value;
        }
        public string LogDateTime
        {
            get => _logDateTime;
            set => _logDateTime = value;
        }

        public LogTypeEnum LogType
        {
            get => _logType;
            set => _logType = value;
        }

        public override string ToString()
        {
            return $"{_logDateTime} {_logText}";
        }
        #endregion

        #region Constructor's 
        public LogInfo(string text, LogTypeEnum logType)
        {
            _logText = text;
            _logType = logType;
        }
        public LogInfo(string text, string dateTime, LogTypeEnum logType)
        {
            _logText = text;
            _logDateTime = dateTime;
            _logType = logType;
        }
        public LogInfo(string text, string dateTime)
        {
            _logText = text;
            _logDateTime = dateTime;
        }
#endregion
    }
}
