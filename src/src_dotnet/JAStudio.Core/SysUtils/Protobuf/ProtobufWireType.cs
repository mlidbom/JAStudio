namespace JAStudio.Core.SysUtils.Protobuf;

/// <summary>How a protobuf field's value is encoded in a message's bytes, as its key gives it (<see cref="ProtobufFieldKey"/>).</summary>
enum ProtobufWireType
{
   /// <summary>A varint: an integer, a bool or an enum, seven bits a byte (<see cref="ProtobufWireReader.ReadVarint"/>).</summary>
   Varint = 0,

   /// <summary>Eight bytes: a <c>fixed64</c>, an <c>sfixed64</c> or a <c>double</c>.</summary>
   Fixed64 = 1,

   /// <summary>A length, then that many bytes: a string, bytes, an embedded message or a packed repeated field (<see cref="ProtobufWireReader.ReadLengthDelimited"/>).</summary>
   LengthDelimited = 2,

   /// <summary>The start of a group, a deprecated kind of embedded message.</summary>
   StartGroup = 3,

   /// <summary>The end of a group, a deprecated kind of embedded message.</summary>
   EndGroup = 4,

   /// <summary>Four bytes: a <c>fixed32</c>, an <c>sfixed32</c> or a <c>float</c>.</summary>
   Fixed32 = 5
}
