using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using EU.Chainfire.Libsuperuser;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;

namespace nMAC
{
    public static class MACFunctions
    {
        public static string LocalMACFile;
        public static string TempLocalMACFile;
        public static string MACFile;
        public static string BackupMACFile;

        public static void AssignPaths(Context context)
        {
            LocalMACFile = Path.Combine(context.FilesDir.AbsolutePath, "wlan_mac.bin");
            TempLocalMACFile = Path.Combine(Path.GetDirectoryName(LocalMACFile), "tmp", "wlan_mac.bin");
            MACFile = "/persist/wlan_mac.bin";
            BackupMACFile = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, ".nMAC", "wlan_mac.bin");
        }

        public static async Task GetMACFile(Context context)
        {
            if (File.Exists(LocalMACFile))
                File.Delete(LocalMACFile);

            var pm = context.PackageManager;

            var apps = pm.GetInstalledApplications(PackageInfoFlags.MetaData);

            int uid = 0;

            foreach (var app in apps)
                if (app.PackageName == context.ApplicationInfo.PackageName)
                {
                    uid = app.Uid;
                    break;
                }

            IList<string> commands = new List<string>()
            {
                $"cp {MACFile} /{LocalMACFile}",
                $"chmod 600 {LocalMACFile}",
                $"chown {uid}.{uid} {LocalMACFile}",
                $"chcon u:object_r:app_data_file:s0:c512,c768 {LocalMACFile}"
            };

            await Task.Run(() => Shell.SU.Run(commands));
        }

        public static string ReadMAC()
        {
            string content = File.ReadAllText(LocalMACFile);

            string searchString = "Intf0MacAddress=";
            int searchLength = searchString.Length;

            return content.Substring(content.IndexOf(searchString) + searchLength, 12);
        }

        public static async Task BackupMACBinary()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BackupMACFile));

            await Task.Run(() => Shell.SU.Run($"cp {MACFile} {BackupMACFile}"));
        }

        public static async Task ReplaceMACFile(Context context, string source)
        {
            IList<string> commands = new List<string>()
            {
                $"cp {source} {MACFile}",
                $"chmod 660 {MACFile}",
                $"chown root.root {MACFile}",
                $"chcon u:object_r:persist_wifi_file:s0 {MACFile}"
            };

            await Task.Run(() => Shell.SU.Run(commands));
        }
    }
}