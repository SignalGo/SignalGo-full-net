using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Publisher.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Models
{
    public class UploadInfo : BaseViewModel
    {
       public ICommand Command { get; set; }
        bool _Status;
        bool _HasProgress;

        //Func<Task<long>> _StreamPosition;
        string _Title;
        string _FileName;
        string _FileExtension;
        string _FilePath;
        public UploadInfo(ICommand command) : base()
        {
            Command = command;
        }


        //public Func<Task<long>> StreamPosition = new Func<Task<long>>(() =>
        //{
        //    Console.WriteLine("...");
        //    return Task.FromResult(Position);
        //});
        //{
        //    get
        //    {
        //        Debug.WriteLine(_StreamPosition);
        //        return _StreamPosition;
        //    }
        //    set
        //    {
        //        _StreamPosition = value;
        //        OnPropertyChanged("StreamPosition");
        //    }
        //}
        public string FilePath
        {
            get
            {
                return _FilePath;
            }
            set
            {
                _FilePath = value;
                OnPropertyChanged("FilePath");
            }
        }
       

        /// <summary>
        /// mime type of file, (rar,zip,...)
        /// </summary>
        public string FileExtension
        {
            get
            {
                return _FileExtension;
            }
            set
            {
                _FileExtension = value;
                OnPropertyChanged("FileExtension");
            }
        }

        /// <summary>
        /// name of file{s}
        /// </summary>
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
                OnPropertyChanged("Title");
            }
        }

        public bool HasProgress
        {
            get
            {
                return _HasProgress;
            }
            set
            {
                _HasProgress = value;
                OnPropertyChanged("HasProgress");
            }
        }

        /// <summary>
        ///  status of upload 
        /// </summary>
        public bool Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
            }
        }

    }
}
