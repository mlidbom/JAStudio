using System;
using System.Security.Cryptography;
using System.Text;

namespace JAStudio.Core.SysUtils;

/// <summary>
/// A name-based GUID, RFC 4122's version 5: the same name in the same namespace always gives the same GUID, and different names, in practice,<br/>
/// different ones, so an id can be made again from what it identifies rather than remembered.
/// </summary>
/// <remarks>The SHA-1 of the namespace's 16 bytes, big-endian, followed by the name's UTF-8 bytes: its first 16 bytes, with the version and variant set.</remarks>
public static class NameBasedGuid
{
   const byte Version5 = 0x50;
   const byte Rfc4122Variant = 0x80;

   /// <summary>The GUID of <paramref name="name"/> in the namespace <paramref name="namespaceId"/>.</summary>
   public static Guid Of(Guid namespaceId, string name)
   {
      var hash = SHA1.HashData([.. namespaceId.ToByteArray(bigEndian: true), .. Encoding.UTF8.GetBytes(name)]);
      hash[6] = (byte)(hash[6] & 0x0F | Version5);
      hash[8] = (byte)(hash[8] & 0x3F | Rfc4122Variant);
      return new Guid(hash.AsSpan(0, 16), bigEndian: true);
   }
}
