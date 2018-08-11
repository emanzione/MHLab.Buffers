using System;
using System.Text;

namespace MHLab.Buffers
{
    public partial class Buffer
    {
        /// <summary>
        /// Writes a byte on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(byte value, int bits = sizeof(byte) * 8)
        {
            EnsureBufferSpace(bits);
            WriteBits(value, bits);
        }

        /// <summary>
        /// Writes a sbyte on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(sbyte value, int bits = sizeof(byte) * 8)
        {
            Write((byte)value, bits);
        }

        /// <summary>
        /// Writes a byte array on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(byte[] value)
        {
            EnsureBufferSpace(value.Length * 8);

            if (m_bitsPointer == 0)
            {
                System.Buffer.BlockCopy(value, 0, m_buffer, m_bytePointer, value.Length);
                m_bytePointer += value.Length;
                return;
            }

            for (var index = 0; index < value.Length; index++)
                Write(value[index]);
        }

        /// <summary>
        /// Writes the NetworkBuffer content on the buffer.
        /// </summary>
        /// <param name="buffer">The content to write.</param>
        public void Write(Buffer buffer)
        {
            Write(buffer.m_buffer);
        }

        /// <summary>
        /// Writes a bool on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(bool value)
        {
            EnsureBufferSpace(1);
            WriteBits((value) ? (byte)1 : (byte)0, 1);
        }

        /// <summary>
        /// Writes a short on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(short value, int bits = sizeof(short) * 8)
        {
            Write((ushort)value, bits);
        }

        /// <summary>
        /// Writes a ushort on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(ushort value, int bits = sizeof(short) * 8)
        {
            EnsureBufferSpace(bits);
            if (bits <= 8)
            {
                WriteBits((byte)value, bits);
            }
            else
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), bits - 8);
            }
        }

        /// <summary>
        /// Writes a int on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(int value, int bits = sizeof(int) * 8)
        {
            Write((uint)value, bits);
        }

        /// <summary>
        /// Writes a uint on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(uint value, int bits = sizeof(uint) * 8)
        {
            EnsureBufferSpace(bits);
            if (bits <= 8)
            {
                WriteBits((byte)value, bits);
            }
            else if (bits <= 16)
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), bits - 8);
            }
            else if (bits <= 24)
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), 8);
                WriteBits((byte)(value >> 16), bits - 16);
            }
            else
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), 8);
                WriteBits((byte)(value >> 16), 8);
                WriteBits((byte)(value >> 24), bits - 24);
            }
        }

        /// <summary>
        /// Writes a long on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(long value, int bits = sizeof(long) * 8)
        {
            Write((ulong)value, bits);
        }

        /// <summary>
        /// Writes a ulong on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void Write(ulong value, int bits = sizeof(ulong) * 8)
        {
            EnsureBufferSpace(bits);

            if (bits <= 32)
            {
                Write((uint)value, bits);
            }
            else
            {
                Write((uint)value, 32);
                Write((uint)(value >> 32), bits - 32);
            }
        }

        /// <summary>
        /// Writes a double on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(double value)
        {
            EnsureBufferSpace(sizeof(double) * 8);

            m_byteConverter.Double = value;

            WriteBits(m_byteConverter.Byte0, 8);
            WriteBits(m_byteConverter.Byte1, 8);
            WriteBits(m_byteConverter.Byte2, 8);
            WriteBits(m_byteConverter.Byte3, 8);
            WriteBits(m_byteConverter.Byte4, 8);
            WriteBits(m_byteConverter.Byte5, 8);
            WriteBits(m_byteConverter.Byte6, 8);
            WriteBits(m_byteConverter.Byte7, 8);
        }

        /// <summary>
        /// Writes a float on the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(float value)
        {
            EnsureBufferSpace(sizeof(float) * 8);

            m_byteConverter.Float = value;

            WriteBits(m_byteConverter.Byte0, 8);
            WriteBits(m_byteConverter.Byte1, 8);
            WriteBits(m_byteConverter.Byte2, 8);
            WriteBits(m_byteConverter.Byte3, 8);
        }

        /// <summary>
        /// Writes a string on the buffer. The max allowed size
        /// of the string is 256 (the length is sent as byte).
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(string value)
        {
            EnsureBufferSpace(sizeof(char) * 8 * value.Length);

            WriteBits((byte)value.Length, 8);
            Write(Encoding.UTF8.GetBytes(value));
        }
    }
}
