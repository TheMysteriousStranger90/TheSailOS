using System;

namespace TheSailOS.MemoryTheSail;

[Flags]
public enum MemoryPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4
}