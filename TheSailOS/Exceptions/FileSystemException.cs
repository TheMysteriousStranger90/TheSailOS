﻿using System;

namespace TheSailOS.Exceptions;

public class FileSystemException : Exception
{
    public FileSystemException(string message) : base(message) { }
    public FileSystemException(string message, Exception inner) : base(message, inner) { }
}