namespace GameServerQuery.Parsers.Interfaces;

public interface IPayloadParser
{
    /// <summary>
    /// The current position in the payload being read
    /// </summary>
    int CurrentPayloadPosition { get; }
    
    /// <summary>
    /// Total length of the payload
    /// </summary>
    int PayloadLength { get; }
    
    /// <summary>
    /// Whether there are remaining bytes currently unparsed
    /// </summary>
    bool HasUnparsedBytes { get; }
    
    /// <summary>
    /// Skip a specific amount of bytes from the current parsed position
    /// </summary>
    /// <param name="amount">Number of bytes to skip</param>
    /// <exception cref="EndOfStreamException">An attempt to read beyond the payload's size was attempted</exception>
    void SkipBytes(int amount);
    
    /// <summary>
    /// Get the remaining unparsed portion of the payload if there is any
    /// </summary>
    /// <returns>Any remaining unparsed bytes in the payload or an empty array</returns>
    byte[] GetUnparsedPayload();
    
    /// <summary>
    /// Read the next available byte in the payload
    /// </summary>
    /// <exception cref="EndOfStreamException">An attempt to read beyond the payload's size was attempted</exception>
    /// <returns>Parsed byte from the payload</returns>
    byte ReadByte();
    
    /// <summary>
    /// Read the next available string in the payload
    /// </summary>
    /// <exception cref="EndOfStreamException">An attempt to read beyond the payload's size was attempted</exception>
    /// <returns>Parsed string from the payload</returns>
    string ReadString();
    
    /// <summary>
    /// Read the next available ushort in the payload
    /// </summary>
    /// <exception cref="EndOfStreamException">An attempt to read beyond the payload's size was attempted</exception>
    /// <returns>Parsed ushort from the payload</returns>
    ushort ReadUShort();
    
    /// <summary>
    /// Read the next available integer in the payload
    /// </summary>
    /// <exception cref="EndOfStreamException">An attempt to read beyond the payload's size was attempted</exception>
    /// <returns>Parsed integer from the payload</returns>
    int ReadInt();
    
    /// <summary>
    /// Read the next available ulong in the payload
    /// </summary>
    /// <exception cref="EndOfStreamException">An attempt to read beyond the payload's size was attempted</exception>
    /// <returns>Parsed ulong from the payload</returns>
    ulong ReadULong();
    
    /// <summary>
    /// Read the next available float in the payload
    /// </summary>
    /// <exception cref="EndOfStreamException">An attempt to read beyond the payload's size was attempted</exception>
    /// <returns>Parsed float from the payload</returns>
    float ReadFloat();
}