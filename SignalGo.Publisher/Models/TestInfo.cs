using MvvmGo.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Models
{
    public enum TestStatus : byte
    {
        None = 0,
        Error = 1,
        Pass = 2
    }

    public class TestInfo : BaseViewModel
    {
        string _Name;
        TestStatus _Status = TestStatus.None;

        public string Name
        {
            get => _Name; set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public TestStatus Status
        {
            get => _Status; set
            {
                _Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }
}
