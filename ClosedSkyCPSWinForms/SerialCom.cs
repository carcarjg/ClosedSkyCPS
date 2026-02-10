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
using System.IO.Ports;
using System.Text;

namespace ClosedSkyCPSWinForms
{
    internal class SerialCom
    {
        private static SerialPort? _serialPort;

        private static SerialDataReceivedEventHandler? _dataReceivedHandler;

        internal static bool Connect()
        {
            string selectedPort = Properties.Settings.Default.serialport;

            if (string.IsNullOrWhiteSpace(selectedPort))
            {
                MessageBox.Show(
                    "No COM port selected. Please configure the serial port in Settings.",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            try
            {
                if (_serialPort is not null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }

                _serialPort = new SerialPort
                {
                    PortName = selectedPort,
                    BaudRate = 19200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                _serialPort.Open();

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    $"Access denied to {selectedPort}. The port may be in use by another application.",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    $"Could not open {selectedPort}. {ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (ArgumentException)
            {
                MessageBox.Show(
                    $"Invalid port name: {selectedPort}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to connect to {selectedPort}: {ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        internal static void Disconnect()
        {
            if (_serialPort is not null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        internal static bool IsConnected => _serialPort is not null && _serialPort.IsOpen;

        internal static bool SendCommand(string command)
        {
            if (_serialPort is null || !_serialPort.IsOpen)
            {
                if (Connect() == false)
                {
                    MessageBox.Show(
                    "Serial port is not connected. Please connect to a COM port first.",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                    return false;
                }
            }

            try
            {
                _serialPort.WriteLine(command);
                return true;
            }
            catch (TimeoutException)
            {
                MessageBox.Show(
                    "Command transmission timed out.",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(
                    "Serial port is not in a valid state for transmission.",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to send command: {ex.Message}",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        internal static string? ReadLine()
        {
            if (_serialPort is null || !_serialPort.IsOpen)
            {
                return null;
            }

            try
            {
                return _serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static void StartDataReceived(RichTextBox debugOutput)
        {
            if (_serialPort is null || !_serialPort.IsOpen)
            {
                MessageBox.Show(
                    "Serial port is not connected. Please connect to a COM port first.",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (_dataReceivedHandler is not null)
            {
                _serialPort.DataReceived -= _dataReceivedHandler;
            }

            _dataReceivedHandler = (sender, e) =>
            {
                try
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        string data = _serialPort.ReadExisting();

                        if (debugOutput.InvokeRequired)
                        {
                            debugOutput.Invoke(() =>
                            {
                                debugOutput.AppendText(data);
                                debugOutput.ScrollToCaret();
                            });
                        }
                        else
                        {
                            debugOutput.AppendText(data);
                            debugOutput.ScrollToCaret();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (debugOutput.InvokeRequired)
                    {
                        debugOutput.Invoke(() =>
                        {
                            debugOutput.AppendText($"\n[Error: {ex.Message}]\n");
                        });
                    }
                    else
                    {
                        debugOutput.AppendText($"\n[Error: {ex.Message}]\n");
                    }
                }
            };

            _serialPort.DataReceived += _dataReceivedHandler;
        }

        internal static void PauseDataReceived()
        {
            if (_serialPort is not null && _dataReceivedHandler is not null)
            {
                _serialPort.DataReceived -= _dataReceivedHandler;
            }
        }

        internal static void ResumeDataReceived()
        {
            if (_serialPort is not null && _dataReceivedHandler is not null)
            {
                _serialPort.DataReceived += _dataReceivedHandler;
            }
        }

        internal static List<string> AtVResponse { get; private set; } = [];

        internal static bool ReadTerminalINFO(TextBox esnTextBox)
        {
            if (_serialPort is null || !_serialPort.IsOpen)
            {
                MessageBox.Show(
                    "Serial port is not connected. Please connect to a COM port first.",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            try
            {
                _serialPort.DiscardInBuffer();

                _serialPort.WriteLine("ati");
                Thread.Sleep(200);

                string? echoLine = _serialPort.ReadLine();
                string? esnResponse = _serialPort.ReadLine();

                if (esnResponse is not null)
                {
                    if (esnTextBox.InvokeRequired)
                    {
                        esnTextBox.Invoke(() => esnTextBox.Text = esnResponse.Trim());
                    }
                    else
                    {
                        esnTextBox.Text = esnResponse.Trim();
                    }
                }

                Thread.Sleep(200);
                _serialPort.DiscardInBuffer();

                _serialPort.WriteLine("at&v");
                Thread.Sleep(500);

                AtVResponse.Clear();

                DateTime startTime = DateTime.Now;
                TimeSpan timeout = TimeSpan.FromSeconds(5);
                bool firstLine = true;

                while ((DateTime.Now - startTime) < timeout)
                {
                    try
                    {
                        if (_serialPort.BytesToRead > 0)
                        {
                            string? line = _serialPort.ReadLine();
                            if (line is not null)
                            {
                                string trimmedLine = line.Trim();

                                if (firstLine && trimmedLine.Equals("at&v", StringComparison.OrdinalIgnoreCase))
                                {
                                    firstLine = false;
                                    startTime = DateTime.Now;
                                    continue;
                                }

                                firstLine = false;

                                if (trimmedLine.Equals("OK", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }

                                AtVResponse.Add(trimmedLine);
                                startTime = DateTime.Now;
                            }
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                    catch (TimeoutException)
                    {
                        break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to read terminal info: {ex.Message}",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        internal static bool SendCommandAndWaitForResponse(string command, string expectedResponse, int timeoutMs = 5000, RichTextBox? debugOutput = null)
        {
            if (_serialPort is null || !_serialPort.IsOpen)
            {
                if (Connect() == false)
                {
                    MessageBox.Show(
                    "Serial port is not connected. Please connect to a COM port first.",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                    return false;
                }
            }

            try
            {
                _serialPort.DiscardInBuffer();

                debugOutput?.Invoke(() => debugOutput.AppendText($"[RS232] Sending: {command}\n"));
                _serialPort.WriteLine(command);

                DateTime startTime = DateTime.Now;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMs);
                bool echoSkipped = false;

                while ((DateTime.Now - startTime) < timeout)
                {
                    try
                    {
                        if (_serialPort.BytesToRead > 0)
                        {
                            string? line = _serialPort.ReadLine();
                            if (line is not null)
                            {
                                string trimmedLine = line.Trim();
                                debugOutput?.Invoke(() => debugOutput.AppendText($"[RS232] Received: {trimmedLine}\n"));

                                if (!echoSkipped && trimmedLine.Equals(command.Trim(), StringComparison.OrdinalIgnoreCase))
                                {
                                    debugOutput?.Invoke(() => debugOutput.AppendText($"[RS232] Skipping echo\n"));
                                    echoSkipped = true;
                                    continue;
                                }

                                if (trimmedLine.Equals(expectedResponse, StringComparison.OrdinalIgnoreCase))
                                {
                                    debugOutput?.Invoke(() => debugOutput.AppendText($"[RS232] Got expected response: {expectedResponse}\n"));
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                }

                debugOutput?.Invoke(() => debugOutput.AppendText($"[RS232] Timeout waiting for: {expectedResponse}\n"));
                MessageBox.Show(
                    $"Timeout waiting for response: {expectedResponse}\nCommand: {command}",
                    "Communication Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            catch (Exception ex)
            {
                debugOutput?.Invoke(() => debugOutput.AppendText($"[RS232] Error: {ex.Message}\n"));
                MessageBox.Show(
                    $"Failed to send command:\n{ex.Message}",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        internal static bool NukeConfiguration(RichTextBox debugOutput)
        {
            if (_serialPort is null || !_serialPort.IsOpen)
            {
                MessageBox.Show(
                    "Serial port is not connected. Please connect to a COM port first.",
                    "Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            try
            {
                PauseDataReceived();

                debugOutput.Invoke(() => debugOutput.AppendText("[NUKE] Starting configuration erase sequence...\n"));
                debugOutput.Invoke(() => debugOutput.AppendText("[NUKE] Sending atz-1 (radio may take 15-20 seconds to respond)...\n"));

                if (!SendCommandAndWaitForResponse("atz-1", "BS", 30000, debugOutput))
                {
                    ResumeDataReceived();
                    return false;
                }

                Thread.Sleep(500);

                if (!SendCommandAndWaitForResponse("**ERASERECORDMODE9", "OK", 15000, debugOutput))
                {
                    ResumeDataReceived();
                    return false;
                }

                Thread.Sleep(500);

                debugOutput.Invoke(() => debugOutput.AppendText("[NUKE] Sending final reset command: atz9\n"));
                _serialPort.WriteLine("atz9");
                Thread.Sleep(1000);

                debugOutput.Invoke(() => debugOutput.AppendText("[NUKE] Configuration erase completed successfully!\n"));

                ResumeDataReceived();
                return true;
            }
            catch (Exception ex)
            {
                ResumeDataReceived();
                debugOutput.Invoke(() => debugOutput.AppendText($"[NUKE] Fatal error: {ex.Message}\n"));
                MessageBox.Show(
                    $"Configuration erase failed:\n{ex.Message}",
                    "Nuke Config Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
    }
}