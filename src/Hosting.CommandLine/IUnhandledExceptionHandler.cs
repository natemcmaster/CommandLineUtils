using System;

namespace McMaster.Extensions.Hosting.CommandLine
{
    public interface IUnhandledExceptionHandler
    {
        void HandleException(Exception e);
    }
}
