using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PauseMe
{
    public partial class OverlayForm : Form
    {
        #region DLLImports

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr window, int index);

        #endregion

        private int _CountDownTimer = 0;
        private readonly Action<int> _updateCountdownLabel;
        private Settings _settings;
        private KeyboardHook _kbHook = new KeyboardHook();

        public OverlayForm(Settings settings)
        {
            InitializeComponent();

            _settings = settings;

            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Opacity = 0.5;
            this.DoubleBuffered = true;

            // Making form a click-through overlay
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            // Subscribe to keyboard events
            _kbHook.KeyDown += KeyboardKeyPress;

            _updateCountdownLabel = (timer) => lblCountdown.Text = "Pause time: " + (new TimeSpan(0, 0, ((int)_settings.PauseTime.TotalSeconds) - timer)).ToShortString();
            _updateCountdownLabel(_CountDownTimer++);

            tmrCountdown.Start();
        }

        private void tmrCountdown_Tick(object sender, EventArgs e)
        {
            _updateCountdownLabel(_CountDownTimer++);

            if (_CountDownTimer > _settings.PauseTime.TotalSeconds)
            {
                // Playing countdown end sound
                if (_settings.soundEnd == true)
                {
                    Utilities.BeepThread(1000, 80, 2, 400);
                }

                _CountDownTimer = 0;
                tmrCountdown.Stop();
                this.Close();
            }
        }

        private void KeyboardKeyPress(Keys pressedKey)
        {
            if (pressedKey.ToString() == _settings.SkipKey.ToString())
            {
                this.Close();
            }
        }
    }
}
