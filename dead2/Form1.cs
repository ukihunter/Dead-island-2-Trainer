using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace dead2
{
    public partial class Form1 : Form
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        private IntPtr processHandle = IntPtr.Zero;
        private IntPtr baseAddress = IntPtr.Zero;
        private IntPtr injectionAddress = IntPtr.Zero;
        private IntPtr caveAddress = IntPtr.Zero;

        private bool isCheatEnabled = false;
        private bool isInjected = false;
        private Timer flashTimer;
        private bool isFlashing = false;

        private bool isGodModeEnabled = false;
        private bool isDurabilityEnabled = false;
        private bool isStaminaEnabled = false;
        private bool isMoneyEnabled = false;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Text = "OFF";
            button2.Text = "OFF";
            button3.Text = "OFF";
            button4.Text = "OFF";
            button5.Text = "OFF";
            label2.Text = "Cheat DISABLED";

            try
            {
                Process[] processes = Process.GetProcessesByName("DeadIsland-Win64-Shipping");
                if (processes.Length > 0)
                {
                    Process process = processes[0];
                    baseAddress = process.MainModule.BaseAddress;

                    injectionAddress = IntPtr.Add(baseAddress, 0x12F14E1); // example address
                    label1.Text = baseAddress.ToString("X");
                    label9.Text = "DeadIsland - Win64 - Shipping.exe";

                    processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
                }
                else
                {
                    label1.Text = "Process not found.";
                    label9.Text = "Run the Game!!!";

                    if (flashTimer == null)
                    {
                        flashTimer = new Timer();
                        flashTimer.Interval = 500;
                        flashTimer.Tick += FlashTimer_Tick;
                    }

                    if (!isFlashing)
                    {
                        flashTimer.Start();
                        isFlashing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                label1.Text = "Error: " + ex.Message;
            }
        }

        //gode mode

        private void button1_Click(object sender, EventArgs e)
        {
            if (processHandle == IntPtr.Zero || injectionAddress == IntPtr.Zero)
            {
                MessageBox.Show("Game process not Found");
                return;
            }

            byte[] bytesToWrite;
            string status;

            if (!isCheatEnabled)
            {
                bytesToWrite = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
                status = "Cheat ENABLED";
                button1.Text = "ON";
            }
            else
            {
                bytesToWrite = new byte[] { 0xF3, 0x0F, 0x11, 0xB3, 0x78, 0x01, 0x00, 0x00 };
                status = "Cheat DISABLED";
                button1.Text = "OFF";
            }

            if (WriteProcessMemory(processHandle, injectionAddress, bytesToWrite, bytesToWrite.Length, out IntPtr _))
            {
                label2.Text = status;
                isCheatEnabled = !isCheatEnabled;
            }
            else
            {
                MessageBox.Show("Failed to write memory.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (processHandle == IntPtr.Zero || baseAddress == IntPtr.Zero)
            {
                MessageBox.Show("Game process not found.");
                return;
            }

            IntPtr durabilityAddress = IntPtr.Add(baseAddress, 0x16C443D); // Confirm this is the right place

            // Patch to always set durability to 100.0 (float 42C80000)
            byte[] patchBytes = new byte[] { 0xC7, 0x87, 0x90, 0x0B, 0x00, 0x00, 0x00, 0x00, 0xC8, 0x42 }; // mov [rdi+0xB90], 100.0

            // Original game instruction that decreases durability (example)
            byte[] originalBytes = new byte[] { 0xF3, 0x0F, 0x11, 0xB7, 0x90, 0x0B, 0x00, 0x00 };

            if (!isDurabilityEnabled)
            {
                if (WriteProcessMemory(processHandle, durabilityAddress, patchBytes, patchBytes.Length, out _))
                {
                    label2.Text = "Infinite Weapon Durability ENABLED";
                    button2.Text = "ON";
                    isDurabilityEnabled = true;
                }
                else
                {
                    MessageBox.Show("Failed to write memory.");
                }
            }
            else
            {
                if (WriteProcessMemory(processHandle, durabilityAddress, originalBytes, originalBytes.Length, out _))
                {
                    label2.Text = "Infinite Weapon Durability DISABLED";
                    button2.Text = "OFF";
                    isDurabilityEnabled = false;
                }
                else
                {
                    MessageBox.Show("Failed to restore memory.");
                }
            }

        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            label9.Visible = !label9.Visible;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
            }
            base.OnFormClosing(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void label6_Click(object sender, EventArgs e) => Close();
        private void label11_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;


        //stamina
        private void button3_Click(object sender, EventArgs e)
        {
            if (processHandle == IntPtr.Zero)
            {
                MessageBox.Show("Game process not found.");
                return;
            }

            // Address: DeadIsland-Win64-Shipping.exe + 0x162D968
            IntPtr staminaAddress = IntPtr.Add(baseAddress, 0x162D968);

            byte[] nopBytes = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }; // NOP the movss

            // Toggle logic
            if (!isCheatEnabled)
            {
                if (WriteProcessMemory(processHandle, staminaAddress, nopBytes, nopBytes.Length, out _))
                {
                    label2.Text = "Infinite Stamina ENABLED";
                    button3.Text = "ON";
                    isCheatEnabled = true;
                }
                else
                {
                    MessageBox.Show("Failed to write memory.");
                }
            }
            else
            {
                // Restore original bytes: F3 0F 11 93 14 01 00 00
                byte[] originalBytes = new byte[] { 0xF3, 0x0F, 0x11, 0x93, 0x14, 0x01, 0x00, 0x00 };

                if (WriteProcessMemory(processHandle, staminaAddress, originalBytes, originalBytes.Length, out _))
                {
                    label2.Text = "Infinite Stamina DISABLED";
                    button3.Text = "OFF";
                    isCheatEnabled = false;
                }
                else
                {
                    MessageBox.Show("Failed to restore memory.");
                }
            }
        }
        //inf money and items
        private void button4_Click(object sender, EventArgs e)
        {

            if (processHandle == IntPtr.Zero || baseAddress == IntPtr.Zero)
            {
                MessageBox.Show("Game process not found.");
                return;
            }

            // Target address = base address + offset
            IntPtr moneyAddress = IntPtr.Add(baseAddress, 0x4065891);

            // mov dword ptr [rdi+20], 7FFFFFFF
            byte[] patchBytes = new byte[] { 0xC7, 0x47, 0x20, 0xFF, 0xFF, 0xFF, 0x7F };

            // Original instruction (optional if you want to disable later): 44 01 67 20
            byte[] originalBytes = new byte[] { 0x44, 0x01, 0x67, 0x20 };

            if (!isCheatEnabled)
            {
                if (WriteProcessMemory(processHandle, moneyAddress, patchBytes, patchBytes.Length, out _))
                {
                    label2.Text = "Infinite Money ENABLED";
                    button4.Text = "ON";
                    isCheatEnabled = true;
                }
                else
                {
                    MessageBox.Show("Failed to write memory.");
                }
            }
            else
            {
                if (WriteProcessMemory(processHandle, moneyAddress, originalBytes, originalBytes.Length, out _))
                {
                    label2.Text = "Infinite Money DISABLED";
                    button4.Text = "OFF";
                    isCheatEnabled = false;
                }
                else
                {
                    MessageBox.Show("Failed to restore memory.");
                }
            }

        }

        //inf Exp
        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}
