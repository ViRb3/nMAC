using System.Text.RegularExpressions;

namespace nMAC.Devices
{
    class Nexus5X : DeviceModel
    {
        public Nexus5X()
        {
            this.Path = "/persist/wlan_mac.bin";
            this.FileSyntax = new Regex(
                @"^(Intf0MacAddress=(([0-9A-F]{2}){6}))\n+(Intf1MacAddress=(([0-9A-F]{2}){6}))\n+(Intf2MacAddress=(([0-9A-F]{2}){6}))\n+(Intf3MacAddress=(([0-9A-F]{2}){6}))\n*$");
        }


        internal override bool CheckFile(string content)
        {
            if (this.FileSyntax.IsMatch(content))
                return true;

            return false;
        }

        internal override string ExtractMACFromFile(string content)
        {
            string searchString = "Intf0MacAddress=";
            int searchLength = searchString.Length;

            return content.Substring(content.IndexOf(searchString) + searchLength, 12);
        }

        internal override void WriteMAC(ref string content, string MAC)
        {
            string searchString = "Intf0MacAddress=";
            int editIndex = content.IndexOf(searchString) + searchString.Length;

            content = content.Remove(editIndex, 12);
            content = content.Insert(editIndex, MAC);
        }
    }
}