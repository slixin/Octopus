using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace OctopusLib
{
    public class LogHelper
    {
        public enum LEVEL {LOG, ERROR, WARNING}

        public static void Log(string file, string message)
        {
            WriteLog(file, LEVEL.LOG, message);
        }

        public static void Warn(string file, string message)
        {
            WriteLog(file, LEVEL.WARNING, message);
        }

        public static void Error(string file, string message)
        {
            WriteLog(file, LEVEL.ERROR, message);
        }

        private static void WriteLog(string file, LEVEL level, string message)
        {
            using (StreamWriter sw = new StreamWriter(file, true))
            {
                sw.WriteLine(string.Format("{0}:{1} {2}", DateTime.Now.ToString(), level.ToString(), message));
                sw.Flush();
                sw.Close();
            }

            if (level == LEVEL.ERROR)
                throw new Exception(message);
            else
                Console.WriteLine(message);
            
        }
    }
}
