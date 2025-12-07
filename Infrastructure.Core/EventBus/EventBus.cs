//using Org.BouncyCastle.Asn1.Cmp;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SoftwaredeveloperDotAt.Infrastructure.Core.MessageBus
//{
//    public interface IEvent
//    {
//    }

//    public interface ISubscription<T> 
//        where T : IEvent
//    {
//        Action<T> ActionHandler { get; }
//    }

//    public class Subscription<T> : ISubscription<T> 
//        where T : IEvent
//    {
//        public Action<T> ActionHandler { get; private set; }

//        public Subscription(Action<T> action)
//        {
//            ActionHandler = action;
//        }
//    }

//    public interface IEventBus
//    {
//        void Publish<T>(T @event) 
//            where T : IEvent;
//    }

//    public class EventBus : IEventBus
//    {
//        private readonly Dictionary<Type, List<object>> _observers
//            = new Dictionary<Type, List<object>>();

//        public ISubscription<T> Subscribe<T>(Action<T> callback) where T : IMessage
//        {
//            ISubscription<T> subscription = null;

//            Type messageType = typeof(T);
//            var subscriptions = _observers.ContainsKey(messageType) ?
//                _observers[messageType] : new List<object< ();

//            if (!subscriptions
//                .Select(s => s as ISubscription<T>)
//                .Any(s => s.ActionHandler == callback))
//            {
//                subscription = new Subscription<T>(callback);
//                subscriptions.Add(subscription);
//            }

//            _observers[messageType] = subscriptions;

//            return subscription;
//        }

//        public void Publish<T>(T message) where T : IMessage
//        {
//            if (message == null) throw new ArgumentNullException(nameof(message));

//            Type messageType = typeof(T);
//            if (_observers.ContainsKey(messageType))
//            {
//                var subscriptions = _observers[messageType];
//                if (subscriptions == null || subscriptions.Count == 0) return;
//                foreach (var handler in subscriptions
//                    .Select(s => s as ISubscription<T>)
//                    .Select(s => s.ActionHandler))
//                {
//                    handler?.Invoke(message);
//                }
//            }
//        }
//        public bool UnSubscribe<T>(ISubscription<T> subscription) where T : IMessage
//        {
//            bool removed = false;

//            if (subscription == null) return false;

//            Type messageType = typeof(T);
//            if (_observers.ContainsKey(messageType))
//            {
//                removed = _observers[messageType].Remove(subscription);

//                if (_observers[messageType].Count == 0)
//                    _observers.Remove(messageType);
//            }
//            return removed;
//        }
//    }
//}