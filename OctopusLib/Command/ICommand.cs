using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OctopusLib
{
    public interface ICommand
    {
        bool Try { get; set; }
        bool IsEnabled { get; set; }
        int Sequence { get; set; }
        string CommandText { get; set; }
        RunCommandType CommandType { get; set; }
        Status Status { get; set; }

        string OutputParameter { get; set; }
        string OutputValue { get; set; }
        string Message { get; set; }
        ObservableCollectionEx<Parameter> ParameterCollection { get; set; }

        int RetryTimes { get; set; }

        int RetryIntervalSeconds { get; set; }
        int ExitCode { get; set; }

        bool Execute(Machine machine);
    }
}
