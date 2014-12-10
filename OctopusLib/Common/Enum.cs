using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OctopusLib
{
    public enum Status { Pass, Warn, Fail, NotRun, InProgress };
    public enum RunCommandType { Remote, Local, Copy, LinuxSSH };
    public enum SSHType { Exec, Stream };
}
