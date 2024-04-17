using MemoryPack;

namespace Domain.Contracts;

[MemoryPackable(GenerateType.Collection)]
public partial class SerializableList<T> : List<T>
{
    public SerializableList()
    {
        
    }
    
    public SerializableList(IEnumerable<T> collection) : base(collection)
    {
    }

    public static SerializableList<T> FromList(IEnumerable<T> enumerable)
    {
        return new SerializableList<T>(enumerable);
    }
}