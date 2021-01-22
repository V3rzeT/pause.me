using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PauseMe
{
    public class Settings
    {
        public TimeSpan PauseTime { get; private set; }
        public TimeSpan PauseEvery { get; private set; }
        public bool soundStart { get; private set; }
        public bool soundEnd { get; private set; }

        public Settings(TimeSpan pauseEvery, TimeSpan pauseTime, bool playStartSound, bool playEndSound)
        {
            PauseTime = pauseTime;
            PauseEvery = pauseEvery;
            soundStart = playStartSound;
            soundEnd = playEndSound;
        }
    }
}
