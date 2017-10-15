using System;
using System.Collections.Concurrent;
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

        private ConcurrentQueue<T> _avail = new ConcurrentQueue<T>();
        private LinkedList<(T entity, Tween tween)> _inUse = new LinkedList<(T, Tween)>();

        public TweenedEntityCache(IEvent repExecEvent)
        {
            repExecEvent.Subscribe(OnRepeatedlyExecute);
        }

        public event OnTweenCompleteHandler OnTweenComplete;

        public void Add(T e)
        {
            _avail.Enqueue(e);
        }

        public T BeginUsing(CreateTweenForEntity createTween)
        {
            if (!_avail.TryDequeue(out T e))
                return null;
            Tween tween = createTween(e);
            if (tween == null)
            {
                _avail.Enqueue(e); // put it back
                return null;
            }
            _inUse.AddLast((e, tween));
            return e;
        }

        private void OnRepeatedlyExecute()
        {
            var node = _inUse.First;
            while (node != null)
            {
                var next = node.Next;
                var (e, tween) = node.Value;
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
