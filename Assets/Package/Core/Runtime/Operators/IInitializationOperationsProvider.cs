using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IInitializationOperationsProvider<T> : IDisposable where T : IOperation
    {
        IEnumerable<T> GetInitializationOperations();
    }
}
