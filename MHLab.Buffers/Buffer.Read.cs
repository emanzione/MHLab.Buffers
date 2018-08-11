using System;
using System.Text;

namespace MHLab.Buffers
{
    public partial class Buffer
    {
        /// <summary>
        /// Reads a byte from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public byte ReadByte(int bits = sizeof(byte) * 8)
        {
            return ReadBits(bits);
        }

        /// <summary>
        /// Reads a sbyte from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public sbyte ReadSByte(int bits = sizeof(byte) * 8)
        {
            return (sbyte)ReadBits(bits);
        }

        /// <summary>
        /// Reads a byte array from the buffer.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        public byte[] ReadBytes(int length)
        {
            var bytes = new byte[length];

            if (m_bitsPointer == 0)
            {
                System.Buffer.BlockCopy(m_buffer, m_bytePointer, bytes, 0, length);
                return bytes;
            }

            for (int index = 0; index < length; index++)
            {
                bytes[index] = ReadBits(8);
            }
            return bytes;
        }

        /// <summary>
        /// Reads a bool from the buffer.
        /// </summary>
        public bool ReadBool()
        {
            return ReadBits(1) == 1;
        }

        /// <summary>
        /// Reads a short from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public short ReadShort(int bits = sizeof(short) * 8)
        {
            return (short)ReadUShort(bits);
        }

        /// <summary>
        /// Reads a ushort from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public ushort ReadUShort(int bits = sizeof(short) * 8)
        {
            if (bits <= 8)
            {
                return (ushort)ReadBits(bits);
            }
            else
            {
                return (ushort)(ReadBits(8) | (ReadBits(bits - 8) << 8));
            }
        }

        /// <summary>
        /// Reads a int from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public int ReadInt32(int bits = sizeof(int) * 8)
        {
            return (int)ReadUInt32(bits);
        }

        /// <summary>
        /// Reads a uint from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public uint ReadUInt32(int bits = sizeof(int) * 8)
        {
            if (bits <= 8)
            {
                return (uint)ReadBits(bits);
            }
            else if (bits <= 16)
            {
                return (uint)(ReadBits(8) | (ReadBits(bits - 8) << 8));
            }
            else if (bits <= 24)
            {
                return (uint)(ReadBits(8) | (ReadBits(8) << 8) | (ReadBits(bits - 16) << 16));
            }
            else
            {
                return (uint)(ReadBits(8) | (ReadBits(8) << 8) | (ReadBits(8) << 16) | (ReadBits(bits - 24) << 24));
            }
        }

        /// <summary>
        /// Reads a long from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public long ReadInt64(int bits = sizeof(long) * 8)
        {
            return (long)ReadUInt64(bits);
        }

        /// <summary>
        /// Reads a ulong from the buffer.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        public ulong ReadUInt64(int bits = sizeof(ulong) * 8)
        {
            if (bits <= 32)
            {
                return ReadUInt32(bits);
            }
            else
            {
                ulong first = ReadUInt32(32) & 0xFFFFFFFF;
                ulong second = ReadUInt32(bits - 32);
                return first | (second << 32);
            }
        }

        /// <summary>
        /// Reads a double from the buffer.
        /// </summary>
        public double ReadDouble()
        {
            m_byteConverter.Byte0 = ReadBits(8);
            m_byteConverter.Byte1 = ReadBits(8);
            m_byteConverter.Byte2 = ReadBits(8);
            m_byteConverter.Byte3 = ReadBits(8);
            m_byteConverter.Byte4 = ReadBits(8);
            m_byteConverter.Byte5 = ReadBits(8);
            m_byteConverter.Byte6 = ReadBits(8);
            m_byteConverter.Byte7 = ReadBits(8);

            return m_byteConverter.Double;
        }

        /// <summary>
        /// Reads a float from the buffer.
        /// </summary>
        public float ReadFloat()
        {
            m_byteConverter.Byte0 = ReadBits(8);
            m_byteConverter.Byte1 = ReadBits(8);
            m_byteConverter.Byte2 = ReadBits(8);
            m_byteConverter.Byte3 = ReadBits(8);

            return m_byteConverter.Float;
        }

        /// <summary>
        /// Reads a string from the buffer.
        /// </summary>
        public string ReadString()
        {
            return Encoding.UTF8.GetString(ReadBytes(ReadBits(8)));
        }
    }
}
