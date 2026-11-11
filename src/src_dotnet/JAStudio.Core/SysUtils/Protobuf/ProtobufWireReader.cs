namespace JAStudio.Core.SysUtils.Protobuf;

/// <summary>
/// Reads a protobuf message field by field, by its wire format alone, with no schema and no protobuf library: each field's key<br/>
/// (<see cref="ProtobufFieldKey"/>), then its value. For reading a message or two whose fields are known, such as an Anki package's media map.
/// </summary>
/// <remarks>
/// A message is its fields one after another, each a key, a varint holding the field's number and its <see cref="ProtobufWireType"/>, then its<br/>
/// value: a varint for <see cref="ProtobufWireType.Varint"/>, and for <see cref="ProtobufWireType.LengthDelimited"/> a varint length followed by that<br/>
/// many bytes, such as a string or an embedded message. See <c>https://protobuf.dev/programming-guides/encoding/</c>.
/// </remarks>
sealed class ProtobufWireReader
{
   const int BitsOfTheWireTypeInAKey = 3;
   const ulong WireTypeBitsOfAKey = 0b111;
   const byte ValueBitsOfAVarintByte = 0x7F;
   const byte AnotherVarintByteFollowsBit = 0x80;

   readonly byte[] _message;
   int _position;

   /// <summary>Reads <paramref name="message"/>, a protobuf message's bytes, from its first field.</summary>
   public ProtobufWireReader(byte[] message) => _message = message;

   /// <summary>Whether the message has a field after those read.</summary>
   public bool HasMoreFields => _position < _message.Length;

   /// <summary>The key of the next field, which its value follows.</summary>
   public ProtobufFieldKey ReadFieldKey()
   {
      var key = ReadVarint();
      return new ProtobufFieldKey((int)(key >> BitsOfTheWireTypeInAKey), (ProtobufWireType)(key & WireTypeBitsOfAKey));
   }

   /// <summary>A varint value: seven bits a byte, the least significant first, every byte but the last with its top bit set.</summary>
   public ulong ReadVarint()
   {
      var value = 0UL;
      for(var shift = 0;; shift += 7)
      {
         var current = _message[_position++];
         value |= (ulong)(current & ValueBitsOfAVarintByte) << shift;
         if((current & AnotherVarintByteFollowsBit) == 0) return value;
      }
   }

   /// <summary>A length-delimited value, such as a string or an embedded message: its length, a varint, then that many bytes.</summary>
   public byte[] ReadLengthDelimited()
   {
      var length = (int)ReadVarint();
      var value = _message[_position..(_position + length)];
      _position += length;
      return value;
   }
}
