// %%%%%%    @%%%%%@
//%%%%%%%%   %%%%%%%@
//@%%%%%%%@  %%%%%%%%%        @@      @@  @@@      @@@ @@@     @@@ @@@@@@@@@@   @@@@@@@@@
//%%%%%%%%@ @%%%%%%%%       @@@@@   @@@@ @@@@@   @@@@ @@@@   @@@@ @@@@@@@@@@@@@@@@@@@@@@@ @@@@
// @%%%%%%%%  %%%%%%%%%      @@@@@@  @@@@  @@@@  @@@@   @@@@@@@@@     @@@@    @@@@         @@@@
//  %%%%%%%%%  %%%%%%%%@     @@@@@@@ @@@@   @@@@@@@@     @@@@@@       @@@@    @@@@@@@@@@@  @@@@
//   %%%%%%%%@  %%%%%%%%%    @@@@@@@@@@@@     @@@@        @@@@@       @@@@    @@@@@@@@@@@  @@@@
//    %%%%%%%%@ @%%%%%%%%    @@@@ @@@@@@@     @@@@      @@@@@@@@      @@@@    @@@@         @@@@
//    @%%%%%%%%% @%%%%%%%%   @@@@   @@@@@     @@@@     @@@@@ @@@@@    @@@@    @@@@@@@@@@@@ @@@@@@@@@@
//     @%%%%%%%%  %%%%%%%%@  @@@@    @@@@     @@@@    @@@@     @@@@   @@@@    @@@@@@@@@@@@ @@@@@@@@@@@
//      %%%%%%%%@ @%%%%%%%%
//      @%%%%%%%%  @%%%%%%%%
//       %%%%%%%%   %%%%%%%@
//         %%%%%      %%%%
//
// Copyright (C) 2025-2026 NyxTel Wireless / Nyx Gallini
//
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ClosedSkyCPSWinForms
{
    public partial class SplashScreen : Form
    {
        private System.Windows.Forms.Timer closeTimer;

        private string gifPath;

        public SplashScreen()
        {
            InitializeComponent();
            SetupSplashScreen();
        }

        private void SetupSplashScreen()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;
            ShowInTaskbar = false;
            TopMost = true;
            string exepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string loadingGifPath = exepath + "loading.gif";
            pictureBox1.Image = Image.FromFile("loading.gif");
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.BringToFront();
            closeTimer = new System.Windows.Forms.Timer
            {
                Interval = 4670
            };
            closeTimer.Tick += CloseTimer_Tick;
            closeTimer.Start();
        }

        private void OnFrameChanged(object? sender, EventArgs e)
        {
            if (pictureBox1?.Image is not null)
            {
                ImageAnimator.UpdateFrames(pictureBox1.Image);
                pictureBox1.Invalidate();
            }
        }

        private void CloseTimer_Tick(object? sender, EventArgs e)
        {
            closeTimer?.Stop();
            closeTimer?.Dispose();

            if (pictureBox1?.Image is not null)
            {
                ImageAnimator.StopAnimate(pictureBox1.Image, OnFrameChanged);
            }

            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                closeTimer?.Dispose();
                pictureBox1?.Image?.Dispose();
                pictureBox1?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}