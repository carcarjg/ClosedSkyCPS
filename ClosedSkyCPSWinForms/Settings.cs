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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace ClosedSkyCPSWinForms
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            LoadComPorts();
        }

        private void LoadComPorts()
        {
            serialportCMB.Items.Clear();
            
            string[] ports = SerialPort.GetPortNames();
            
            if (ports.Length > 0)
            {
                serialportCMB.Items.AddRange(ports);
                
                string savedPort = Properties.Settings.Default.serialport;
                if (!string.IsNullOrEmpty(savedPort) && serialportCMB.Items.Contains(savedPort))
                {
                    serialportCMB.SelectedItem = savedPort;
                }
                else if (serialportCMB.Items.Count > 0)
                {
                    serialportCMB.SelectedIndex = 0;
                }
            }
        }

        private void saveBUT_Click(object sender, EventArgs e)
        {
            if (serialportCMB.SelectedItem is not null)
            {
                Properties.Settings.Default.serialport = serialportCMB.SelectedItem.ToString();
            }
            
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}