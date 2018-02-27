using System;

namespace CodeBeingTested.Interfaces
{
    public interface ILogger
    {
        void Write(string s);
        void Debug(string s);
        void Error(Exception exception);
    }
}