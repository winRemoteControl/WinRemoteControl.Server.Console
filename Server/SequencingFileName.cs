using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class SequencingFileName
    {
        private string _fileNameFormat;
        public int Index = 0;

        public SequencingFileName(string fileNameFormat)
        {
            this._fileNameFormat = fileNameFormat;
            var p = Path.GetDirectoryName(fileNameFormat);
            if (System.IO.Directory.Exists(p))
                System.IO.Directory.CreateDirectory(p);
        }

        private string MakeFileName(int index)
        {
            return string.Format(this._fileNameFormat, index.ToString("000"));
        }

        public string GetCurrentFileName()
        {
            return this.MakeFileName(this.Index);
        }

        public string GetPreviousFileName()
        {
            return this.MakeFileName(this.Index-1);
        }

        public string GetNextFilename()
        {
            var f = GetCurrentFileName();
            this.Index++;
            return f;
        }

        public void CleanButKeepLastOne()
        {
            Clean(this.Index-1);
        }

        public void Clean(int? upToIndex = null)
        {
            if (!upToIndex.HasValue)
                upToIndex = this.Index;

            for (var i = 0; i < upToIndex; i++)
            {
                var f = this.MakeFileName(i);
                if (System.IO.File.Exists(f))
                    System.IO.File.Delete(f);
            }
        }
    }
}
