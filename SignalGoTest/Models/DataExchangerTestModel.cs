using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;

namespace SignalGoTest.Models
{
    public class UserInfoTest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.OutgoingCall)]
        public string Password { get; set; }
        public int Age { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.IncomingCall)]
        public List<PostInfoTest> PostInfoes { get; set; }
        public List<RoleInfoTest> RoleInfoes { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.Both)]
        public PostInfoTest LastPostInfo { get; set; }
    }

    [CustomDataExchanger("Title", "Text", ExchangeType = CustomDataExchangerType.TakeOnly, LimitationMode = LimitExchangeType.IncomingCall)]
    [CustomDataExchanger("Id", "Title", "Text", ExchangeType = CustomDataExchangerType.TakeOnly, LimitationMode = LimitExchangeType.OutgoingCall)]
    public class PostInfoTest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string PostSecurityLink { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.Both)]
        public UserInfoTest User { get; set; }
        public RoleInfoTest PostRoleToSee { get; set; }
    }

    public class ValidationRule
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class MessageContract<T>
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; }
        public List<ValidationRule> Errors { get; set; }
    }

    public class ArticleInfo
    {
        [EmptyValidationRule(Message = "please fill the Name!")]
        public string Name { get; set; }
        [EmptyValidationRule(Message = "please fill the Detail!")]
        public string Detail { get; set; }
        [CreatedDateTime(TaskType = ValidationRuleInfoTaskType.ChangeValue)]
        public DateTime? CreatedDateTime { get; set; }
    }

    public class EmptyValidationRuleAttribute : BaseValidationRuleAttribute
    {
        public override bool CheckIsValidate()
        {
            if (CurrentValue == null || (CurrentValue is string text && string.IsNullOrEmpty(text)))
                return false;
            return true;
        }

        public override object GetChangedValue()
        {
            throw new NotImplementedException();
        }

        public override object GetErrorValue()
        {
            return Message;
        }
    }

    public abstract class BaseValidationRuleAttribute : ValidationRuleInfoAttribute
    {
        public string Message { get; set; }
    }

    public class CreatedDateTimeAttribute : ValidationRuleInfoAttribute
    {
        public override bool CheckIsValidate()
        {
            return CurrentValue != null;
        }

        public override object GetChangedValue()
        {
            return DateTime.Now;
        }

        public override object GetErrorValue()
        {
            throw new NotImplementedException();
        }
    }

    public enum RoleTypeTest
    {
        Normal,
        Admin,
        Editor,
        Viewer
    }

    public class RoleInfoTest
    {
        public int Id { get; set; }
        public RoleTypeTest Type { get; set; }
        public UserInfoTest User { get; set; }
    }
}
