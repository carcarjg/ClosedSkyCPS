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
            
            string gitHash = GetGitCommitHash();
            githashLAB.Text = gitHash;
            
            // Set version from VersionInfo
            label3.Text = $"Version: {VersionInfo.GetVersion()}";
            
            closeTimer = new System.Windows.Forms.Timer
            {
                Interval = 4670
            };
            closeTimer.Tick += CloseTimer_Tick;
            closeTimer.Start();
        }

        private string GetGitCommitHash()
        {
            try
            {
                string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                
                string repoPath = appDirectory;
                while (!string.IsNullOrEmpty(repoPath) && !Directory.Exists(Path.Combine(repoPath, ".git")))
                {
                    repoPath = Path.GetDirectoryName(repoPath);
                }

                if (string.IsNullOrEmpty(repoPath))
                {
                    return "No Git Repository";
                }

                System.Diagnostics.ProcessStartInfo hashInfo = new()
                {
                    FileName = "git",
                    Arguments = "rev-parse --short HEAD",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using System.Diagnostics.Process? hashProcess = System.Diagnostics.Process.Start(hashInfo);
                if (hashProcess is null)
                {
                    return "Git Not Available";
                }

                hashProcess.WaitForExit();
                
                if (hashProcess.ExitCode != 0)
                {
                    return "Git Error";
                }

                string hash = hashProcess.StandardOutput.ReadLine()?.Trim() ?? "Unknown";

                System.Diagnostics.ProcessStartInfo statusInfo = new()
                {
                    FileName = "git",
                    Arguments = "status --porcelain",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using System.Diagnostics.Process? statusProcess = System.Diagnostics.Process.Start(statusInfo);
                if (statusProcess is not null)
                {
                    statusProcess.WaitForExit();
                    
                    if (statusProcess.ExitCode == 0)
                    {
                        string statusOutput = statusProcess.StandardOutput.ReadToEnd().Trim();
                        if (!string.IsNullOrEmpty(statusOutput))
                        {
                            hash += "-dirty";
                        }
                    }
                }

                return $"Commit: {hash}";
            }
            catch
            {
                return "Git Unavailable";
            }
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