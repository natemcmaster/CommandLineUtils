using System;

namespace McMaster.Extensions.Hosting.CommandLine.Internal
{
    public class StoreExceptionHandler : IUnhandledExceptionHandler
    {
        public Exception StoredException { get; private set; } = null;

        public void HandleException(Exception e)
        {
            if (StoredException != null)
            {
                throw new AggregateException("Second exception received!", StoredException, e);
            }

            StoredException = e;
        }
    }
}
