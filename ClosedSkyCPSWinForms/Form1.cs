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
using System.Diagnostics;
using System.Net.Mail;

namespace ClosedSkyCPSWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        internal bool Connect()
        {
            debugRTB.AppendText("[DEBUG] Attempting to connect to serial port..." + Properties.Settings.Default + "\n");
            if (SerialCom.Connect())
            {
                debugRTB.AppendText("[DEBUG] Successfully connected to serial port.\n");
                SerialCom.StartDataReceived(debugRTB);
                return true;
            }
            else
            {
                debugRTB.AppendText("[ERROR] Failed to connect to serial port.\n");
                return false;
            }
        }

        internal bool SendCMD(string cmd)
        {
            debugRTB.AppendText($"[DEBUG] Sending command: {cmd}\n");
            if (SerialCom.IsConnected)
            {
                SerialCom.SendCommand(cmd);
                debugRTB.AppendText("[DEBUG] Command sent successfully.\n");
                return true;
            }
            else
            {
                debugRTB.AppendText("[ERROR] Cannot send command. Serial port is not connected.\n");
            }
            return false;
        }

        private void ProgSetBUT_Click(object sender, EventArgs e)
        {
            Settings SF = new Settings();
            SF.ShowDialog();
        }

        private void readinfoBUT_Click(object sender, EventArgs e)
        {
            if (!SerialCom.IsConnected)
            {
                debugRTB.AppendText("[ERROR] Cannot read terminal info. Attempting to connect...\n");
                if (SerialCom.Connect())
                {
                    debugRTB.AppendText("[DEBUG] Successfully connected to serial port.\n");
                }
                else
                {
                    debugRTB.AppendText("[ERROR] Failed to connect. Cannot read terminal info.\n");
                    return;
                }
            }

            debugRTB.AppendText("[DEBUG] Reading terminal information...\n");

            if (SerialCom.ReadTerminalINFO(esnTXT))
            {
                debugRTB.AppendText("[DEBUG] Terminal info read successfully.\n");
                debugRTB.AppendText($"[DEBUG] ESN: {esnTXT.Text}\n");
                debugRTB.AppendText($"[DEBUG] AT&V Response ({SerialCom.AtVResponse.Count} lines):\n");

                foreach (string line in SerialCom.AtVResponse)
                {
                    debugRTB.AppendText($"  {line}\n");
                }

                SerialCom.StartDataReceived(debugRTB);
                debugRTB.AppendText("[DEBUG] Auto data reception enabled.\n");
                foreach (string line in SerialCom.AtVResponse)
                {
                    ATVparse(line);
                }
                LoadATVIntoUI();
            }
            else
            {
                debugRTB.AppendText("[ERROR] Failed to read terminal info.\n");
            }
        }

        private Dictionary<string, string> atvSettings = new Dictionary<string, string>();

        private void ATVparse(string line)
        {
            int index = line.IndexOf(": ");
            if (index >= 0)
            {
                string key = line.Substring(0, index);
                string value = line.Substring(index + 2);
                atvSettings.TryAdd(key, value);
            }
        }

        private void LoadATVIntoUI()
        {
            foreach (var kvp in atvSettings)
            {
                switch (kvp.Key)
                {
                    case "REV":
                        atvSettings.TryGetValue("REV", out string rev);
                        fwrelTXT.Text = rev.TrimStart();
                        break;

                    case "Transmit Power(dBm)":
                        atvSettings.TryGetValue("Transmit Power(dBm)", out string txpw);
                        txpwrTXT.Text = txpw.TrimStart();
                        break;

                    case "VCO Cal Value(DAC)":
                        break;

                    case "Voice Gain":
                        break;

                    case "Sync Loss Report Mark":
                        break;

                    case "Shutdown Timer(Min)":
                        break;

                    case "IP Address":
                        atvSettings.TryGetValue("IP Address", out string ipa);
                        ipaTXT.Text = ipa.TrimStart();
                        break;

                    case "Broadcast IP Address":
                        atvSettings.TryGetValue("Broadcast IP Address", out string bipa);
                        bipaTXT.Text = bipa.TrimStart();
                        break;

                    case "Service IP Address":
                        atvSettings.TryGetValue("Service IP Address", out string sipa);
                        sipaTXT.Text = sipa.TrimStart();
                        break;

                    case "Service Port":
                        atvSettings.TryGetValue("Service Port", out string svpt);
                        svcportTXT.Text = svpt.TrimStart();
                        break;

                    case "User ID":
                        atvSettings.TryGetValue("User ID", out string uid);
                        UIDTXT.Text = uid.TrimStart();
                        break;

                    case "Voice Reg":
                        break;

                    case "Chan Scan Enable":
                        break;

                    case "Side Tone Level Code":
                        break;

                    case "Roam Tone Level Code":
                        break;

                    case "Grant Tone Level Code":
                        break;

                    case "Auto Reg":
                        break;

                    case "Auto Provisioning":
                        break;

                    case "Auto Online Cmd":
                        break;

                    case "Secondary Reg":
                        break;

                    case "TNIC Address":
                        break;

                    case "TNIC Port":
                        break;

                    case "Service Provider Network ID":
                        break;

                    case "Wide Area Service ID":
                        break;

                    case "Use DTR/DSR":
                        break;

                    case "Audio Output":
                        break;

                    case "Scan Mode":
                        break;

                    case "Post-Queue PTT Timer(Sec)":
                        break;

                    case "Emergency Tx Period(Sec)":
                        break;

                    case "Emergency Button Raise Delay(ms)":
                        break;

                    case "Save/Restore User Selected Profile":
                        break;

                    case "Alert Messages":
                        break;

                    case "Modem Escape Char":
                        break;

                    case "Voice Scale In":
                        break;

                    case "Voice Scale Out":
                        break;

                    case "Encrypt Data":
                        break;

                    case "Emit Grant Tone":
                        atvSettings.TryGetValue("Emit Grant Tone", out string egtenab);
                        if (egtenab.TrimStart() == "1")
                        {
                            granttoneCHK.Checked = true;
                        }
                        else
                        {
                            granttoneCHK.Checked = false;
                        }
                        break;

                    case "Emergency Tone Level":
                        break;

                    case "Password Entry Type":
                        break;

                    case "Auto Lock Status":
                        break;

                    case "Emergency Dismiss Timer (min)":
                        break;

                    case "Deviation":
                        break;

                    case "User Mode":
                        break;

                    case "Contrast":
                        break;

                    case "Scan Mode Mask":
                        break;

                    case "Enable Silent Emergency":
                        atvSettings.TryGetValue("Enable Silent Emergency", out string semrclren);
                        semrclren = semrclren.Substring(8);
                        if (semrclren.TrimStart() == "Saved: 1")
                        {
                            silentemrCHK.Checked = true;
                        }
                        else
                        {
                            silentemrCHK.Checked = false;
                        }
                        break;

                    case "Silent Emergency Prefix":
                        atvSettings.TryGetValue("Silent Emergency Prefix", out string semrpret);
                        semrpret = semrpret.Replace("\"", "");

                        semerprefixTXT.Text = semrpret.TrimStart();
                        break;

                    case "Display Global Profile VGs":
                        break;

                    case "Wide Area Communications Network ID":
                        break;

                    case "VNIC Protocol Range":
                        break;

                    case "Radio Appears Disabled":
                        break;

                    case "VNIC Security Policy":
                        break;

                    case "Out Of Range Tone Interval":
                        break;

                    case "Emergency Scan Mode":
                        atvSettings.TryGetValue("Emergency Scan Mode", out string escnmodeena);
                        if (escnmodeena.TrimStart() == "1")
                        {
                            emrgscnMdCHK.Checked = true;
                        }
                        else
                        {
                            emrgscnMdCHK.Checked = false;
                        }
                        break;

                    case "VTAC Connection Mode":
                        break;

                    case "Mandown Switch Quiet Delay":
                        break;

                    case "Mandown Switch Loud Delay":
                        break;

                    case "Emergency Clearing Allowed":
                        atvSettings.TryGetValue("Emergency Clearing Allowed", out string emrclren);
                        if (emrclren.TrimStart() == "1")
                        {
                            emrgClrCHK.Checked = true;
                        }
                        else
                        {
                            emrgClrCHK.Checked = false;
                        }
                        break;

                    case "Out Of Range Tone Level Code":
                        break;

                    case "Save/Restore User Settings":
                        break;

                    case "Enabled Client Modes":
                        break;

                    case "Announcement TG Profile":
                        break;

                    case "SOI Personality":
                        break;

                    case "Disable/Enable All Call":
                        atvSettings.TryGetValue("Disable/Enable All Call", out string allcallen);
                        if (allcallen.TrimStart() == "1")
                        {
                            allcallCHK.Checked = true;
                        }
                        else
                        {
                            allcallCHK.Checked = false;
                        }
                        break;

                    case "Scan Priority Mode":
                        break;

                    case "Primary Mic Sensitivity":
                        break;

                    case "Aux Mic Sensitivity":
                        break;

                    case "Input Audio Limiter":
                        break;

                    case "Output Audio Limiter":
                        break;

                    case "Noise Gate":
                        break;

                    case "ID Request Timer (0.1s increments)":
                        break;

                    case "Vocoder Mode":
                        break;

                    case "Enable Low Pass Filter":
                        break;

                    case "Enable AMBE Noise Sup":
                        break;

                    case "AMBE Noise Sup Level":
                        break;

                    case "Enable AMBE Tone Detection":
                        break;

                    case "Audio Record":
                        break;

                    case "MDP Type":
                        break;

                    case "Enabled Keypad Easy Buttons":
                        break;

                    case "AGC Enable":
                        break;

                    case "AGC Max Gain (dB)":
                        break;

                    case "AGC Decay (ms)":
                        break;

                    case "AGC Target Level (dBFS)":
                        break;

                    case "AGC Min Gain (dB)":
                        break;

                    case "DSP Config":
                        break;

                    case "Select Output Volume Table":
                        break;

                    case "Minimum Volume":
                        break;

                    case "ADC High Pass Filter Config (Hz)":
                        break;

                    case "Enable/Disable Logging":
                        atvSettings.TryGetValue("Enable/Disable Logging", out string lgenab);
                        if (lgenab.TrimStart() == "1")
                        {
                            logsenableCHK.Checked = true;
                        }
                        else
                        {
                            logsenableCHK.Checked = false;
                        }
                        break;

                    case "Serial Flow Control":
                        break;

                    case "Voice Key Policy":
                        break;

                    case "Disable/Enable Radio Unit Monitor":
                        break;

                    case "Enables/Disables GPS during call receive":
                        break;

                    case "A/B/C Switch Mode":
                        break;

                    case "Verbosity":
                        break;
                }
            }
        }

        private void saveBUT_Click(object sender, EventArgs e)
        {
        }

        private void loadBUT_Click(object sender, EventArgs e)
        {
        }
    }
}