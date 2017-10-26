using System.Text.RegularExpressions;

namespace nMAC.Devices
{
    class Samsung : DeviceModel
    {
        public Samsung()
        {
            this.Path = "/efs/wifi/.mac.info";
            this.FileSyntax = new Regex(@"^([0-9A-F]{2}[:]){5}([0-9A-F]{2})\n*$");
        }

        internal override bool CheckFile(string content)
        {
            if (this.FileSyntax.IsMatch(content))
                return true;

            return false;
        }

        internal override string ExtractMACFromFile(string content)
        {
            return content.Replace(":", string.Empty);
        }

        internal override void WriteMAC(ref string content, string MAC)
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
        }
    }
}