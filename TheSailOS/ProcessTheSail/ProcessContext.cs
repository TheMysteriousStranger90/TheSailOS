using System;
using System.Collections.Generic;

namespace TheSailOS.ProcessTheSail;

public class ProcessContext
{
    public uint StackPointer { get; set; }
    public uint ProgramCounter { get; set; }
    public Dictionary<string, object> Registers { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public int TimeQuantum { get; set; }

    public ProcessContext()
    {
        Registers = new Dictionary<string, object>();
        TimeQuantum = 100;
        ExecutionTime = TimeSpan.Zero;
    }
}