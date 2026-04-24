using System;

namespace ObserveThing
{
    public class CombineValueObservable<T1, T2> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<(T1, T2)> _receiver;
        private (T1, T2) _value;
        private bool _disposed;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObserver<(T1, T2)> receiver)
        {
            _receiver = receiver;
            _sourceStream = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => HandleNext(x, _value.Item2),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source2.Subscribe(
                    onNext: x => HandleNext(_value.Item1, x),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                )

            );

            // Always send init call
            if (Equals(_value, default((T1, T2))))
                _receiver.OnNext(default);
        }

        private void HandleNext(T1 value1, T2 value2)
        {
            (T1, T2) newValue = new(value1, value2);

            if (Equals(newValue, _value))
                return;

            _value = newValue;
            _receiver.OnNext(_value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<(T1, T2, T3)> _receiver;
        private (T1, T2, T3) _value;
        private bool _disposed;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObserver<(T1, T2, T3)> receiver)
        {
            _receiver = receiver;
            _sourceStream = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => HandleNext(x, _value.Item2, _value.Item3),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source2.Subscribe(
                    onNext: x => HandleNext(_value.Item1, x, _value.Item3),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source3.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, x),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                )

            );

            // Always send init call
            if (Equals(_value, default((T1, T2, T3))))
                _receiver.OnNext(default);
        }

        private void HandleNext(T1 value1, T2 value2, T3 value3)
        {
            (T1, T2, T3) newValue = new(value1, value2, value3);

            if (Equals(newValue, _value))
                return;

            _value = newValue;
            _receiver.OnNext(_value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<(T1, T2, T3, T4)> _receiver;
        private (T1, T2, T3, T4) _value;
        private bool _disposed;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObserver<(T1, T2, T3, T4)> receiver)
        {
            _receiver = receiver;
            _sourceStream = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => HandleNext(x, _value.Item2, _value.Item3, _value.Item4),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source2.Subscribe(
                    onNext: x => HandleNext(_value.Item1, x, _value.Item3, _value.Item4),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source3.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, x, _value.Item4),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source4.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, x),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                )

            );

            // Always send init call
            if (Equals(_value, default((T1, T2, T3, T4))))
                _receiver.OnNext(default);
        }

        private void HandleNext(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            (T1, T2, T3, T4) newValue = new(value1, value2, value3, value4);

            if (Equals(newValue, _value))
                return;

            _value = newValue;
            _receiver.OnNext(_value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<(T1, T2, T3, T4, T5)> _receiver;
        private (T1, T2, T3, T4, T5) _value;
        private bool _disposed;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObserver<(T1, T2, T3, T4, T5)> receiver)
        {
            _receiver = receiver;
            _sourceStream = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => HandleNext(x, _value.Item2, _value.Item3, _value.Item4, _value.Item5),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source2.Subscribe(
                    onNext: x => HandleNext(_value.Item1, x, _value.Item3, _value.Item4, _value.Item5),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source3.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, x, _value.Item4, _value.Item5),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source4.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, x, _value.Item5),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source5.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, _value.Item4, x),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                )
            );

            // Always send init call
            if (Equals(_value, default((T1, T2, T3, T4, T5))))
                _receiver.OnNext(default);
        }

        private void HandleNext(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            (T1, T2, T3, T4, T5) newValue = new(value1, value2, value3, value4, value5);

            if (Equals(newValue, _value))
                return;

            _value = newValue;
            _receiver.OnNext(_value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5, T6> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<(T1, T2, T3, T4, T5, T6)> _receiver;
        private (T1, T2, T3, T4, T5, T6) _value;
        private bool _disposed;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObserver<(T1, T2, T3, T4, T5, T6)> receiver)
        {
            _receiver = receiver;
            _sourceStream = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => HandleNext(x, _value.Item2, _value.Item3, _value.Item4, _value.Item5, _value.Item6),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source2.Subscribe(
                    onNext: x => HandleNext(_value.Item1, x, _value.Item3, _value.Item4, _value.Item5, _value.Item6),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source3.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, x, _value.Item4, _value.Item5, _value.Item6),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source4.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, x, _value.Item5, _value.Item6),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source5.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, _value.Item4, x, _value.Item6),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source6.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, _value.Item4, _value.Item5, x),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                )
            );

            // Always send init call
            if (Equals(_value, default((T1, T2, T3, T4, T5, T6))))
                _receiver.OnNext(default);
        }

        private void HandleNext(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            (T1, T2, T3, T4, T5, T6) newValue = new(value1, value2, value3, value4, value5, value6);

            if (Equals(newValue, _value))
                return;

            _value = newValue;
            _receiver.OnNext(_value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5, T6, T7> : IDisposable
    {
        private IDisposable _sourceStream;
        private IValueObserver<(T1, T2, T3, T4, T5, T6, T7)> _receiver;
        private (T1, T2, T3, T4, T5, T6, T7) _value;
        private bool _disposed;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, IValueObserver<(T1, T2, T3, T4, T5, T6, T7)> receiver)
        {
            _receiver = receiver;
            _sourceStream = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => HandleNext(x, _value.Item2, _value.Item3, _value.Item4, _value.Item5, _value.Item6, _value.Item7),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source2.Subscribe(
                    onNext: x => HandleNext(_value.Item1, x, _value.Item3, _value.Item4, _value.Item5, _value.Item6, _value.Item7),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source3.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, x, _value.Item4, _value.Item5, _value.Item6, _value.Item7),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source4.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, x, _value.Item5, _value.Item6, _value.Item7),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source5.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, _value.Item4, x, _value.Item6, _value.Item7),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source6.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, _value.Item4, _value.Item5, x, _value.Item7),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                ),

                source7.Subscribe(
                    onNext: x => HandleNext(_value.Item1, _value.Item2, _value.Item3, _value.Item4, _value.Item5, _value.Item6, x),
                    onError: _receiver.OnError,
                    onDispose: Dispose,
                    immediate: receiver.immediate
                )
            );

            // Always send init call
            if (Equals(_value, default((T1, T2, T3, T4, T5, T6, T7))))
                _receiver.OnNext(default);
        }

        private void HandleNext(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            (T1, T2, T3, T4, T5, T6, T7) newValue = new(value1, value2, value3, value4, value5, value6, value7);

            if (Equals(newValue, _value))
                return;

            _value = newValue;
            _receiver.OnNext(_value);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _sourceStream.Dispose();

            _receiver.OnDispose();
        }
    }
}