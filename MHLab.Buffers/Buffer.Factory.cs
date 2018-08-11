using MHLab.Pooling;
using System;

namespace MHLab.Buffers
{
    public partial class Buffer : IPoolable
    {
        private static readonly Pool<Buffer> Pool = new Pool<Buffer>(10, () => { return new Buffer(); });

        /// <summary>
        /// Returns an instance of NetworkBuffer, from the pool.
        /// </summary>
        /// <param name="size">The size of the underlayer array.</param>
        /// <returns></returns>
        public static Buffer Create(int size)
        {
            var buffer = Pool.Rent();
            buffer.m_buffer = BytesPool.Get(size);

            return buffer;
        }

        /// <summary>
        /// Returns an instance of NetworkBuffer, from the pool.
        /// It is initialized with the passed data.
        /// </summary>
        /// <param name="data">The data to initialize the buffer.</param>
        /// <param name="createCopy">If true, data will be copied in a new array; else data will be used as internal buffer.</param>
        /// <returns></returns>
        public static Buffer Create(byte[] data, bool createCopy = true)
        {
            var buffer = Pool.Rent();

            if (createCopy)
            {
                buffer.m_buffer = BytesPool.Get(data.Length);
                System.Buffer.BlockCopy(data, 0, buffer.m_buffer, 0, data.Length);
            }
            else
            {
                buffer.m_buffer = data;
            }

            return buffer;
        }

        public void Recycle()
        {
            BytesPool.Recycle(m_buffer);
            m_bitsPointer = 0;
            m_buffer = null;
            Pool.Recycle(this);
        }
    }
}