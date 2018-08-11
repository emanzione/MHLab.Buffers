using MHLab.Buffers.Converters;
using MHLab.Pooling;
using System.Runtime.CompilerServices;

namespace MHLab.Buffers
{
    public partial class Buffer
    {
        private const float GrowingFactor = 1.5f;
        private byte[] m_buffer;
        private int m_bitsPointer;
        private byte m_currentByte;
        private int m_bytePointer;

        private ByteConverter m_byteConverter;

        protected Buffer()
        {
            m_byteConverter = new ByteConverter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetPointer()
        {
            m_bitsPointer = 0;
            m_bytePointer = 0;
            m_currentByte = (m_buffer.Length > 0) ? m_buffer[0] : (byte)0x00;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBufferSpace(int additionalSpaceInBits)
        {
            var length = m_buffer.Length;
            if (m_bitsPointer + additionalSpaceInBits > length << 3)
            {
                var tmpBuffer = m_buffer;
                var newBuffer = BytesPool.Get((int)(length * GrowingFactor) + 1);
                System.Buffer.BlockCopy(tmpBuffer, 0, newBuffer, 0, tmpBuffer.Length);
                BytesPool.Recycle(m_buffer);
                m_buffer = newBuffer;
            }
        }
        
        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(byte data, int bitsAmount)
        {
            if (bitsAmount <= 0) return;

            if (bitsAmount > 8) bitsAmount = 8;

            // Ensure that the buffer can hold the new amount
            // of bits.
            EnsureBufferSpace(bitsAmount);

            // Identify the position in our buffer: m_bitsPointer / 8
            // The division operation is expensive on CPU.
            // We use a right-shift operator that is faster,
            // it is the same as writing m_bitsPointer / 2^3
            var positionInByte = m_bitsPointer >> 3;

            // Identify the bit offset relative to positionInByte: 
            // m_bitsPointer % 8;
            // The division operation is expensive on CPU.
            // For this reason we use a & operation instead.
            // The explanation is the following.
            // We have that 81 / 8 = 10 and 81 % 8 = 1.
            // Given our x = 81 => 0 1 0 1 0 0 0 1
            // Given our y = 7  => 0 0 0 0 0 1 1 1
            // We apply & oper. => 0 0 0 0 0 0 0 1
            // This is the result of x % 8, but faster.
            var bitsOffset = m_bitsPointer & 0x7;

            // A trivial case: when the offset is 0 and
            // the amount of bits we have to write is 8,
            // we just have to write a byte.
            // So we can directly set it in the buffer.
            if (bitsOffset == 0 && bitsAmount == 8)
            {
                m_buffer[positionInByte] = data;
                m_bitsPointer += 8;
                return;
            }

            // The amount of free bits in the current byte.
            var freeBits = 8 - bitsOffset;

            // The amount of left bits after our write operation
            var leftBitsAfterWriting = freeBits - bitsAmount;

            // We want to strip off bits that should not be written.
            // So let's say that bitsAmount is 5 and we have this:
            // data = 0 1 0 1 0 1 0 0
            // We mask it with a 0xFF right-shifted by 8 - 5 = 3
            // mask = 1 1 1 1 1 1 1 1 >> 3 = 0 0 0 1 1 1 1 1
            // The result of (data & mask):
            // 0 0 0 1 0 1 0 0
            data = (byte)(data & (0xFF >> (8 - bitsAmount)));
            
            // If we have atleast 0 left bits, we are ok: we don't 
            // need to write in the next byte.
            if (leftBitsAfterWriting >= 0)
            {
                // So prepare the mask.
                // First mask has to identify old bits in the buffer:
                // mask_1 = 0xFF >> freeBits       = 0 0 0 0 0 0 0 1
                // Second mask has to identify free left bits:
                // mask_2 = 0xFF << (8 - leftBits) = 1 1 0 0 0 0 0 0
                // This assuming that freeBits = 1 and leftBits = 2,
                // we have:
                // mask = mask_1 | mask_2          = 1 1 0 0 0 0 0 1
                // Where 0s are, we have our 5 bits to write.
                var mask = (0xFF >> freeBits) | (0xFF << (8 - leftBitsAfterWriting));
                
                // Apply the mask to the current byte in our buffer:
                // current byte = 1 1 1 1 1 1 1 1
                // mask         = 1 1 0 0 0 0 0 1
                // masked byte  = 1 1 0 0 0 0 0 1
                // Write the data left-shifted of bitsOffset:
                // data = 0 0 0 1 0 1 0 0 << 1 = 0 0 1 0 1 0 0 0
                // result = 1 1 1 0 1 0 0 1
                m_buffer[positionInByte] = (byte)((m_buffer[positionInByte] & mask) | (data << bitsOffset));
            }
            else
            {
                // Apply the mask to the current byte in our buffer.
                // Shift our data to fit free bits.
                m_buffer[positionInByte] = (byte)((m_buffer[positionInByte] & (0xFF >> freeBits)) | (data << bitsOffset));

                // Same here. This time we work on the next byte in
                // our buffer. So we mask it and write the remaining 
                // bits.
                m_buffer[positionInByte + 1] = (byte)((m_buffer[positionInByte + 1] & (0xFF << (bitsAmount - freeBits))) | (data >> freeBits));
            }

            m_bitsPointer += bitsAmount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadBits(int bitsAmount)
        {
            if (bitsAmount <= 0) return 0x00;
            if (bitsAmount > 8) bitsAmount = 8;

            var positionInByte = m_bitsPointer >> 8;
            var bitsOffset = m_bitsPointer & 0x7;

            if (bitsOffset == 0 && bitsAmount == 8)
            {
                m_bitsPointer += 8;
                return m_buffer[positionInByte];
            }

            var freeBits = 8 - bitsOffset;
            var leftBitsAfterRead = freeBits - bitsAmount;

            if (leftBitsAfterRead >= 0)
            {
                byte data = (byte)(m_buffer[positionInByte] >> bitsOffset);
                m_bitsPointer += bitsAmount;
                return (byte)(data & (0xFF >> (8 - bitsAmount)));
            }
            else
            {
                byte dataFirst = (byte)((m_buffer[positionInByte] >> bitsOffset) & (0xFF >> bitsOffset));
                byte dataSecond = (byte)((m_buffer[positionInByte + 1]) & (0xFF >> (bitsAmount - (8 - bitsOffset))));
                m_bitsPointer += bitsAmount;
                return (byte)(dataFirst | (dataSecond << (8 - bitsOffset)));
            }
        }*/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(byte data, int bitsAmount)
        {
            if (bitsAmount <= 0) return;
            if (bitsAmount > 8) bitsAmount = 8;

            if (m_bitsPointer == 0 && bitsAmount == 8)
            {
                m_currentByte = data;
                m_buffer[m_bytePointer] = data;
                m_bytePointer += 1;
                return;
            }
            
            // Left bits in the current byte in the buffer.
            //int leftBits = 8 - m_bitsPointer - bitsAmount;
            // If it is < 0, we have to move to the next byte.
            //leftBits = (leftBits < 0) ? 0 : leftBits;
            // Remaining bits to write to the next byte.
            int leftBitsToWrite = bitsAmount - (8 - m_bitsPointer);

            // This is the mask to preserve old written bits (0xFF << m_bitsPointer)
            // and to allow writing of new ones.
            var mask = (0xFF << m_bitsPointer); // & (0xFF >> leftBits); // Do we really need to mask out bits > m_bitsPointer + leftBits?

            // Write to the current byte.
            m_currentByte = (byte)((m_currentByte & (0xFF >> (8 - m_bitsPointer))) | ((data << m_bitsPointer) & mask));
            // Write the current byte to the buffer.
            m_buffer[m_bytePointer] = m_currentByte;

            // If we have left bits to write, we have to advance the buffer pointer.
            if (leftBitsToWrite > 0)
            {
                m_bytePointer += 1;
                m_currentByte = 0x00;//m_buffer[m_bytePointer];

                m_currentByte = (byte)((m_currentByte) | ((data >> (bitsAmount - leftBitsToWrite)) & (0xFF >> 8 - leftBitsToWrite)));
                m_buffer[m_bytePointer] = m_currentByte;

                m_bitsPointer = leftBitsToWrite;
            }
            // Else we can just increment our bits pointer.
            else
            {
                m_bitsPointer += bitsAmount;
                if (m_bitsPointer >= 8)
                {
                    m_bitsPointer = 0;
                    m_bytePointer += 1;
                    m_currentByte = 0x00;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadBits(int bitsAmount)
        {
            if (m_bitsPointer == 0 && bitsAmount == 8)
            {
                m_bytePointer += 1;
                var tmp = m_currentByte;
                m_currentByte = m_buffer[m_bytePointer];
                return tmp;
            }

            int freeBitsMaskOffset = (8 - m_bitsPointer - bitsAmount);
            freeBitsMaskOffset = (freeBitsMaskOffset >= 0) ? freeBitsMaskOffset : 0;
            // Remaining bits to read on the next byte in the buffer.
            int leftBitsToRead = bitsAmount - (8 - m_bitsPointer);

            byte data = (byte)((m_currentByte & ((0xFF << m_bitsPointer) & (0xFF >> freeBitsMaskOffset))) >> m_bitsPointer);

            if (leftBitsToRead > 0)
            {
                m_bytePointer += 1;
                m_currentByte = m_buffer[m_bytePointer];

                var alreadyReadBits = (bitsAmount - leftBitsToRead);
                data = (byte)(data | ((m_currentByte << alreadyReadBits) & ((0xFF << alreadyReadBits) & (0xFF >> (8 - bitsAmount)))));
                m_bitsPointer = leftBitsToRead;
            }
            else
            {
                m_bitsPointer += bitsAmount;
            }

            return data;
        }

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadBitsImp2(int bitsAmount)
        {
            if (bitsAmount <= 0) return 0x00;
            if (bitsAmount > 8) bitsAmount = 8;

            if (m_bitsPointer == 0 && bitsAmount == 8)
            {
                m_bytePointer += 1;
                var tmp = m_currentByte;
                m_currentByte = m_buffer[m_bytePointer];
                return tmp;
            }

            var freeBits = 8 - m_bitsPointer;
            var leftBitsAfterRead = freeBits - bitsAmount;

            if (leftBitsAfterRead >= 0)
            {
                byte data = (byte)(m_currentByte >> m_bitsPointer);
                m_bitsPointer += bitsAmount;
                if (m_bitsPointer >= 8)
                {
                    m_bitsPointer = 0;
                    m_bytePointer += 1;
                }
                return (byte)(data & (0xFF >> (8 - bitsAmount)));
            }
            else
            {
                byte dataFirst = (byte)((m_currentByte >> m_bitsPointer) & (0xFF >> m_bitsPointer));
                m_bytePointer += 1;
                m_currentByte = m_buffer[m_bytePointer];
                byte dataSecond = (byte)(m_currentByte & (0xFF >> (bitsAmount - (8 - m_bitsPointer))));
                m_bitsPointer += bitsAmount;
                return (byte)(dataFirst | (dataSecond << (8 - m_bitsPointer)));
            }
        }*/

        public byte[] ToByteArray()
        {
            var bytes = new byte[m_bytePointer + 1];
            System.Buffer.BlockCopy(m_buffer, 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
