namespace JAStudio.Core.SysUtils.Protobuf;

/// <summary>The key a protobuf field's value comes after, in a message's bytes (<see cref="ProtobufWireReader"/>): which field it is, and how its value is encoded.</summary>
/// <param name="FieldNumber">The field's number in its message's schema, such as 1 for <c>string name = 1</c>.</param>
sealed record ProtobufFieldKey(int FieldNumber, ProtobufWireType WireType);
