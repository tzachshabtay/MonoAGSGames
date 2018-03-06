using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    // TODO: find a better name
    public interface IThisGameState
    {
        bool IsBlocking { get; }
        void RepExec(float deltaTime);
        bool OnKeyDown(KeyboardEventArgs args);
    }

    /// <summary>
    /// An attempt to make a focus switch between different game states.
    /// </summary>
    /// TODO: non static class
    /// TODO: find better name (not related to states)
    public static class GameStateManager
    {
        private static IGame _game;
        private static Stack<IThisGameState> _states = new Stack<IThisGameState>();
        private static object _lockable = new Object();

        public static IReadOnlyCollection<IThisGameState> StateStack { get => _states; }

        public static void Init(IGame game)
        {
            _game = game;
        }

        public static void Uninit()
        {
            _states.Clear();
            unsubscribe();
        }

        public static void PushState(IThisGameState state)
        {
            if (_states.Contains(state))
                return;
            // wait until possible event callback(s) end
            Task.Run(() => {
                lock(_lockable)
                {
                    _states.Push(state);
                    if (_states.Count == 1)
                        subscribeToEvents();
                }
            });
        }

        public static void PopState(IThisGameState state)
        {
            if (!_states.Contains(state))
                return;
            // wait until event callback(s) end
            Task.Run(() =>
            {
                lock (_lockable)
                {
                    IThisGameState poppedState;
                    do
                    {
                        poppedState = _states.Pop();
                    }
                    while (poppedState != state);
                    if (_states.Count == 0)
                        unsubscribe();
                }
            });
        }

        private static void subscribeToEvents()
        {
            _game.Events.OnRepeatedlyExecute.Subscribe(repExec);
            _game.Input.KeyDown.Subscribe(onKeyDown);
        }

        private static void unsubscribe()
        {
            _game.Events.OnRepeatedlyExecute.Unsubscribe(repExec);
            _game.Input.KeyDown.Unsubscribe(onKeyDown);
        }

        private static void repExec()
        {
            lock (_lockable)
            {
                // TODO: get delta time from one API, using more precise calculation
                float deltaTime = (float)(1.0 / AGSGame.UPDATE_RATE);
                foreach (var s in _states)
                {
                    s.RepExec(deltaTime);
                    if (s.IsBlocking)
                        break;
                }
            }
        }

        private static void onKeyDown(KeyboardEventArgs args)
        {
            lock (_lockable)
            {
                foreach (var s in _states)
                    if (s.OnKeyDown(args) || s.IsBlocking)
                        break;
            }
        }
    }
}
