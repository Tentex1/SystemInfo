using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Diagnostics;
using System.Windows.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Http;

namespace SystemReqWPF
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        private PerformanceCounter ramCounter;

        public MainWindow()
        {
            InitializeComponent();
            DisplayRamInfo();
            DisplayOSInfo();
            DisplayDriveInfo();
            DisplayNetworkInfoAsync();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void DisplayOSInfo()
        {
            var os = Environment.OSVersion;
            string OSPlatform = $"Platform: {os.Platform}\n";
            string OSVersion = $"Version: {os.Version}\n";
            string OSBuild = $"Build: {os.VersionString}\n";
            string OSServicePack = $"Service Pack: {os.ServicePack}\n";            

            OS.Content = OSPlatform;
            OSBuildG.Content = OSBuild;
            OSVersionG.Content = OSVersion;

            string bitInfo = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            OSArchitecture.Content = $"OS Bit: {bitInfo}";
            CPUBits.Content = $"Bits: {bitInfo}";

            string processorInfo = GetProcessorInfo();
            OSProcessor.Content = $"Processor: {processorInfo}";
            CPUBrandandModel.Content = $"Processor: {processorInfo}";

            int maxClockSpeed = GetProcessorMaxClockSpeed();
            CPUMaxFrequence.Content = $"Max Clock Speed: {maxClockSpeed} MHz";

            int minClockSpeed = GetProcessorMinClockSpeed();
            CPUMinFrequence.Content = $"Min Clock Speed: {minClockSpeed} MHz";

            int coreCount = GetProcessorCoreCount();
            CPUCores.Content = $"Cores: {coreCount}";
        }

        private string GetProcessorInfo()
        {
            string processorInfo = string.Empty;

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        processorInfo = obj["Name"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                processorInfo = $"Error: {ex.Message}";
            }

            return processorInfo;
        }

        private int GetProcessorMaxClockSpeed()
        {
            int maxClockSpeed = 0;

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select MaxClockSpeed from Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        maxClockSpeed = Convert.ToInt32(obj["MaxClockSpeed"]);
                    }
                }
            }
            catch (Exception ex)
            {
                maxClockSpeed = -1; // Error value
            }

            return maxClockSpeed;
        }

        private int GetProcessorMinClockSpeed()
        {
            int minClockSpeed = 0;

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select MinClockSpeed from Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        minClockSpeed = Convert.ToInt32(obj["MinClockSpeed"]);
                    }
                }
            }
            catch (Exception ex)
            {
                minClockSpeed = -1; // Error value
            }

            return minClockSpeed;
        }

        private int GetProcessorCoreCount()
        {
            int coreCount = 0;

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select NumberOfCores from Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        coreCount = Convert.ToInt32(obj["NumberOfCores"]);
                    }
                }
            }
            catch (Exception ex)
            {
                coreCount = -1; // Error value
            }

            return coreCount;
        }

        private void DisplayRamInfo()
        {
            double totalRam = GetTotalPhysicalMemory() / (1024.0 * 1024.0);
            double availableRam = GetAvailablePhysicalMemory() / (1024.0 * 1024.0);
            TotalRAM.Content = $"Total RAM: {totalRam:F2} MB";
            FreeRAM.Content = $"Available RAM: {availableRam:F2} MB";
        }

        private ulong GetTotalPhysicalMemory()
        {
            ulong totalMemory = 0;
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalMemory = (ulong)obj["TotalVisibleMemorySize"] * 1024;
                    }
                }
            }
            catch (Exception ex)
            {
                totalMemory = 0; // Error value
            }
            return totalMemory;
        }

        private ulong GetAvailablePhysicalMemory()
        {
            ulong availableMemory = 0;
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        availableMemory = (ulong)obj["FreePhysicalMemory"] * 1024;
                    }
                }
            }
            catch (Exception ex)
            {
                availableMemory = 0; // Error value
            }
            return availableMemory;
        }

        private void DisplayDriveInfo()
        {
            string systemDrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            DriveInfo drive = new DriveInfo(systemDrive);

            string driveLetter = drive.Name;
            string driveFormat = drive.DriveFormat;
            double totalSize = drive.TotalSize / (1024.0 * 1024.0 * 1024.0); 
            double usedSize = (drive.TotalSize - drive.AvailableFreeSpace) / (1024.0 * 1024.0 * 1024.0);
            double freeSpace = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            double usagePercentage = (usedSize / totalSize) * 100;

            WindowsDisk.Content = $"Windows Installed: {driveLetter}";
            FileSystem.Content = $"File System: {driveFormat}";
            UsageDisk.Content = $"Used: {usedSize:F2} GB";
            FreeDisk.Content = $"Free: {freeSpace:F2} GB";
            DiskPercentage.Content = $"Percentage: ({usagePercentage:F2}%)";
        }

        private async void DisplayNetworkInfoAsync()
        {
            string hostName = Dns.GetHostName();
            HostName.Content = $"Host Name: {hostName}";

            string ipAddress = await GetPublicIPAddress();
            IPAddress.Content = $"IP Address: {ipAddress}";

            (ulong sent, ulong received) = GetNetworkTraffic();
            TotalSent.Content = $"Sent: {sent / (1024.0 * 1024.0):F2} MB";
            TotalReceived.Content = $"Received: {received / (1024.0 * 1024.0):F2} MB";
        }

        private async Task<string> GetPublicIPAddress()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync("https://icanhazip.com");
                    return response;
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private (ulong sent, ulong received) GetNetworkTraffic()
        {
            ulong bytesSent = 0;
            ulong bytesReceived = 0;

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
                    bytesSent += (ulong)stats.BytesSent;
                    bytesReceived += (ulong)stats.BytesReceived;
                }
            }
            return (bytesSent, bytesReceived);
        }
    }
}
