﻿/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" (Revision 42):
 * <peter@pratscher.org> wrote this file.  As long as you retain this notice you
 * can do whatever you want with this stuff. If we meet some day, and you think
 * this stuff is worth it, you can buy me a beer in return.   Poul-Henning Kamp
 * ----------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PauseMe
{
    public partial class MainForm : Form
    {
        private int _CountDownTimer = 0;
        private DateTime _TimerStarted;
        List<OverlayForm> _OpenForms = new List<OverlayForm>();
        private Settings _settings;

        public MainForm(Settings settings)
        {
            InitializeComponent();

            _settings = settings;

            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Opacity = 0.5;
            this.WindowState = FormWindowState.Maximized;
            this.tmrMain.Interval = (int)_settings.PauseEvery.TotalMilliseconds;

            tbxStatus.Text = "Stopped";
        }

        private void tmrCountdown_Tick(object sender, EventArgs e)
        {
            if (_CountDownTimer > _settings.PauseTime.TotalSeconds)
            {
                _CountDownTimer = 0;
                tmrCountdown.Stop();
                this.Hide();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Disable start button and enable pause button
            cmsMain.Items[2].Enabled = false;
            cmsMain.Items[3].Enabled = true;

            frm_Restart();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Disable pause button and enable start button
            cmsMain.Items[3].Enabled = false;
            cmsMain.Items[2].Enabled = true;

            tmrMain.Stop();
            tmrUpdateStatus.Stop();
            tbxStatus.Text = "Stopped";
            niMain.Text = "Pause Me - Stopped";
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
                startToolStripMenuItem_Click(this, null);
                niMain.ShowBalloonTip(5000, "Pause Me Started", $"Pause Me has been started and will gently remind you every {_settings.PauseEvery.TotalMinutes} minutes to rest your eyes!", ToolTipIcon.Info);
            }));
        }

        private void tmrMain_Tick(object sender, EventArgs e)
        {
            _OpenForms.Clear();
            tmrMain.Stop();
            tmrUpdateStatus.Stop();

            // Playing countdown start sound
            if (_settings.soundStart == true)
            {
                Task.Run(() =>
                    {
                        Utilities.BeepThread(1000, 50, 5, 20);
                        Thread.Sleep(900);
                        Utilities.BeepThread(1000, 50, 5, 20);
                    }
                );
            }

            foreach (var screen in Screen.AllScreens)
            {
                var frm = new OverlayForm(_settings);
                _OpenForms.Add(frm);
                frm.FormClosed += frm_FormClosed;
                frm.Left = screen.Bounds.Left;
                frm.Top = screen.Bounds.Top;
                frm.Size = new Size(screen.Bounds.Width, screen.Bounds.Height / 14);
                frm.StartPosition = FormStartPosition.Manual;

                frm.Show(); 
            }
        }

        void frm_FormClosed(object sender, FormClosedEventArgs e)
        {
            frm_Restart();
        }

        private void frm_Restart()
        {
            foreach (var frm in _OpenForms.ToArray())
            {
                frm.FormClosed -= frm_FormClosed;
                frm.Close();
            }

            tmrMain.Stop();     // Stop() is a hack to reset timer in case it's not stopped
            tmrMain.Start();
            _TimerStarted = DateTime.Now;
            tmrUpdateStatus.Stop();
            tmrUpdateStatus.Start();
        }

        private void lblCountdown_DoubleClick(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void tmrUpdateStatus_Tick(object sender, EventArgs e)
        {
            var timePassed = (DateTime.Now - (_TimerStarted.AddMinutes(_settings.PauseEvery.TotalMinutes))).Negate();
            var minutesPassed = timePassed.Minutes.ToString("00");
            var secondsPassed = timePassed.Seconds.ToString("00");

            tbxStatus.Text = "Running (" + minutesPassed + ":" + secondsPassed + ")";
            niMain.Text = "Pause Me - Running (" + minutesPassed + ":" + secondsPassed + ")";
        }

        void SessionSwitchHandler(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                tmrMain.Stop();
                tmrUpdateStatus.Stop();
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                if (tbxStatus.Text != "Stopped")
                {
                    frm_Restart();
                }
                // else preserve paused state
            }
        }
    }
}
