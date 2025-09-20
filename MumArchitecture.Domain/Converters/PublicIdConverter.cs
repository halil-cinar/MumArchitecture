using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Converters
{
    internal class PublicIdConverter
    {
        private readonly byte[] _key;
        private readonly int _rounds;
        private readonly byte[] _salt;

        public PublicIdConverter(byte[] key, int rounds = 8, string? salt = null)
        {
            _key = key;
            _rounds = rounds;
            _salt = string.IsNullOrEmpty(salt) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(salt);
        }
        public PublicIdConverter()
        {
            _key = Encoding.UTF8.GetBytes(AppSettings.instance?.IdConvertersSetting?.Key??"");
            _rounds = AppSettings.instance?.IdConvertersSetting?.Rounds ?? 8;
            var salt = AppSettings.instance?.IdConvertersSetting?.Salt;
            _salt = string.IsNullOrEmpty(salt) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(salt);
        }

        public int Encrypt(int value, string? scope = null)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            var v = (uint)value & 0x7FFFFFFF;
            var r = Feistel(v, scope, true);
            return (int)r;
        }

        public int Decrypt(int value, string? scope = null)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            var v = (uint)value & 0x7FFFFFFF;
            var r = Feistel(v, scope, false);
            return (int)r;
        }

        private uint Feistel(uint v, string? scope, bool forward)
        {
            uint l = v >> 15;
            uint r = v & 0x7FFF;
            if (forward)
            {
                for (int i = 0; i < _rounds; i++)
                {
                    var f = RoundF(r, i, scope) & 0xFFFF;
                    var nl = r;
                    var nr = (l ^ f) & 0x7FFF;
                    l = nl;
                    r = nr;
                }
            }
            else
            {
                for (int i = _rounds - 1; i >= 0; i--)
                {
                    var f = RoundF(l, i, scope) & 0xFFFF;
                    var nr = l;
                    var nl = (r ^ f) & 0x7FFF;
                    r = nr;
                    l = nl;
                }
            }
            return ((l & 0xFFFF) << 15) | (r & 0x7FFF);
        }

        private uint RoundF(uint half, int round, string? scope)
        {
            Span<byte> input = stackalloc byte[4 + 4 + (_salt?.Length ?? 0) + (scope == null ? 0 : Encoding.UTF8.GetByteCount(scope))];
            var span = input;
            BitConverter.TryWriteBytes(span.Slice(0, 4), half);
            BitConverter.TryWriteBytes(span.Slice(4, 4), round);
            var offset = 8;
            if (_salt.Length > 0) { _salt.CopyTo(span.Slice(offset)); offset += _salt.Length; }
            if (scope != null)
            {
                var sb = Encoding.UTF8.GetBytes(scope);
                sb.CopyTo(span.Slice(offset));
                offset += sb.Length;
            }
            using var h = new HMACSHA256(_key);
            var hash = h.ComputeHash(input.Slice(0, offset).ToArray());
            return BitConverter.ToUInt32(hash, 0);
        }
    }

    public static class PublicIdConverterExtensions
    {
        public static int ToPublicId(this int id, string scope = "default")
        {
            var converter = new PublicIdConverter();
            return converter.Encrypt(id, scope);
        }
        public static int ToDatabaseId(this int publicId, string scope = "default")
        {
            var converter = new PublicIdConverter();
            return converter.Decrypt(publicId, scope);
        }
        public static int? ToPublicId(this int? id, string scope = "default")
        {
            var converter = new PublicIdConverter();
            return (id == null) ? null : converter.Encrypt(id ?? 0, scope);
        }
        public static int? ToDatabaseId(this int? publicId, string scope = "default")
        {
            var converter = new PublicIdConverter();
            return (publicId == null) ? null : converter.Decrypt(publicId ?? 0, scope);
        }

    }
}