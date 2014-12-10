using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace OctopusLib.Common
{
    public class WMI
    {
        static public string GetProcessorSerial()
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
