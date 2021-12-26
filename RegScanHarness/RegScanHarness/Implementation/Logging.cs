using System;
using System.IO;

namespace ScanX.Implementation
{
    public class Logging
    {
        public Logging(string path)
        {
            FilePath = path;
        }

        public string FilePath {get; set;}

        public void WriteLine(string message)
        {
            try
            {
                StreamWriter writeStream = File.AppendText(FilePath);
                writeStream.WriteLine("  :{0}", message);
                writeStream.Flush();
                writeStream.Close();
            }
            catch { }
        }

        public void WriteLog(string message)
        {
            if (FilePath.Length == 0)
                return;
            try
            {
                StreamWriter writeStream = File.AppendText(FilePath);
                writeStream.Write("\r\nLog Entry : ");
                writeStream.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                writeStream.WriteLine("  :");
                writeStream.WriteLine("  :{0}", message);
                writeStream.WriteLine("-------------------------------");
                writeStream.Flush();
                writeStream.Close();
            }
            catch { }
        }
        public string FetchLog()
        {
            string line = String.Empty;
            string entry = String.Empty;

            if (FilePath.Length == 0)
                return line;
            try
            {
                StreamReader readStream = File.OpenText(FilePath);
                while ((line = readStream.ReadLine()) != null)
                {
                    entry += line;
                }
                readStream.Close();
            }
            catch { }
            return line;
        }
    }
}
