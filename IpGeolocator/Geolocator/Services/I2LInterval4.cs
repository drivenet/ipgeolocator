using System;

using static System.FormattableString;

namespace IpGeolocator.Geolocator.Services
{
    internal readonly struct I2LInterval4 : IEquatable<I2LInterval4>
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

        public static bool operator ==(I2LInterval4 left, I2LInterval4 right) => left.Equals(right);

        public static bool operator !=(I2LInterval4 left, I2LInterval4 right) => !(left == right);

        public override bool Equals(object? obj) => obj is I2LInterval4 interval && Equals(interval);

        public bool Equals(I2LInterval4 other)
            => FromAddress == other.FromAddress
            && ToAddress == other.ToAddress
            && Index == other.Index;

        public override int GetHashCode() => HashCode.Combine(FromAddress, ToAddress);
    }
}
