using System;

using static System.FormattableString;

namespace IpGeolocator.Geolocator.Application
{
    internal readonly struct LocationV4Record
    {
        public LocationV4Record(uint fromAddress, uint toAddress, int index)
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
