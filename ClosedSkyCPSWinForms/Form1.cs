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
using System.Text;

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
                SendRadioCommand(cmd);
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
                if (IsFirmwareVersionAtLeast("16.0") == false)
                {
                    logsenableCHK.Visible = false;
                    radunitMON.Visible = false;
                    allcallCHK.Visible = false;
                    emrgClrCHK.Visible = false;
                    emrgscnMdCHK.Visible = false;
                    agcCHK.Visible = false;
                }
                if (IsFirmwareVersionAtLeast("20.0") == false)
                {
                    AFCenbCHK.Visible = false;
                    agcCHK.Visible = false;
                }
            }
            else
            {
                debugRTB.AppendText("[ERROR] Failed to read terminal info.\n");
            }
        }

        private Dictionary<string, string> atvSettings = new Dictionary<string, string>();

        private bool IsFirmwareVersionAtLeast(string minVersion)
        {
            if (!atvSettings.TryGetValue("REV", out string? fwVersion))
            {
                debugRTB.AppendText("[DEBUG] Firmware version not found in settings\n");
                return false;
            }

            try
            {
                string cleanVersion = fwVersion.Trim();
                debugRTB.AppendText($"[DEBUG] Checking firmware version: '{cleanVersion}' >= '{minVersion}'\n");

                string versionNumber = cleanVersion;

                if (cleanVersion.StartsWith("OTP R", StringComparison.OrdinalIgnoreCase))
                {
                    versionNumber = cleanVersion.Substring(5).Trim();
                }

                debugRTB.AppendText($"[DEBUG] Extracted version number: '{versionNumber}'\n");

                if (double.TryParse(versionNumber, out double currentVersion) &&
                    double.TryParse(minVersion, out double requiredVersion))
                {
                    bool result = currentVersion >= requiredVersion;
                    debugRTB.AppendText($"[DEBUG] Version check: {currentVersion} >= {requiredVersion} = {result}\n");
                    return result;
                }

                debugRTB.AppendText("[DEBUG] Failed to parse version numbers\n");
                return false;
            }
            catch (Exception ex)
            {
                debugRTB.AppendText($"[DEBUG] Version check exception: {ex.Message}\n");
                return false;
            }
        }

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
                        atvSettings.TryGetValue("Voice Reg", out string vrEnab);
                        if (vrEnab.TrimStart() == "1")
                        {
                            VoiceRegCHK.Checked = true;
                        }
                        else
                        {
                            VoiceRegCHK.Checked = false;
                        }
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
                        atvSettings.TryGetValue("Auto Reg", out string arEnab);
                        if (arEnab.TrimStart() == "1")
                        {
                            AutoRegCHK.Checked = true;
                        }
                        else
                        {
                            AutoRegCHK.Checked = false;
                        }
                        break;

                    case "Auto Provisioning":
                        atvSettings.TryGetValue("Auto Provisioning", out string apEnab);
                        if (apEnab.TrimStart() == "1")
                        {
                            autoprovCHK.Checked = true;
                        }
                        else
                        {
                            autoprovCHK.Checked = false;
                        }
                        break;

                    case "Auto Online Cmd":
                        break;

                    case "Secondary Reg":
                        break;

                    case "TNIC Address":
                        atvSettings.TryGetValue("TNIC Address", out string tnicaddr);
                        tnicaddrTXT.Text = tnicaddr.TrimStart();
                        break;

                    case "TNIC Port":
                        atvSettings.TryGetValue("TNIC Port", out string tnicpt);
                        tnicptTXT.Text = tnicpt.TrimStart();
                        break;

                    case "Service Provider Network ID":
                        atvSettings.TryGetValue("Service Provider Network ID", out string spnit);
                        spnetIDTXT.Text = spnit.TrimStart();
                        break;

                    case "Wide Area Service ID":
                        atvSettings.TryGetValue("Wide Area Service ID", out string wasvcid);
                        wasvcidTXT.Text = wasvcid.TrimStart();
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
                        atvSettings.TryGetValue("AGC Enable", out string agcen);
                        if (agcen.TrimStart() == "1")
                        {
                            logsenableCHK.Checked = true;
                        }
                        else
                        {
                            logsenableCHK.Checked = false;
                        }
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
                        atvSettings.TryGetValue("Disable/Enable Radio Unit Monitor", out string raduntmon);
                        if (raduntmon.TrimStart() == "1")
                        {
                            radunitMON.Checked = true;
                        }
                        else
                        {
                            radunitMON.Checked = false;
                        }
                        break;

                    case "Enables/Disables GPS during call receive":
                        break;

                    case "A/B/C Switch Mode":
                        break;

                    case "Verbosity":
                        break;

                    case "AFC Enable":
                        atvSettings.TryGetValue("AFC Enable", out string afcen);
                        if (afcen.TrimStart() == "1")
                        {
                            AFCenbCHK.Checked = true;
                        }
                        else
                        {
                            AFCenbCHK.Checked = false;
                        }
                        break;
                }
            }
        }

        private void UpdateSettingsFromUI()
        {
            debugRTB.AppendText("[DEBUG] Updating settings from UI...\n");

            // Update settings from text boxes
            if (!string.IsNullOrWhiteSpace(ipaTXT.Text))
                atvSettings["IP Address"] = ipaTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(bipaTXT.Text))
                atvSettings["Broadcast IP Address"] = bipaTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(sipaTXT.Text))
                atvSettings["Service IP Address"] = sipaTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(svcportTXT.Text))
                atvSettings["Service Port"] = svcportTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(UIDTXT.Text))
                atvSettings["User ID"] = UIDTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(txpwrTXT.Text))
                atvSettings["Transmit Power(dBm)"] = txpwrTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(tnicaddrTXT.Text))
                atvSettings["TNIC Address"] = tnicaddrTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(tnicptTXT.Text))
                atvSettings["TNIC Port"] = tnicptTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(spnetIDTXT.Text))
                atvSettings["Service Provider Network ID"] = spnetIDTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(wasvcidTXT.Text))
                atvSettings["Wide Area Service ID"] = wasvcidTXT.Text.Trim();

            if (!string.IsNullOrWhiteSpace(semerprefixTXT.Text))
                atvSettings["Silent Emergency Prefix"] = $"\"{semerprefixTXT.Text.Trim()}\"";

            // Update settings from checkboxes
            atvSettings["Voice Reg"] = VoiceRegCHK.Checked ? "1" : "0";
            atvSettings["Auto Reg"] = AutoRegCHK.Checked ? "1" : "0";
            atvSettings["Auto Provisioning"] = autoprovCHK.Checked ? "1" : "0";
            atvSettings["Emit Grant Tone"] = granttoneCHK.Checked ? "1" : "0";

            if (IsFirmwareVersionAtLeast("16.0"))
            {
                atvSettings["Enable/Disable Logging"] = logsenableCHK.Checked ? "1" : "0";
                atvSettings["Disable/Enable Radio Unit Monitor"] = radunitMON.Checked ? "1" : "0";
                atvSettings["Disable/Enable All Call"] = allcallCHK.Checked ? "1" : "0";
                atvSettings["Emergency Clearing Allowed"] = emrgClrCHK.Checked ? "1" : "0";
                atvSettings["Emergency Scan Mode"] = emrgscnMdCHK.Checked ? "1" : "0";
                atvSettings["AGC Enable"] = agcCHK.Checked ? "1" : "0";
            }

            if (IsFirmwareVersionAtLeast("20.0"))
            {
                atvSettings["AFC Enable"] = AFCenbCHK.Checked ? "1" : "0";
            }

            // Silent emergency requires special format
            atvSettings["Enable Silent Emergency"] = silentemrCHK.Checked
                ? "Saved: 1 Current: 1"
                : "Saved: 0 Current: 0";

            debugRTB.AppendText("[DEBUG] UI settings updated in dictionary.\n");
        }

        private void saveBUT_Click(object sender, EventArgs e)
        {
            if (atvSettings.Count == 0)
            {
                MessageBox.Show(
                    "No settings to save. Please read terminal info first.",
                    "Save Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "OpenSky Codeplugs (*.openskycpg)|*.openskycpg|All Files (*.*)|*.*",
                DefaultExt = "openskycpg",
                FileName = $"OpenSkySettings_{esnTXT.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.openskycpg",
                Title = "Save Radio Settings"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant();

                    if (extension == ".openskycpg")
                    {
                        ConfigurationFile.SaveEncrypted(
                            saveFileDialog.FileName,
                            esnTXT.Text,
                            atvSettings,
                            "HarrisKilledTheOpenSkyStar");

                        debugRTB.AppendText($"[DEBUG] OpenSky settings saved to: {saveFileDialog.FileName}\n");
                    }
                    else
                    {
                        using StreamWriter writer = new(saveFileDialog.FileName);

                        writer.WriteLine($"# Radio Settings Saved: {DateTime.Now}");
                        writer.WriteLine($"# ESN: {esnTXT.Text}");
                        writer.WriteLine($"# Total Settings: {atvSettings.Count}");
                        writer.WriteLine();

                        foreach (var kvp in atvSettings)
                        {
                            writer.WriteLine($"{kvp.Key}: {kvp.Value}");
                        }

                        debugRTB.AppendText($"[DEBUG] Settings saved to: {saveFileDialog.FileName}\n");
                        MessageBox.Show(
                            $"Settings saved successfully!\n\nFile: {saveFileDialog.FileName}",
                            "Save Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    debugRTB.AppendText($"[ERROR] Failed to save settings: {ex.Message}\n");
                    MessageBox.Show(
                        $"Failed to save settings:\n{ex.Message}",
                        "Save Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void loadBUT_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new()
            {
                Filter = "OpenSky Codeplugs (*.openskycpg)|*.openskycpg|All Files (*.*)|*.*",
                DefaultExt = "openskycpg",
                Title = "Load Radio Settings"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bool isEncrypted = IsEncryptedFile(openFileDialog.FileName);

                    if (isEncrypted)
                    {
                        debugRTB.AppendText($"[DEBUG] Detected encrypted OpenSky codeplug format.\n");

                        ConfigurationFile.ConfigData data = ConfigurationFile.LoadEncrypted(
                            openFileDialog.FileName,
                            "HarrisKilledTheOpenSkyStar");

                        atvSettings.Clear();
                        foreach (var kvp in data.Settings)
                        {
                            atvSettings[kvp.Key] = kvp.Value;
                        }

                        esnTXT.Text = data.ESN;
                        LoadATVIntoUI();

                        // Reset all version-dependent controls to visible
                        logsenableCHK.Visible = true;
                        radunitMON.Visible = true;
                        allcallCHK.Visible = true;
                        emrgClrCHK.Visible = true;
                        emrgscnMdCHK.Visible = true;
                        AFCenbCHK.Visible = true;
                        agcCHK.Visible = true;

                        // Hide controls based on firmware version
                        if (IsFirmwareVersionAtLeast("16.0") == false)
                        {
                            logsenableCHK.Visible = false;
                            radunitMON.Visible = false;
                            allcallCHK.Visible = false;
                            emrgClrCHK.Visible = false;
                            emrgscnMdCHK.Visible = false;
                            agcCHK.Visible = false;
                        }
                        if (IsFirmwareVersionAtLeast("20.0") == false)
                        {
                            AFCenbCHK.Visible = false;
                            agcCHK.Visible = false;
                        }
                        debugRTB.AppendText($"[DEBUG] Loaded {atvSettings.Count} encrypted settings from: {openFileDialog.FileName}\n");
                        debugRTB.AppendText($"[DEBUG] ESN: {data.ESN}\n");
                        debugRTB.AppendText($"[DEBUG] Saved: {data.SavedDate}\n");
                        debugRTB.AppendText($"[DEBUG] Checksum verified successfully.\n");

                        MessageBox.Show(
                            $"Settings loaded successfully!\n\nFile: {openFileDialog.FileName}\nESN: {data.ESN}\nSettings loaded: {atvSettings.Count}",
                            "Load Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        debugRTB.AppendText($"[DEBUG] Detected plain text file format.\n");

                        atvSettings.Clear();

                        using StreamReader reader = new(openFileDialog.FileName);
                        string? line;
                        int settingsLoaded = 0;
                        int lineNumber = 0;

                        while ((line = reader.ReadLine()) is not null)
                        {
                            lineNumber++;

                            if (lineNumber == 2 && line.StartsWith("# ESN: "))
                            {
                                string esnValue = line[7..].Trim();
                                esnTXT.Text = esnValue;
                                debugRTB.AppendText($"[DEBUG] ESN loaded: {esnValue}\n");
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                            {
                                continue;
                            }

                            int separatorIndex = line.IndexOf(": ");
                            if (separatorIndex >= 0)
                            {
                                string key = line[..separatorIndex];
                                string value = line[(separatorIndex + 2)..];
                                atvSettings[key] = value;
                                settingsLoaded++;
                            }
                        }

                        LoadATVIntoUI();

                        // Reset all version-dependent controls to visible
                        logsenableCHK.Visible = true;
                        radunitMON.Visible = true;
                        allcallCHK.Visible = true;
                        emrgClrCHK.Visible = true;
                        emrgscnMdCHK.Visible = true;
                        AFCenbCHK.Visible = true;
                        agcCHK.Visible = true;

                        // Hide controls based on firmware version
                        if (IsFirmwareVersionAtLeast("16.0") == false)
                        {
                            logsenableCHK.Visible = false;
                            radunitMON.Visible = false;
                            allcallCHK.Visible = false;
                            emrgClrCHK.Visible = false;
                            emrgscnMdCHK.Visible = false;
                            agcCHK.Visible = false;
                        }
                        if (IsFirmwareVersionAtLeast("20.0") == false)
                        {
                            AFCenbCHK.Visible = false;
                            agcCHK.Visible = false;
                        }
                        debugRTB.AppendText($"[DEBUG] Loaded {settingsLoaded} settings from: {openFileDialog.FileName}\n");
                        MessageBox.Show(
                            $"Settings loaded successfully!\n\nFile: {openFileDialog.FileName}\nSettings loaded: {settingsLoaded}",
                            "Load Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    debugRTB.AppendText($"[ERROR] Failed to load settings: {ex.Message}\n");
                    MessageBox.Show(
                        $"Failed to load settings:\n{ex.Message}",
                        "Load Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private bool IsEncryptedFile(string filePath)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
                if (fs.Length < 12)
                {
                    return false;
                }

                byte[] header = new byte[12];
                fs.Read(header, 0, 12);

                string magic = Encoding.ASCII.GetString(header);
                return magic == "OPENSKYCPGV1";
            }
            catch
            {
                return false;
            }
        }

        private void nukeconfigBUT_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "WARNING: This will ERASE ALL CONFIGURATION from the radio!\n\n" +
                "This action cannot be undone. The radio will be reset to factory defaults.\n\n" +
                "Are you absolutely sure you want to continue?",
                "Nuke Configuration - Confirmation Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                debugRTB.AppendText("[NUKE] Configuration erase cancelled by user.\n");
                return;
            }

            if (!SerialCom.IsConnected)
            {
                debugRTB.AppendText("[ERROR] Cannot nuke config. Attempting to connect...\n");
                if (!SerialCom.Connect())
                {
                    debugRTB.AppendText("[ERROR] Failed to connect. Cannot nuke configuration.\n");
                    return;
                }
            }

            debugRTB.AppendText("[NUKE] ---------------------------------------\n");
            debugRTB.AppendText("[NUKE] INITIATING CONFIGURATION ERASE...\n");
            debugRTB.AppendText("[NUKE] ---------------------------------------\n");

            if (SerialCom.NukeConfiguration(debugRTB))
            {
                debugRTB.AppendText("[NUKE] ---------------------------------------\n");
                debugRTB.AppendText("[NUKE] CONFIGURATION SUCCESSFULLY ERASED!\n");
                debugRTB.AppendText("[NUKE] ---------------------------------------\n");

                MessageBox.Show(
                    "Configuration has been successfully erased!\n\n" +
                    "The radio has been reset to factory defaults.",
                    "Nuke Configuration - Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                SerialCom.StartDataReceived(debugRTB);
                debugRTB.AppendText("[DEBUG] Auto data reception re-enabled.\n");
            }
            else
            {
                debugRTB.AppendText("[NUKE] ---------------------------------------\n");
                debugRTB.AppendText("[NUKE] CONFIGURATION ERASE FAILED!\n");
                debugRTB.AppendText("[NUKE] ---------------------------------------\n");
            }
        }

        private void WriteRadio()
        {
            debugRTB.AppendText("[WRITE] Starting radio programming...\n");
            debugRTB.AppendText("[WRITE] Pausing automatic data reception to prevent conflicts...\n");

            SerialCom.PauseDataReceived();

            try
            {
                foreach (var kvp in atvSettings)
                {
                    switch (kvp.Key)
                    {
                        case "Transmit Power(dBm)":
                            atvSettings.TryGetValue("Transmit Power(dBm)", out string txpw);
                            SendRadioCommand($"at*****{txpw}");
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
                            SendRadioCommand($@"at\s{ipa.TrimStart()}");
                            break;

                        case "Broadcast IP Address":
                            atvSettings.TryGetValue("Broadcast IP Address", out string bipa);
                            SendRadioCommand($@"at\b{bipa.TrimStart()}");
                            break;

                        case "Service IP Address":
                            atvSettings.TryGetValue("Service IP Address", out string sipa);
                            SendRadioCommand($@"at\u{sipa.TrimStart()}");
                            break;

                        case "Service Port":
                            atvSettings.TryGetValue("Service Port", out string svpt);
                            SendRadioCommand($@"at\p{svpt.TrimStart()}");
                            break;

                        case "User ID":
                            atvSettings.TryGetValue("User ID", out string uid);
                            SendRadioCommand($@"at@u{uid.TrimStart()}");
                            break;

                        case "Voice Reg":
                            atvSettings.TryGetValue("Voice Reg", out string vrEnab);
                            SendRadioCommand($@"at@vreg{(vrEnab.TrimStart() == "1" ? "1" : "0")}");
                            break;

                        case "Chan Scan Enable":
                            atvSettings.TryGetValue("Chan Scan Enable", out string chscn);
                            SendRadioCommand($@"atchanscan{(chscn.TrimStart() == "1" ? "1" : "0")}");
                            break;

                        case "Side Tone Level Code":
                            break;

                        case "Roam Tone Level Code":
                            break;

                        case "Grant Tone Level Code":
                            break;

                        case "Auto Reg":
                            atvSettings.TryGetValue("Auto Reg", out string arEnab);
                            if (arEnab.TrimStart() == "1")
                            {
                                SendRadioCommand(@"at@ar" + "" + 1);
                            }
                            else
                            {
                                SendRadioCommand(@"at@ar" + "" + 0);
                            }
                            break;

                        case "Auto Provisioning":
                            atvSettings.TryGetValue("Auto Provisioning", out string apEnab);
                            if (apEnab.TrimStart() == "1")
                            {
                                SendRadioCommand(@"at@ap" + "" + 1);
                            }
                            else
                            {
                                SendRadioCommand(@"at@ap" + "" + 0);
                            }
                            break;

                        case "Auto Online Cmd":
                            break;

                        case "Secondary Reg":
                            break;

                        case "TNIC Address":
                            atvSettings.TryGetValue("TNIC Address", out string tnicaddr);
                            SendRadioCommand(@"at\tnica" + "" + tnicaddr.TrimStart());
                            break;

                        case "TNIC Port":
                            atvSettings.TryGetValue("TNIC Port", out string tnicpt);
                            SendRadioCommand(@"at\tnicp" + "" + tnicpt.TrimStart());
                            break;

                        case "Service Provider Network ID":
                            atvSettings.TryGetValue("Service Provider Network ID", out string spnit);

                            //Do not write this.
                            //SendRadioCommand(@"at@spni" + "" + spnit.TrimStart());
                            break;

                        case "Wide Area Service ID":
                            atvSettings.TryGetValue("Wide Area Service ID", out string wasvcid);

                            //Do not write this.
                            //SendRadioCommand(@"at@wasi" + "" + wasvcid.TrimStart());
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
                                SendRadioCommand(@"at*grant_tone" + "" + 1);
                            }
                            else
                            {
                                SendRadioCommand(@"at*grant_tone" + "" + 0);
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
                                SendRadioCommand(@"at@se_enable" + "" + 1);
                            }
                            else
                            {
                                SendRadioCommand(@"at@se_enable" + "" + 0);
                            }
                            break;

                        case "Silent Emergency Prefix":
                            atvSettings.TryGetValue("Silent Emergency Prefix", out string semrpret);
                            semrpret = semrpret.Replace("\"", "");

                            //SendRadioCommand(@"se_prefix" + "" + semrpret.TrimStart());
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
                            if (IsFirmwareVersionAtLeast("16.0"))
                            {
                                //This CMD only runs on fw newer than OTP R16.0. If the radio is older just ignore it
                                atvSettings.TryGetValue("Emergency Scan Mode", out string escnmodeena);
                                if (escnmodeena.TrimStart() == "1")
                                {
                                    SendRadioCommand(@"at@emerg_scanmode" + "" + 1);
                                }
                                else
                                {
                                    SendRadioCommand(@"at@emerg_scanmode" + "" + 0);
                                }
                            }
                            else
                            {
                                debugRTB.AppendText("[WARN] Skipping Emergency Scan Mode - requires OTP R16.0 or higher\n");
                            }
                            break;

                        case "VTAC Connection Mode":
                            break;

                        case "Mandown Switch Quiet Delay":
                            break;

                        case "Mandown Switch Loud Delay":
                            break;

                        case "Emergency Clearing Allowed":

                            if (IsFirmwareVersionAtLeast("16.0"))
                            {
                                //This CMD only runs on fw newer than OTP R16.0. If the radio is older just ignore it
                                atvSettings.TryGetValue("Emergency Clearing Allowed", out string emrclren);
                                if (emrclren.TrimStart() == "1")
                                {
                                    SendRadioCommand(@"at@emerg_clear" + "" + 1);
                                }
                                else
                                {
                                    SendRadioCommand(@"at@emerg_clear" + "" + 0);
                                }
                            }
                            else
                            {
                                debugRTB.AppendText("[WARN] Skipping Emergency Clearing Allowed - requires OTP R16.0 or higher\n");
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

                            if (IsFirmwareVersionAtLeast("16.0"))
                            {
                                //This CMD only runs on fw newer than OTP R16.0. If the radio is older just ignore it
                                atvSettings.TryGetValue("Disable/Enable All Call", out string allcallen);
                                if (allcallen.TrimStart() == "1")
                                {
                                    SendRadioCommand(@"at@allcall" + "" + 1);
                                }
                                else
                                {
                                    SendRadioCommand(@"at@allcall" + "" + 0);
                                }
                            }
                            else
                            {
                                debugRTB.AppendText("[WARN] Skipping Disable/Enable All Call - requires OTP R16.0 or higher\n");
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
                            if (IsFirmwareVersionAtLeast("20.0"))
                            {
                                //This CMD only runs on fw newer than OTP R20.0. If the radio is older just ignore it
                                atvSettings.TryGetValue("AGC Enable", out string agcenab);
                                if (agcenab.TrimStart() == "1")
                                {
                                    SendRadioCommand(@"at@exths_agc_enable" + "" + 1);
                                }
                                else
                                {
                                    SendRadioCommand(@"at@exths_agc_enable" + "" + 0);
                                }
                            }
                            else
                            {
                                debugRTB.AppendText("[WARN] Skipping AGC Enable - requires OTP R20.0 or higher\n");
                            }
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
                            if (IsFirmwareVersionAtLeast("16.0"))
                            {
                                //This CMD only runs on fw newer than OTP R16.0. If the radio is older just ignore it
                                atvSettings.TryGetValue("Enable/Disable Logging", out string lgenab);
                                if (lgenab.TrimStart() == "1")
                                {
                                    SendRadioCommand(@"at@logs_enable" + "" + 1);
                                }
                                else
                                {
                                    SendRadioCommand(@"at@logs_enable" + "" + 0);
                                }
                            }
                            else
                            {
                                debugRTB.AppendText("[WARN] Skipping Enable/Disable Logging - requires OTP R16.0 or higher\n");
                            }
                            break;

                        case "Serial Flow Control":
                            break;

                        case "Voice Key Policy":
                            break;

                        case "Disable/Enable Radio Unit Monitor":
                            if (IsFirmwareVersionAtLeast("16.0"))
                            {
                                //This CMD only runs on fw newer than OTP R16.0. If the radio is older just ignore it
                                atvSettings.TryGetValue("Disable/Enable Radio Unit Monitor", out string raduntmon);
                                if (raduntmon.TrimStart() == "1")
                                {
                                    SendRadioCommand(@"at*rum_enable" + "" + 1);
                                }
                                else
                                {
                                    SendRadioCommand(@"at*rum_enable" + "" + 0);
                                }
                            }
                            else
                            {
                                debugRTB.AppendText("[WARN] Skipping Disable/Enable Radio Unit Monitor - requires OTP R16.0 or higher\n");
                            }
                            break;

                        case "Enables/Disables GPS during call receive":
                            break;

                        case "A/B/C Switch Mode":
                            break;

                        case "Verbosity":
                            break;

                        case "AFC Enable":
                            if (IsFirmwareVersionAtLeast("20.0"))
                            {
                                //This CMD only runs on fw newer than OTP R16.0. If the radio is older just ignore it
                                atvSettings.TryGetValue("AFC Enable", out string afcenab);
                            }
                            else
                            {
                                debugRTB.AppendText("[WARN] Skipping AFC Enable - requires OTP R20.0 or higher\n");
                            }
                            break;
                    }
                }

                debugRTB.AppendText("[WRITE] Saving settings to NVRAM...\n");
                SerialCom.SendCommandAndWaitForResponse("at&w", "OK", 5000, debugRTB);
                Thread.Sleep(500);
                debugRTB.AppendText("[WRITE] Resetting radio...\n");
                SerialCom.SendCommand("atz");
            }
            catch (Exception ex)
            {
                debugRTB.AppendText($"[WRITE] Error during programming: {ex.Message}\n");
            }
            finally
            {
                debugRTB.AppendText("[WRITE] Resuming automatic data reception...\n");
                SerialCom.ResumeDataReceived();
                debugRTB.AppendText("[WRITE] Radio programming completed.\n");
            }
        }

        private bool SendRadioCommand(string command)
        {
            return SerialCom.SendCommandAndWaitForResponse(command, "OK", 5000, debugRTB);
        }

        private void writeBUT_Click(object sender, EventArgs e)
        {
            // Capture UI changes before writing to radio
            UpdateSettingsFromUI();

            debugRTB.AppendText("[WRITE] Starting radio write operation...\n");
            WriteRadio();
        }
    }
}
