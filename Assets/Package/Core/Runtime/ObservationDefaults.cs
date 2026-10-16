using System;

namespace ObserveThing
{
    public static class ObservationDefaults
    {
        public static Action<Exception> DefaultExceptionHandler = UnityEngine.Debug.LogException;
        public static ObservationContext DefaultObservationContext = new ObservationContext();
    }
}