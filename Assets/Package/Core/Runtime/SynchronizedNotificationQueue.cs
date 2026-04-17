// using System;
// using System.Collections.Generic;

// namespace ObserveThing
// {
//     public interface ISynchronizedObserver
//     {
//         bool disposed { get; }

//         void SendPendingNotifications();
//         void SendDisposeNotification();
//     }

//     public delegate void NotifyObserverDelegate<TObserver, TNotification>(TObserver observer, TNotification notification);

//     public class SynchronizedNotificationQueue<TObserver, TNotification> : IDisposable where TObserver : IObserverBase
//     {
//         private NotifyObserverDelegate<TObserver, TNotification> _notifyObserver;
//         private SynchronizationContext _context;
//         private List<ObserverData> _observers = new List<ObserverData>();
//         private List<ObserverData> _immediateObservers = new List<ObserverData>();
//         private HashSet<ObserverData> _disposedImmediateObservers = new HashSet<ObserverData>();
//         private bool _notifyingImmediateObservers = false;
//         private bool _disposed;

//         private class ObserverData : ISynchronizedObserver, IDisposable
//         {
//             public TObserver observer { get; private set; }
//             public bool disposed { get; private set; }

//             private bool _disposeSent = false;
//             private Queue<TNotification> _pendingNotifications = new Queue<TNotification>();
//             private NotifyObserverDelegate<TObserver, TNotification> _notifyObserver;
//             private Action<ObserverData> _cleanUpObserver;

//             public ObserverData(TObserver observer, NotifyObserverDelegate<TObserver, TNotification> notifyObserver, Action<ObserverData> cleanUpObserver)
//             {
//                 this.observer = observer;
//                 _notifyObserver = notifyObserver;
//                 _cleanUpObserver = cleanUpObserver;
//             }

//             public void SendPendingNotifications()
//             {
//                 if (disposed && _disposeSent)
//                     return;

//                 var count = _pendingNotifications.Count;

//                 while (count > 0)
//                 {
//                     _notifyObserver(observer, _pendingNotifications.Dequeue());
//                     count--;
//                 }

//                 if (disposed)
//                 {
//                     _disposeSent = true;
//                     observer.OnDispose();
//                 }
//             }

//             public void SendDisposeNotification()
//             {
//                 if (disposed)
//                     return;

//                 observer.OnDispose();
//             }

//             public void EnqueueNotification(TNotification notification)
//             {
//                 _pendingNotifications.Enqueue(notification);
//             }

//             public void Dispose()
//             {
//                 if (disposed)
//                     return;

//                 disposed = true;
//                 _cleanUpObserver.Invoke(this);
//             }
//         }

//         public SynchronizedNotificationQueue(NotifyObserverDelegate<TObserver, TNotification> notifyObserver, SynchronizationContext context = default)
//         {
//             _notifyObserver = notifyObserver;
//             _context = context ?? SynchronizationContext.Default;
//         }

//         private void CleanUpObserver(ObserverData observerData)
//         {
//             if (_disposed)
//                 return;

//             if (observerData.observer.immediate)
//             {
//                 _immediateObservers.Remove(observerData);
//             }
//             else
//             {
//                 _observers.Remove(observerData);
//             }

//             observerData.observer.OnDispose();
//         }

//         public IDisposable RegisterObserver(TObserver observer)
//         {
//             var data = new ObserverData(observer, _notifyObserver, CleanUpObserver);

//             if (observer.immediate)
//             {
//                 _immediateObservers.Add(data);
//             }
//             else
//             {
//                 _observers.Add(data);
//                 _context.RegisterObserver(data);
//             }

//             return data;
//         }

//         public void EnqueueNotify(TNotification notification)
//         {
//             _context.PauseExecution();

//             foreach (var observer in _observers)
//             {
//                 observer.EnqueueNotification(notification);
//                 _context.MarkDirty(observer);
//             }

//             _notifyingImmediateObservers = true;

//             int count = _immediateObservers.Count;

//             for (int i = 0; i < count; i++)
//             {
//                 var instance = _immediateObservers[i];

//                 if (instance.disposed)
//                     continue;

//                 try
//                 {
//                     _notifyObserver(instance.observer, notification);
//                 }
//                 catch (Exception exc)
//                 {
//                     UnityEngine.Debug.LogException(exc);
//                 }
//             }

//             foreach (var disposed in _disposedImmediateObservers)
//                 _immediateObservers.Remove(disposed);

//             _disposedImmediateObservers.Clear();

//             _notifyingImmediateObservers = false;

//             _context.ResumeExecution();
//         }

//         public void Dispose()
//         {
//             if (_disposed)
//                 return;

//             _disposed = true;

//             _context.PauseExecution();

//             foreach (var observer in _observers)
//             {
//                 observer.Dispose();
//                 _context.MarkDirty(observer); // calling notify after dispose calls OnDispose on the observer
//             }

//             for (int i = 0; i < _immediateObservers.Count; i++)
//             {
//                 var instance = _immediateObservers[i];

//                 if (instance.disposed)
//                     continue;

//                 try
//                 {
//                     instance.observer.OnDispose();
//                 }
//                 catch (Exception exc)
//                 {
//                     UnityEngine.Debug.LogException(exc);
//                 }
//             }

//             _context.ResumeExecution();
//         }
//     }
// }