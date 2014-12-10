using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
using OctopusLib;
using AutomationHelper;

namespace Octopus
{
    class Program
    {
        private static OctopusLib.OctopusInstance octopusInstance;
        private static string _octopusfile;
        private static string _logpath = Path.Combine(Environment.CurrentDirectory, "Log");
        static Dictionary<string, string> Arguments;

        static void Exception()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("===============================================================================");
            sb.AppendLine("Welcome to use HP Octopus Console, this tool is to distributed executing on remote server, for example: distribution installation.");
            sb.AppendLine("Format: Octopus.exe -f=<configuration file> [-l=<Log Path>] [<parameter name>=<parameter value>]");
            sb.AppendLine("Detail Information:");
            sb.AppendLine("     -f : configuration file, it is mandatory, you have to specify it with full path.");
            sb.AppendLine("     -l : specified the log path, not log file, for example: c:\\Octopuslog");
            sb.AppendLine("     <parameter name>=<parameter value> : in configuration file, there are some customized parameters defined, if you'd like to overwrite them from command line. You can specify them in command with the format.");
            sb.AppendLine("     for example: DROP_NUM=12.0.1100.0 VERSION=12");
            sb.AppendLine("Example: Octopus.exe -f=c:\\octopus_example.xml -l=c:\\Octopuslog DROP_NUM=12.0.1100.0 VERSION=12");
            sb.AppendLine("===============================================================================");
            sb.AppendLine();
            Console.WriteLine(sb.ToString());
            Environment.Exit(-1);
        }

        static void GetArguments(string[] args)
        {
            Arguments = new Dictionary<string, string>();
            foreach (string s in args)
            {
                Arguments.Add(s.Split(new char[] { '=' })[0], s.Split(new char[] { '=' })[1]);
            }
        }

        static void Main(string[] args)
        {
            GetArguments(args);
            _octopusfile = !Arguments.ContainsKey("-f") ? string.Empty : Arguments["-f"];
            if (Arguments.ContainsKey("-l"))
                _logpath = Arguments["-l"];

            if (string.IsNullOrEmpty(_octopusfile)) Exception();
            if (!File.Exists(_octopusfile)) Exception();
            if (!Directory.Exists(_logpath)) Directory.CreateDirectory(_logpath);
            
            octopusInstance = new OctopusInstance(_octopusfile);
            octopusInstance.Load();

            if (octopusInstance.TaskCollection == null)
            {
                Console.WriteLine(string.Format("Load Octopus task configuration file failed."));
                Environment.Exit(-1);
            }

            UpdateParameter();

            Start();
        }

        static void UpdateParameter()
        {
            foreach(KeyValuePair<string, string> kv in Arguments)
            {
                if (octopusInstance.ParameterCollection.Where(o=>o.Name.Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
                {
                    (octopusInstance.ParameterCollection.Where(o => o.Name.Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase)).Single() as OctopusLib.Parameter).Value = kv.Value;
                }
            }            
        }

        static void Start()
        {
            Console.WriteLine("******************* OCTOPUS START *******************");
            octopusInstance.CreateRunInstance();
            List<Thread> runInstanceThreads = new List<Thread>();
            foreach (var runInstancesGroup in octopusInstance.RunInstanceCollection.OrderBy(k => k.Task.Sequence).GroupBy(p => p.Task.Sequence))
            {
                foreach (var runInstance in runInstancesGroup)
                {
                    Thread runInstanceThread = new Thread(runInstance.StartRun);
                    runInstanceThread.Priority = ThreadPriority.AboveNormal;
                    runInstanceThread.Start();
                    runInstanceThreads.Add(runInstanceThread);
                }
                foreach (Thread thread in runInstanceThreads)
                {
                    // Wait until thread is finished.
                    thread.Join();
                }
            }
            Console.WriteLine("Log files:");
            foreach(var runInstance in octopusInstance.RunInstanceCollection)
            {
                Console.WriteLine(runInstance.LogFile);
            }
            Console.WriteLine("******************* OCTOPUS FINISHED.*******************");

            if (octopusInstance.RunInstanceCollection.Where(o => o.Status == OctopusLib.Status.Fail).Count() > 0)
            {

                Environment.Exit(-1);
            }
            else
            {
                Environment.Exit(0);
            }                
        }
    }
}
