using System;

namespace ObserveThing
{
    public class CombineValueObservable<T1, T2> : IDisposable
    {
        private ObservableValue<(T1, T2)> _operand;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, ObservableValue<(T1, T2)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => operand.value = new(x, operand.value.Item2),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source2.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, x),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3> : IDisposable
    {
        private ObservableValue<(T1, T2, T3)> _operand;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, ObservableValue<(T1, T2, T3)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => operand.value = new(x, operand.value.Item2, operand.value.Item3),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source2.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, x, operand.value.Item3),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source3.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, x),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4> : IDisposable
    {
        private ObservableValue<(T1, T2, T3, T4)> _operand;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, ObservableValue<(T1, T2, T3, T4)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => operand.value = new(x, operand.value.Item2, operand.value.Item3, operand.value.Item4),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source2.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, x, operand.value.Item3, operand.value.Item4),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source3.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, x, operand.value.Item4),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source4.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, x),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5> : IDisposable
    {
        private ObservableValue<(T1, T2, T3, T4, T5)> _operand;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, ObservableValue<(T1, T2, T3, T4, T5)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => operand.value = new(x, operand.value.Item2, operand.value.Item3, operand.value.Item4, operand.value.Item5),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source2.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, x, operand.value.Item3, operand.value.Item4, operand.value.Item5),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source3.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, x, operand.value.Item4, operand.value.Item5),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source4.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, x, operand.value.Item5),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source5.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, operand.value.Item4, x),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5, T6> : IDisposable
    {
        private ObservableValue<(T1, T2, T3, T4, T5, T6)> _operand;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, ObservableValue<(T1, T2, T3, T4, T5, T6)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => operand.value = new(x, operand.value.Item2, operand.value.Item3, operand.value.Item4, operand.value.Item5, operand.value.Item6),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source2.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, x, operand.value.Item3, operand.value.Item4, operand.value.Item5, operand.value.Item6),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source3.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, x, operand.value.Item4, operand.value.Item5, operand.value.Item6),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source4.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, x, operand.value.Item5, operand.value.Item6),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source5.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, operand.value.Item4, x, operand.value.Item6),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source6.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, operand.value.Item4, operand.value.Item5, x),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5, T6, T7> : IDisposable
    {
        private ObservableValue<(T1, T2, T3, T4, T5, T6, T7)> _operand;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, ObservableValue<(T1, T2, T3, T4, T5, T6, T7)> operand)
        {
            _operand = operand;
            _subscriptions = new ComposedDisposable(

                source1.Subscribe(
                    onNext: x => operand.value = new(x, operand.value.Item2, operand.value.Item3, operand.value.Item4, operand.value.Item5, operand.value.Item6, operand.value.Item7),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source2.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, x, operand.value.Item3, operand.value.Item4, operand.value.Item5, operand.value.Item6, operand.value.Item7),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source3.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, x, operand.value.Item4, operand.value.Item5, operand.value.Item6, operand.value.Item7),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source4.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, x, operand.value.Item5, operand.value.Item6, operand.value.Item7),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source5.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, operand.value.Item4, x, operand.value.Item6, operand.value.Item7),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source6.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, operand.value.Item4, operand.value.Item5, x, operand.value.Item7),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                source7.Subscribe(
                    onNext: x => operand.value = new(operand.value.Item1, operand.value.Item2, operand.value.Item3, operand.value.Item4, operand.value.Item5, operand.value.Item6, x),
                    onError: operand.OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _operand.Dispose();
        }
    }
}