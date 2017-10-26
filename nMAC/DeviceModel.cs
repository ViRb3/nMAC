using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace nMAC
{
    internal abstract class DeviceModel
    {
        internal int Priority = 100;
        internal string Path { get; set; }
        internal Regex FileSyntax;

        internal virtual bool CheckFile(byte[] content)
        {
            return CheckFile(Encoding.UTF8.GetString(content));
        }

        internal virtual bool CheckFile(string content)
        {
            throw new NotImplementedException();
        }

        internal virtual string ExtractMACFromFile(byte[] content)
        {
            return ExtractMACFromFile(Encoding.UTF8.GetString(content));
        }

        internal virtual string ExtractMACFromFile(string content)
        {
            throw new NotImplementedException();
        }

        internal virtual void WriteMAC(ref byte[] content, string MAC)
        {
            string contentString = Encoding.UTF8.GetString(content);
            WriteMAC(ref contentString, MAC);

            content = Encoding.UTF8.GetBytes(contentString);
        }

        internal virtual void WriteMAC(ref string content, string MAC)
        {
            throw new NotImplementedException();
        }

        internal byte[] GetMACBytes(string MAC)
        {
            int counter = 0;
            string @byte = string.Empty;
            List<byte> MACBytes = new List<byte>();

            for (int i = 0; i < MAC.Length; i++)
            {
                @byte += MAC[i].ToString();

                if (counter < 1)
                {
                    counter++;
                    continue;
                }

                MACBytes.Add(Convert.ToByte(@byte, 16));

                counter = 0;
                @byte = string.Empty;
            }

            return MACBytes.ToArray();
        }
    }
}