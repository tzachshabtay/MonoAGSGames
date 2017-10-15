using System;
using System.Collections.Generic;
using AGS.API;
using AGS.Engine;

namespace Game.Utils
{
    class TweenedEntityCache<T>
        where T : class
    {
        public delegate Tween CreateTweenForEntity(T entity);
        public delegate void OnTweenCompleteHandler(T entity);

        private Queue<T> _avail = new Queue<T>();
        private LinkedList<KeyValuePair<T, Tween>> _inUse = new LinkedList<KeyValuePair<T, Tween>>();

        public TweenedEntityCache(IEvent repExecEvent)
        {
            repExecEvent.Subscribe(OnRepeatedlyExecute);
        }

        public event OnTweenCompleteHandler OnTweenComplete;

        public void Add(T e)
        {
            lock (_avail)
            {
                _avail.Enqueue(e);
            }
        }

        public T BeginUsing(CreateTweenForEntity createTween)
        {
            lock (_avail)
            {
                if (_avail.Count == 0)
                    return null;
                T e = _avail.Dequeue();
                Tween tween = createTween(e);
                if (tween == null)
                {
                    _avail.Enqueue(e); // put it back
                    return null;
                }
                _inUse.AddLast(new KeyValuePair<T, Tween>(e, tween));
                return e;
            }
        }

        private void OnRepeatedlyExecute()
        {
            lock (_avail)
            {
                var node = _inUse.First;
                while (node != null)
                {
                    var next = node.Next;
                    var e = node.Value.Key;
                    var tween = node.Value.Value;
                    if (tween.Task.IsCompleted)
                    {
                        _inUse.Remove(node);
                        OnTweenComplete(e);
                        _avail.Enqueue(e);
                    }
                    node = next;
                }
            }
        }
    }
}
