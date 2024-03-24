using MemoryPack;

namespace Domain.Contracts;

[MemoryPackable(GenerateType.Collection)]
public partial class SerializableList<T> : List<T>
{
    
}