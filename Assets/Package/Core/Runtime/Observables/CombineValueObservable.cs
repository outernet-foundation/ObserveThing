using System;

namespace ObserveThing
{
    public class CombineValueObservable<T1, T2> : ObservableValueBase<(T1, T2)>
    {
        private IValueObservable<T1> _source1;
        private IValueObservable<T2> _source2;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2) : base(source1.context)
        {
            _source1 = source1;
            _source2 = source2;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: x => SetValueInternal(new(x, _value.Item2)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source2.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, x)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    public class CombineValueObservable<T1, T2, T3> : ObservableValueBase<(T1, T2, T3)>
    {
        private IValueObservable<T1> _source1;
        private IValueObservable<T2> _source2;
        private IValueObservable<T3> _source3;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3) : base(source1.context)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: x => SetValueInternal(new(x, _value.Item2, _value.Item3)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source2.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, x, _value.Item3)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source3.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, x)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4> : ObservableValueBase<(T1, T2, T3, T4)>
    {
        private IValueObservable<T1> _source1;
        private IValueObservable<T2> _source2;
        private IValueObservable<T3> _source3;
        private IValueObservable<T4> _source4;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4) : base(source1.context)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
            _source4 = source4;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: x => SetValueInternal(new(x, _value.Item2, _value.Item3, _value.Item4)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source2.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, x, _value.Item3, _value.Item4)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source3.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, x, _value.Item4)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source4.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, x)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5> : ObservableValueBase<(T1, T2, T3, T4, T5)>
    {
        private IValueObservable<T1> _source1;
        private IValueObservable<T2> _source2;
        private IValueObservable<T3> _source3;
        private IValueObservable<T4> _source4;
        private IValueObservable<T5> _source5;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5) : base(source1.context)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
            _source4 = source4;
            _source5 = source5;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: x => SetValueInternal(new(x, _value.Item2, _value.Item3, _value.Item4, _value.Item5)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source2.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, x, _value.Item3, _value.Item4, _value.Item5)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source3.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, x, _value.Item4, _value.Item5)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source4.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, x, _value.Item5)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source5.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, _value.Item4, x)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5, T6> : ObservableValueBase<(T1, T2, T3, T4, T5, T6)>
    {
        private IValueObservable<T1> _source1;
        private IValueObservable<T2> _source2;
        private IValueObservable<T3> _source3;
        private IValueObservable<T4> _source4;
        private IValueObservable<T5> _source5;
        private IValueObservable<T6> _source6;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6) : base(source1.context)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
            _source4 = source4;
            _source5 = source5;
            _source6 = source6;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: x => SetValueInternal(new(x, _value.Item2, _value.Item3, _value.Item4, _value.Item5, _value.Item6)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source2.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, x, _value.Item3, _value.Item4, _value.Item5, _value.Item6)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source3.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, x, _value.Item4, _value.Item5, _value.Item6)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source4.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, x, _value.Item5, _value.Item6)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source5.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, _value.Item4, x, _value.Item6)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source6.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, _value.Item4, _value.Item5, x)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    public class CombineValueObservable<T1, T2, T3, T4, T5, T6, T7> : ObservableValueBase<(T1, T2, T3, T4, T5, T6, T7)>
    {
        private IValueObservable<T1> _source1;
        private IValueObservable<T2> _source2;
        private IValueObservable<T3> _source3;
        private IValueObservable<T4> _source4;
        private IValueObservable<T5> _source5;
        private IValueObservable<T6> _source6;
        private IValueObservable<T7> _source7;
        private IDisposable _subscriptions;

        public CombineValueObservable(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7) : base(source1.context)
        {
            _source1 = source1;
            _source2 = source2;
            _source3 = source3;
            _source4 = source4;
            _source5 = source5;
            _source6 = source6;
            _source7 = source7;
        }

        protected override void OnFirstObserverAdded()
        {
            _subscriptions = new ComposedDisposable(

                _source1.Subscribe(
                    onNext: x => SetValueInternal(new(x, _value.Item2, _value.Item3, _value.Item4, _value.Item5, _value.Item6, _value.Item7)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source2.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, x, _value.Item3, _value.Item4, _value.Item5, _value.Item6, _value.Item7)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source3.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, x, _value.Item4, _value.Item5, _value.Item6, _value.Item7)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source4.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, x, _value.Item5, _value.Item6, _value.Item7)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source5.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, _value.Item4, x, _value.Item6, _value.Item7)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source6.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, _value.Item4, _value.Item5, x, _value.Item7)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                ),

                _source7.Subscribe(
                    onNext: x => SetValueInternal(new(_value.Item1, _value.Item2, _value.Item3, _value.Item4, _value.Item5, _value.Item6, x)),
                    onError: OnError,
                    onDispose: Dispose,
                    immediate: true
                )

            );
        }

        protected override void OnLastObserverRemoved()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            SetValueInternal(default);
        }

        protected override void DisposeInternal()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}