# Welcome!

ObserveThing is an event stream library designed to make observability straightfoward and performant. In addition to single-value implementations (in C# terms, value types), ObserveThing provides observable implementations of several basic collection types (`List`, `Dictionary`, and `Set`).

### Observables

For our purposes, an `Observable` is anything which implements the `IObservable` interface or one of its variants (`IValueObservable`, `IListObservable`, `IDictionaryObservable`, etc). Each `Observable` supplies a `Subscribe` method, which takes an `IObserver` (or variant) as an argument and returns an `IDisposable` which represents that specific subscription:

```csharp
using ObserveThing;

public class Foo
{
    public IObservable<IOperation> someObservable;

    public void Bar()
    {
        var subscription = someObservable.Subscribe(new Observer()
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
    public IObservable<IOperation> someObservable;

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
    public IObservable<IOperation> someObservable;

    public void Bar()
    {
        var subscription = someObservable.Subscribe(
            onNext: op => { /* this will be called with any updates to someObservable */ }
        );
    }

    public void 
}
```
Strictly speaking, this is all you need to know in order to use ObserveThing. We wrote the rest of this document for fun.
### IObserver

### ObservationContext
