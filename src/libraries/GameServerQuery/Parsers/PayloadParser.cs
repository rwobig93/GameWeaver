using System.Text;
using GameServerQuery.Parsers.Interfaces;

namespace GameServerQuery.Parsers;

public class PayloadParser : IPayloadParser
{
    private readonly byte[] _payload;

    /// <summary>
    /// Network byte payload parser
    /// </summary>
    /// <param name="payload">Byte array payload from a socket response to parse</param>
    public PayloadParser(byte[] payload)
    {
        _payload = payload;
        PayloadLength = _payload.Length - 1;
    }
    
    /// <inheritdoc />
    public int CurrentPayloadPosition { get; private set; } = -1;

    /// <inheritdoc />
    public int PayloadLength { get; }

    /// <inheritdoc />
    public int PayloadUnparsed => CurrentPayloadPosition - PayloadLength;

    /// <inheritdoc />
    public void SkipBytes(int amount)
    {
        CurrentPayloadPosition += amount;
        if (CurrentPayloadPosition > PayloadLength)
        {
            throw new EndOfStreamException();
        }
    }

    /// <inheritdoc />
    public byte[] GetUnparsedPayload()
    {
        return PayloadUnparsed <= 0 ? [] : _payload.Skip(CurrentPayloadPosition).ToArray();
    }

    /// <inheritdoc />
    public byte ReadByte()
    {
        CurrentPayloadPosition++;
        if (CurrentPayloadPosition > PayloadLength)
        {
            throw new EndOfStreamException();
        }
        
        return _payload[CurrentPayloadPosition];
    }

    /// <inheritdoc />
    public string ReadString()
    {
        CurrentPayloadPosition++;
        var stringStartIndex = CurrentPayloadPosition;

        while (_payload[CurrentPayloadPosition] != 0x00)
        {
            CurrentPayloadPosition++;
            if (CurrentPayloadPosition > PayloadLength)
            {
                throw new EndOfStreamException();
            }
        }
        
        return Encoding.UTF8.GetString(_payload, stringStartIndex, CurrentPayloadPosition - stringStartIndex);
    }

    /// <inheritdoc />
    public ushort ReadUShort()
    {
        CurrentPayloadPosition++;
        if (CurrentPayloadPosition + sizeof(ushort) - 1 > PayloadLength)
        {
            throw new EndOfStreamException();
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(_payload, CurrentPayloadPosition, sizeof(ushort));
        }

        try
        {
            var parsedInt16 = BitConverter.ToUInt16(_payload, CurrentPayloadPosition);
            CurrentPayloadPosition += sizeof(ushort) - 1;
            return parsedInt16;
        }
        catch (Exception)
        {
            throw new EndOfStreamException();
        }
    }

    /// <inheritdoc />
    public int ReadInt()
    {
        CurrentPayloadPosition++;
        if (CurrentPayloadPosition + sizeof(int) - 1 > PayloadLength)
        {
            throw new EndOfStreamException();
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(_payload, CurrentPayloadPosition, sizeof(int));
        }

        try
        {
            var parsedInt32 = BitConverter.ToInt32(_payload, CurrentPayloadPosition);
            CurrentPayloadPosition += sizeof(int) - 1;
            return parsedInt32;
        }
        catch (Exception)
        {
            throw new EndOfStreamException();
        }
    }

    /// <inheritdoc />
    public ulong ReadULong()
    {
        CurrentPayloadPosition++;
        if (CurrentPayloadPosition + sizeof(ulong) - 1 > PayloadLength)
        {
            throw new EndOfStreamException();
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(_payload, CurrentPayloadPosition, sizeof(ulong));
        }

        try
        {
            var parsedInt64 = BitConverter.ToUInt64(_payload, CurrentPayloadPosition);
            CurrentPayloadPosition += sizeof(ulong) - 1;
            return parsedInt64;
        }
        catch (Exception)
        {
            throw new EndOfStreamException();
        }
    }

    /// <inheritdoc />
    public float ReadFloat()
    {
        CurrentPayloadPosition++;
        if (CurrentPayloadPosition + sizeof(float) - 1 > PayloadLength)
        {
            throw new EndOfStreamException();
        }

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(_payload, CurrentPayloadPosition, sizeof(float));
        }

        try
        {
            var parsedFloat = BitConverter.ToSingle(_payload, CurrentPayloadPosition);
            CurrentPayloadPosition += sizeof(float) - 1;
            return parsedFloat;
        }
        catch (Exception)
        {
            throw new EndOfStreamException();
        }
    }
}