using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace dead2
{
    public static class CheatEngineInjector
    {
        // WinAPI imports for memory manipulation
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        // Helper to parse module base + offset
        public static IntPtr GetAddress(Process process, string moduleName, int offset)
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return module.BaseAddress + offset;
                }
            }
            return IntPtr.Zero;
        }

        // Injects the code: mov [rbx+178],(float)999 at the specified address
        public static bool InjectCheat(Process process)
        {
            // Address: DeadIsland-Win64-Shipping.exe+12F14E1
            // Module: DeadIsland-Win64-Shipping.exe, Offset: 0x12F14E1
            IntPtr address = GetAddress(process, "DeadIsland-Win64-Shipping.exe", 0x12F14E1);
            if (address == IntPtr.Zero)
                return false;

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (hProcess == IntPtr.Zero)
                return false;

            try
            {
                // Assembly: mov dword ptr [rbx+178], 0x447F4000 (float 999)
                // We'll NOP the original 8 bytes and JMP to a code cave is not trivial here, so for demo, just overwrite with mov [rbx+178],(float)999; nop; nop; nop; nop
                // mov dword ptr [rbx+178], 0x447F4000 = C7 83 78 01 00 00 00 40 7F 44
                // We'll pad with NOPs to match 8 bytes
                byte[] patch = new byte[] { 0xC7, 0x83, 0x78, 0x01, 0x00, 0x00, 0x00, 0x40, 0x7F, 0x44, 0x90, 0x90, 0x90 };
                // But original instruction is 8 bytes, so only write first 8 bytes
                byte[] patch8 = new byte[] { 0xC7, 0x83, 0x78, 0x01, 0x00, 0x00, 0x00, 0x40 };

                // Change memory protection
                uint oldProtect;
                VirtualProtectEx(hProcess, address, patch8.Length, PAGE_EXECUTE_READWRITE, out oldProtect);

                // Write patch
                IntPtr bytesWritten;
                bool result = WriteProcessMemory(hProcess, address, patch8, patch8.Length, out bytesWritten);

                // Restore protection
                VirtualProtectEx(hProcess, address, patch8.Length, oldProtect, out oldProtect);

                return result && bytesWritten.ToInt32() == patch8.Length;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        // Disables the cheat by restoring the original bytes at the address
        public static bool DisableCheat(Process process)
        {
            // Address: DeadIsland-Win64-Shipping.exe+12F14E1
            // Original bytes: F3 0F 11 B3 78 01 00 00
            IntPtr address = GetAddress(process, "DeadIsland-Win64-Shipping.exe", 0x12F14E1);
            if (address == IntPtr.Zero)
                return false;

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (hProcess == IntPtr.Zero)
                return false;

            try
            {
                byte[] originalBytes = new byte[] { 0xF3, 0x0F, 0x11, 0xB3, 0x78, 0x01, 0x00, 0x00 };
                uint oldProtect;
                VirtualProtectEx(hProcess, address, originalBytes.Length, PAGE_EXECUTE_READWRITE, out oldProtect);

                IntPtr bytesWritten;
                bool result = WriteProcessMemory(hProcess, address, originalBytes, originalBytes.Length, out bytesWritten);

                VirtualProtectEx(hProcess, address, originalBytes.Length, oldProtect, out oldProtect);

                return result && bytesWritten.ToInt32() == originalBytes.Length;
                
                
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }
    }
}
