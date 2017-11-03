using System;

namespace nMAC.Devices
{
    class WileyfoxSpark : DeviceModel
    {
        public WileyfoxSpark()
        {
            this.Path = "/nvdata/APCFG/APRDEB/WIFI";
        }

        internal override bool CheckFile(byte[] content)
        {
            if (content.Length != 0x202)
                return false;

            return true;
        }

        internal override string ExtractMACFromFile(byte[] content)
        {
            byte[] MAC = new byte[6];
            Array.Copy(content, 4, MAC, 0, 6);

            return BitConverter.ToString(MAC).Replace("-", string.Empty); // BitConverter result separated by '-'
        }

        internal override void WriteMAC(ref byte[] content, string MAC)
        {
            Array.Copy(this.GetMACBytes(MAC), 0, content, 4, 6);
        }
    }
}