using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PauseMe
{
    class Utilities
    {
        public static void BeepThread(int frequency, int duration, int timesToRepeat = 1, int delay = 0)
        {
            Task.Run(() =>
                {
                    for (int i = 0; i < timesToRepeat; i++)
                    {
                        Console.Beep(frequency, duration);
                        Thread.Sleep(delay);
                    }
                }
            );
        }
    }
}
