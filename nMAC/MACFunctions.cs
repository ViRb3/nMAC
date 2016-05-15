using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Content;
using EU.Chainfire.Libsuperuser;
using Environment = Android.OS.Environment;

namespace nMAC
{
    public static class MACFunctions
    {
        public static string MACFile;
        public static string LocalMACFile;
        public static string BackupMACFile;
        private static readonly Regex RegexPlainMAC = new Regex(@"^([0-9A-F]{2}[:]){5}([0-9A-F]{2})\n?$");
        private static readonly Regex RegexN5XMAC = new Regex(@"^(Intf0MacAddress=(([0-9A-F]{2}){6}))\n(Intf1MacAddress=(([0-9A-F]{2}){6}))\n(Intf2MacAddress=(([0-9A-F]{2}){6}))\n(Intf3MacAddress=(([0-9A-F]{2}){6}))\n?$");

        private static MACSyntax _MACSyntax = MACSyntax.Unknown;

        public static async Task AssignPaths(Context context)
        {
            MACFile = await FindMACLocation();

            if (MACFile == null)
            {
                Helpers.ShowCriticalError(context, @"Sorry, this device not supported.
Please contact me if you want to make it work!");
                return;
            }

            LocalMACFile = Path.Combine(context.FilesDir.AbsolutePath, "mac.bin");
            BackupMACFile = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, ".nMAC", "wlan_mac.bin");       
        }

        private static async Task<string> FindMACLocation()
        {
            string[] MACPaths =
            {
                "/persist/wlan_mac.bin",
                "/efs/wifi/.mac.info"
            };

            IList<string> result = null;

            foreach (string path in MACPaths)
            {
                await Task.Run(() => result = Shell.SU.Run($"cat {path}"));

                if (result == null || result.Count == 0 || string.IsNullOrWhiteSpace(result.ToString()))
                    continue;

                return path;
            }

            return null;
        }

        public static async Task GetMACFile(Context context)
        {
            if (File.Exists(LocalMACFile))
                File.Delete(LocalMACFile);

            File.WriteAllText(LocalMACFile, string.Empty); // let user create their own properly owned file

            await Task.Run(() => Shell.SU.Run($"cat {MACFile} > {LocalMACFile}"));
        }

        public static string ReadMAC(Context context)
        {
            string content = File.ReadAllText(LocalMACFile);

            if (RegexPlainMAC.IsMatch(content))
            {
                _MACSyntax = MACSyntax.Plain;
                return ReadPlainMAC(content);
            }

            if (RegexN5XMAC.IsMatch(content))
            {
                _MACSyntax = MACSyntax.N5X;
                return ReadN5XMAC(content);
            }

            Helpers.ShowCriticalError(context, @"Sorry, this device not supported.
Please contact me if you want to make it work!");
            return null;
        }

        private static string ReadPlainMAC(string content)
        {
            return content.Replace(":", string.Empty);
        }

        private static string ReadN5XMAC(string content)
        {
            string searchString = "Intf0MacAddress=";
            int searchLength = searchString.Length;

            return content.Substring(content.IndexOf(searchString) + searchLength, 12);
        }

        public static void WriteMAC(ref string content, string MAC)
        {
            if (_MACSyntax == MACSyntax.Plain)
            {
                string formattedMAC = string.Empty;

                int @break = 0;
                for (int i = 0; i < MAC.Length;)
                {
                    if (@break < 2)
                    {
                        formattedMAC += MAC[i];
                        @break++;
                        i++;
                    }
                    else
                    {
                        @break = 0;
                        formattedMAC += ":";
                    }
                }

                content = formattedMAC;
                return;
            }

            if (_MACSyntax == MACSyntax.N5X)
            {
                string searchString = "Intf0MacAddress=";
                int editIndex = content.IndexOf(searchString) + searchString.Length;

                content = content.Remove(editIndex, 12);
                content = content.Insert(editIndex, MAC);
            }
        }

        public static async Task BackupMACBinary()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BackupMACFile));

            await Task.Run(() => Shell.SU.Run($"cat {MACFile} > {BackupMACFile}"));
        }

        public static async Task ReplaceMACFile(Context context, string source)
        {
            await Task.Run(() => Shell.SU.Run($"cat {source} > {MACFile}"));
        }
    }

    enum MACSyntax
    {
       Unknown,
       N5X,
       Plain
    }
}