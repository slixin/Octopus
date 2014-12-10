using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomationHelper;
using System.Management;

namespace EncryptionTool
{
    class Program
    {
        static void Main(string[] args)
        {
            bool decrypt = false;
            string returnvalue = null;

            if (args == null)
                Alert();

            if (args.Length == 0)
                Alert();
            string value = args[0];
            string key = GetProcessorSerial();

            if (args.Where(o => o.ToLower().Equals("/d")).Count() > 0)
                decrypt = true;

            if (!decrypt)
            {
                returnvalue = Encryption.Encrypt(value, key);
            }
            else
            {
                returnvalue = Encryption.Decrypt(value, key);
            }

            Console.WriteLine(returnvalue);
        }

        static void Alert()
        {
            Console.WriteLine("The format: EncryptionTool.exe <Encrypt/Decrypt string> [/d(decrypt)], for example: EncryptionTool.exe helloworld or EncryptionTool.exe 1sd23as3 /d");
        }

        static string GetProcessorSerial()
        {
            string cpuInfo = string.Empty;
            ManagementClass cimobject = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = cimobject.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
            }

            return cpuInfo;
        }
    }
}
