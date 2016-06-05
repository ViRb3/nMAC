using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Content;
using EU.Chainfire.Libsuperuser;
using nMAC.Devices;
using Environment = Android.OS.Environment;

namespace nMAC
{
    internal static class MACFunctions
    {
        private const string UnsupportedDeviceMessage = @"Sorry, this device not supported.
Please contact me if you want to make it work!";
        internal static string MACFile;
        internal static string LocalMACFile;
        internal static string BackupMACFile;

        internal static DeviceModel Device;

        internal static async Task AssignPaths(Context context)
        {
            DeviceModel device = await DetectDevice();

            if (device == null)
            {
                Helpers.ShowCriticalError(context, UnsupportedDeviceMessage);
                return;
            }

            Device = device;
            MACFile = device.Path;
            LocalMACFile = Path.Combine(context.FilesDir.AbsolutePath, "mac.bin");
            BackupMACFile = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, ".nMAC", "wlan_mac.bin");       
        }

        private static async Task<DeviceModel> DetectDevice()
        {
            List<DeviceModel> devices = new List<DeviceModel>()
            {
                new Nexus5(),
                new Nexus5X(),
                new Samsung()
            };

            IList<string> result = null;

            foreach (DeviceModel device in devices)
            {
                string path = device.Path;

                await Task.Run(() => result = Shell.SU.Run($"cat {path}"));

                if (result == null || result.Count == 0 || string.IsNullOrWhiteSpace(result.ToString()))
                    continue;

                return device;
            }

            return null;
        }

        internal static async Task GetMACFile(Context context)
        {
            if (File.Exists(LocalMACFile))
                File.Delete(LocalMACFile);

            File.WriteAllText(LocalMACFile, string.Empty); // let user create their own properly owned file

            await Task.Run(() => Shell.SU.Run($"cat {MACFile} > {LocalMACFile}"));
        }

        internal static string ReadMAC(Context context)
        {
            byte[] content = File.ReadAllBytes(LocalMACFile);

            if (!Device.CheckFile(content))
            {
                Helpers.ShowCriticalError(context, UnsupportedDeviceMessage);
                return null;
            }

            return Device.ExtractMACFromFile(content);
        }

        internal static void WriteMAC(ref byte[] content, string MAC)
        {
            Device.WriteMAC(ref content, MAC);
        }

        internal static async Task BackupMACBinary()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BackupMACFile));

            await Task.Run(() => Shell.SU.Run($"cat {MACFile} > {BackupMACFile}"));
        }

        internal static async Task ReplaceMACFile(Context context, string source)
        {
            await Task.Run(() => Shell.SU.Run($"cat {source} > {MACFile}"));
        }
    }
}