using System;

using static System.FormattableString;

namespace IpGeolocator.Application
{
    internal readonly struct I2LInterval4
    {
        public I2LInterval4(uint fromAddress, uint toAddress, int index)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Index = index;

            if (ToAddress < FromAddress)
            {
                InvalidAddress();
            }

            void InvalidAddress()
            {
                throw new ArgumentOutOfRangeException(nameof(toAddress), toAddress, Invariant($"The \"to\" address {toAddress} is not larger than \"from\" {fromAddress}."));
            }
        }

        public uint FromAddress { get; }

        public uint ToAddress { get; }

        public int Index { get; }
    }
}
