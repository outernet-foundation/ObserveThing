# Welcome!

ObserveThing is an event stream library designed to make observability straightfoward and performant. In addition to single-value implementations (in C# terms, value types), ObserveThing provides observable implementations of several basic collection types (`List`, `Dictionary`, and `Set`).

## Observables

For our purposes, an `Observable` is anything which implements the `IObservable` interface or one of its variants (`IValueObservable`, `IListObservable`, `IDictionaryObservable`, etc). Each `Observable` supplies a `Subscribe` method, which takes an `IObserver` (or variant) as an argument and returns an `IDisposable` which represents that specific subscription:

```csharp
using ObserveThing;

public class Foo
{
    public IValueObservable<float> someObservable;

    public void Bar()
    {
        var subscription = someObservable.Subscribe(new ValueObserver<float>()
        {
            onNext: op => { /* this will be called with any updates to someObservable */ }
        });
    }
}
```

Disposing of this disposable removes the subscription and cleans up it's resources:

```csharp
public void Bar()
{
    ...
    subscription.Dispose(); // onNext will no longer be called
}
```

ObserveThing provides extension methods which handle constructing observers from supplied methods, reducing the complexity of hooking up an observer:

```csharp
using ObserveThing;

public class Foo
{
    public IValueObservable<float> someObservable;

    public void Bar()
    {
        var subscription = someObservable.Subscribe(
            onNext: op => { /* this will be called with any updates to someObservable */ }
        );
    }
}
```

Calling `Subscribe` on the same observable multiple times will yield several distinct subscriptions, even if the same listener methods are reused:

```csharp
using ObserveThing;

public class Foo
{
    public IValueObservable<float> someObservable;

    public void Bar()
    {
        var subscription1 = someObservable.Subscribe(onNext: HandleNext);
        var subscription2 = someObservable.Subscribe(onNext: HandleNext);
        var subscription3 = someObservable.Subscribe(onNext: HandleNext);
    }

    public void HandleNext(float value)
    {
        // This will be called three times
    }
}
```

Each `Observer` or `Subscribe` overload also provides access to `OnError` and `OnDispose` methods which can be used to further inspect the lifecycle of observables. `OnDispose` is called when either the observable (below `someObservable`) _or_ the subscription itself is disposed, but never both:

```csharp
using ObserveThing;

public class Foo
{
    public ObservableValue<float> someObservable;

    public void Bar()
    {
        someObservable = new ObservableValue<float>();

        var subscription = someObservable.Subscribe(
            onNext: x => Debug.Log("New value is "+x),
            onError: exc => Debug.LogError(exc),
            onDispose: () => Debug.Log("Observer or observable is no longer active")
        );

        subscription.Dispose(); // onDispose would be called here
        someObservable.Dispose(); // and here as well, if the subscription were still active
    }
}
```

Collections have different observer signatures than values- `onNext` is replaced by `onAdd` and `onRemove`. Lists include an `index` in `onAdd` and `onRemove` as well:

```csharp
using ObserveThing;

public class Foo
{
    public IListObservable<float> someObservable;

    public void Bar()
    {
        var subscription = someObservable.Subscribe(
            onAdd: (index, value) => { /* this will be called with any adds to someObservable */ },
            onRemove: (index, value) => { /* this will be called with any removes from someObservable */ }
        );
    }
}
```

If an observer is subscribed to an observable which already has a value or elements, the appropriate callbacks will be raised to bring the observer "up to date":

```csharp
using ObserveThing;

public class Foo
{
    public ObservableList<float> someObservable;

    public void Bar()
    {
        someObservable = new ObservableList<float>();

        someObservable.Add(1.0f);
        someObservable.Add(2.0f);
        someObservable.Add(3.0f);

        var subscription = someObservable.Subscribe(
            onAdd: (index, value) => { /* this will immediately be called with 1.0, 2.0, and 3.0 */ },
        );
    }
}
```

Strictly speaking, this is all you need to know in order to use ObserveThing. We wrote the rest of this document for fun.

## Operators

The real power of ObserveThing is comes from operators. Essentially, operators take events from one or several observable types and permute those events into another format or data type. For those familiar with LINQ, ObserverThing operators can be thought of as LINQ operators which respond to changes in observable types. The best way to understand this is by example:

```csharp
using ObserveThing;

public class Foo
{
    public IListObservable<float> list;

    public void Bar()
    {
        var subscription = list.ObservableSelect(x => x.ToString())
            .Subscribe(
                onAdd: (index, value) => Debug.Log($"Added {value} at {index}. Value type: {value.GetType().Name}"),
                onRemove: (index, value) =>Debug.Log($"Removed {value} at {index}. Value type: {value.GetType().Name}")
            );
    }
}
```

In the above code, the `onAdd` and `onRemove` methods provided in `Subscribe` will be called with the string representations in the order they appear in `list`. It is important to note that operators do not perform any work unless they have at least one subscriber. Multiple subscribers to the same operator will receive the same events, but the work will only be performed once. All operators return observables, which allows them to be chained:

```csharp
using ObserveThing;

public class Foo
{
    public IListObservable<float> list;

    public void Bar()
    {
        var subscription = list
            .ObservableSelect(x => x.ToString()) // convert to string
            .ObservableOrderBy(x => x.Length) // order by string length
            .Subscribe(
                onAdd: (index, value) => Debug.Log($"Added {value} at {index}"),
                onRemove: (index, value) => Debug.Log($"Removed {value} at {index}")
            );
    }
}
```

All operators are written to support nested observables:

```csharp
using ObserveThing;

public class Foo
{
    public class ListItem
    {
        public ObservableValue<int> priority = new ObservableValue<int>();
        public ObservableValue<string> name = new ObservableValue<string>();
    }

    public ObservableSet<ListItem> set;

    public void Bar()
    {
        var subscription = set
            .ObservableOrderBy(x => x.priority) // order by priority
            .ObservableSelect(x => x.name) // select name
            .Subscribe(
                onAdd: (index, value) => Debug.Log($"Added {value} at {index}"),
                onRemove: (index, value) => Debug.Log($"Removed {value} at {index}")
            );

        var item1 = new ListItem();
        item.priority.value = 2;
        item.name.value = "Todd";

        // Logs:
        // "Added Todd at 0"
        set.Add(item1);

        var item2 = new ListItem();
        item.priority.value = 3;
        item.name.value = "Steven";

        // Logs:
        // "Added Steven at 1"
        set.Add(item2);

        // Logs:
        // "Removed Steven at 1"
        // "Added Steven at 0"
        item2.priority.value = 1;

        // Logs:
        // "Removed Steven at 0"
        // "Added Eric at 0"
        item2.name.value = "Eric";

        // Logs:
        // "Removed Eric at 0"
        // "Removed Todd at 0"
        set.Clear();
    }
}
```
