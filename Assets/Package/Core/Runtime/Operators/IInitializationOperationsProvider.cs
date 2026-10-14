using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IInitializationOperationsProvider<T> : IDisposable
    {
        IEnumerable<T> GetInitializationOperations();
    }
}
