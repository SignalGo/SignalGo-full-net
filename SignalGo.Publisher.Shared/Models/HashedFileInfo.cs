using SignalGo.Publisher.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Shared.Models
{
    public class HashedFileDto
    {
        private readonly string _delimiter = "\\";
        private readonly int _skipLength = 0;

        //public HashedFileInfo(string delimiter = "\\")
        public HashedFileDto()
        {
            //_delimiter = delimiter;
            //_skipLength = delimiter.Length > 1 ? delimiter.Length : 0;
            _skipLength = _delimiter.Length;
        }

        public string FilePath { get; set; } = "";
        public string FileName
        {
            get
            {
                //return FilePath[(FilePath.LastIndexOf("\\") + 1)..],  //range operator is not available in c# 7.3
                return FilePath.Substring(FilePath.LastIndexOf(_delimiter) + _skipLength);
            }
        }
        public string FileHash { get; set; }
        public FileStatusType FileStatus { get; private set; } = FileStatusType.Deleted;


        #region Methods
        public HashedFileDto Clone()
        {
            return new HashedFileDto()
            {
                FilePath = FilePath,
                FileHash = FileHash,
                FileStatus = FileStatus,
            };
        }
        public void MarkAsUnchanged()
        {
            FileStatus = FileStatusType.Unchanged;
        }

        public void MarkAsModified()
        {
            FileStatus = FileStatusType.Modified;
        }

        public void MarkAsAdded()
        {
            FileStatus = FileStatusType.Added;
        }

        public void MarkAsDeleted()
        {
            FileStatus = FileStatusType.Deleted;
        }

        public void MarkAsIgnored()
        {
            FileStatus = FileStatusType.Ignored;
        }
        #endregion
    }
}
