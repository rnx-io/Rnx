using Rnx.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rnx.Tasks.Core.FileSystem
{
    public class WriteFileData : Rnx.Common.Util.ICloneable
    {
        private string _directoryName;
        private string _baseName;
        private string _extension;
        private string _stem;

        public WriteFileData(string relativePath)
        {
            _extension = Path.GetExtension(relativePath);
            _baseName = Path.GetFileName(relativePath);
            _directoryName = Path.GetDirectoryName(relativePath);
            _stem = Path.GetFileNameWithoutExtension(relativePath);
        }

        public string WrittenFilename { get; set; }

        public string RelativePath
        {
            get { return Path.Combine(_directoryName, _stem + _extension); }
        }

        public string DirectoryName
        {
            get { return _directoryName; }
            set { _directoryName = value; }
        }

        public string Extension
        {
            get { return _extension; }
            set
            {
                _extension = value;
                _baseName = Path.GetFileNameWithoutExtension(_baseName) + value;
            }
        }

        public string Stem
        {
            get { return _stem; }
            set
            {
                _stem = value;
                _baseName = value + Path.GetExtension(_baseName);
            }
        }

        public string BaseName
        {
            get { return _baseName; }
            set
            {
                _baseName = value;
                _extension = Path.GetExtension(value);
                _stem = Path.GetFileNameWithoutExtension(value);
            }
        }

        public bool IsPathRooted => Path.IsPathRooted(_directoryName);

        public object Clone()
        {
            return new WriteFileData(RelativePath);
        }
    }
}