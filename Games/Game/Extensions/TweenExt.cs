using System;
using AGS.Engine;

namespace Game
{
    public static class TweenExt
    {
        public static Tween RepeatForever(this Tween t)
        {
            t.Task.ContinueWith(a => {
                t.Stop(TweenCompletion.Rewind);
                Tween.Run(t.From, t.To, t.Setter, t.DurationInTicks / (float)AGSGame.UPDATE_RATE, t.Easing).RepeatForever(); }
            );
            return t;
        }

        public static Tween RepeatTimes(this Tween t, int times)
        {
            if (times <= 0)
                return t;
            t.Task.ContinueWith(a => {
                t.Stop(TweenCompletion.Rewind);
                Tween.Run(t.From, t.To, t.Setter, t.DurationInTicks / (float)AGSGame.UPDATE_RATE, t.Easing).RepeatTimes(--times);
            }
            );
            return t;
        }
    }
}
