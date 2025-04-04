using System.Runtime.InteropServices;
using System.Text;
using System.Management;

namespace Infrabot.WorkerService.Utils
{
    public static class HardwareInfo
    {
        private static object _linuxMemoryLock = new();
        private static char[] _arrayForMemInfoRead = new char[200];
        private static object _winMemoryLock = new();

        /// <summary>
        /// Retrieves the available and total RAM in gigabytes.
        /// </summary>
        public static void GetRamGB(out ulong available, out ulong total)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                GetBytesCountOnLinux(out available, out total);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GetBytesCountOnWindows(out available, out total);
            }
            else
            {
                throw new NotImplementedException("Not implemented for OS: " + Environment.OSVersion);
            }
        }

        /// <summary>
        /// Retrieves the current CPU usage percentage on Windows.
        /// </summary>
        public static int GetCPUUsage()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
                ManagementObjectCollection collection = searcher.Get();
                ManagementObject queryObj = collection.Cast<ManagementObject>().First();
                return 100 - Convert.ToInt32(queryObj["PercentIdleTime"]);
            }
            catch (ManagementException ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Helper method to get RAM stats on Linux.
        /// </summary>
        private static void GetBytesCountOnLinux(out ulong availableBytes, out ulong totalBytes)
        {
            lock (_linuxMemoryLock)
            {
                totalBytes = GetBytesCountFromLinuxMemInfo("MemTotal:", true);
                availableBytes = GetBytesCountFromLinuxMemInfo("MemAvailable:", false);

                availableBytes = availableBytes / 1024 / 1024 / 1024;
                totalBytes = totalBytes / 1024 / 1024 / 1024;
            }
        }

        /// <summary>
        /// Parses /proc/meminfo for a specific token (e.g., "MemTotal:").
        /// </summary>
        private static ulong GetBytesCountFromLinuxMemInfo(string token, bool refreshFromFile)
        {
            var readSpan = _arrayForMemInfoRead.AsSpan();

            if (refreshFromFile)
            {
                using var fileStream = new FileStream("/proc/meminfo", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream, Encoding.UTF8, leaveOpen: true);
                reader.ReadBlock(readSpan);
            }

            var tokenIndex = readSpan.IndexOf(token);
            if (tokenIndex == -1)
                throw new InvalidOperationException($"Token '{token}' not found in /proc/meminfo.");

            var fromTokenSpan = readSpan.Slice(tokenIndex + token.Length);
            var kbIndex = fromTokenSpan.IndexOf("kB");
            var notTrimmedSpan = fromTokenSpan.Slice(0, kbIndex);
            var trimmedSpan = notTrimmedSpan.Trim(' ');
            var kBytesCount = ulong.Parse(trimmedSpan);
            var bytesCount = kBytesCount * 1024;

            return bytesCount;
        }

        /// <summary>
        /// Helper method to get RAM stats on Windows.
        /// </summary>
        private static void GetBytesCountOnWindows(out ulong availableBytes, out ulong totalBytes)
        {
            lock (_winMemoryLock)
            {
                MEMORYSTATUSEX _memStatus = new MEMORYSTATUSEX(); ;

                if (!GlobalMemoryStatusEx(_memStatus))
                    throw new InvalidOperationException("Failed to retrieve memory status.");

                availableBytes = _memStatus.ullAvailPhys / 1024 / 1024 / 1024;
                totalBytes = _memStatus.ullTotalPhys / 1024 / 1024 / 1024;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }
}
