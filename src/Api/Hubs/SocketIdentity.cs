using Api.Common.Enums;
using Api.Extensions;

namespace Api.Hubs;

public class SocketIdentity(Guid id, UserType type)
{
    private Guid Id { get; } = id;
    private UserType Type { get; } = type;

    public override int GetHashCode()
    {
        return Id.GetHashCode() ^ Type.GetHashCode();
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as SocketIdentity);
    }
    
    private bool Equals(SocketIdentity? obj)
    {
        return obj != null && obj.Id == Id && obj.Type == Type;
    }

    public override string ToString()
    {
        return "Connection: " + Id.ToString() + "_" + Type.GetEnumDisplayName();
    }
}