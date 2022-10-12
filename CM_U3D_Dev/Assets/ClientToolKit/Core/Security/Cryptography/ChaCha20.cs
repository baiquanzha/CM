using System;
using MTool.Core.Functional;

namespace MTool.Core.Security.Cryptography
{
    public class ChaCha20
    {
		private uint[] state;
        private uint[] prev;
        private byte[] block;
        private int pos;
        private uint blockCounter;
        private uint[] nonce;
        private uint[] key;
        private const int CACHE_SIZE = 1024;
        private byte[] mCache = new byte[CACHE_SIZE];

		public ChaCha20(byte[] Key, uint BlockCounter, byte[] Nonce)
		{
			this.Reset(Key, BlockCounter, Nonce);
		}

        private static uint ToUInt32(byte[] Buffer, int Start)
		{
			uint Result;
			Start += 3;
			Result = Buffer[Start--];
			Result <<= 8;
			Result |= Buffer[Start--];
			Result <<= 8;
			Result |= Buffer[Start--];
			Result <<= 8;
			Result |= Buffer[Start];
			return Result;
		}


        private void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
		{
			uint i;
			a += b;
			d ^= a;
			i = d;
			d = (i << 16) | (i >> 16);
			c += d;
			b ^= c;
			i = b;
			b = (i << 12) | (i >> 20);
			a += b;
			d ^= a;
			i = d;
			d = (i << 8) | (i >> 24);
			c += d;
			b ^= c;
			i = b;
			b = (i << 7) | (i >> 25);
		}



		/// <summary>
		/// Generates a new block.
        /// <summary>
		public void NextBlock()
		{
			int i, j;
			uint k;
			this.state[0] = 0x61707865;
			this.state[1] = 0x3320646e;
			this.state[2] = 0x79622d32;
			this.state[3] = 0x6b206574;
			Array.Copy(this.key, 0, this.state, 4, 8);
			this.state[12] = this.blockCounter++;
			Array.Copy(this.nonce, 0, this.state, 13, 3);
			Array.Copy(this.state, 0, this.prev, 0, 16);
			for (i = 0; i < 10; i++)
			{
				this.QuarterRound(ref this.state[0], ref this.state[4], ref this.state[08], ref this.state[12]);
				this.QuarterRound(ref this.state[1], ref this.state[5], ref this.state[09], ref this.state[13]);
				this.QuarterRound(ref this.state[2], ref this.state[6], ref this.state[10], ref this.state[14]);
				this.QuarterRound(ref this.state[3], ref this.state[7], ref this.state[11], ref this.state[15]);
				this.QuarterRound(ref this.state[0], ref this.state[5], ref this.state[10], ref this.state[15]);
				this.QuarterRound(ref this.state[1], ref this.state[6], ref this.state[11], ref this.state[12]);
				this.QuarterRound(ref this.state[2], ref this.state[7], ref this.state[08], ref this.state[13]);
				this.QuarterRound(ref this.state[3], ref this.state[4], ref this.state[09], ref this.state[14]);
			}
			for (i = j = 0; i < 16; i++)
			{
				k = this.state[i] + this.prev[i];
				this.state[i] = k;
				this.block[j++] = (byte)k;
				k >>= 8;
				this.block[j++] = (byte)k;
				k >>= 8;
				this.block[j++] = (byte)k;
				k >>= 8;
				this.block[j++] = (byte)k;
			}
			this.pos = 0;
		}


		/// <summary>
		/// Gets the next number of bytes in the stream.
		/// </summary>
		private void GetBytes(int NrBytes)
		{
			int i = 0;
			int j;
			while (i < NrBytes)
			{
				if (this.pos >= 64)
					this.NextBlock();
				j = Math.Min(NrBytes - i, 64 - this.pos);
				Array.Copy(this.block, this.pos, this.mCache, i, j);
				this.pos += j;
				i += j;
			}
		}

        public void Reset(byte[] Key, uint BlockCounter, byte[] Nonce)
        {
            if (Key.Length != 32)
                throw new ArgumentException("ChaCha20 keys must be 32 bytes (256 bits) long.", nameof(Key));
            if (Nonce.Length != 12)
                throw new ArgumentException("ChaCha20 nonces must be 12 bytes (96 bits) long.", nameof(Nonce));
            this.blockCounter = BlockCounter;

            if (this.key == null)
            {
                this.key = new uint[]
                {
                    ToUInt32(Key, 0),
                    ToUInt32(Key, 4),
                    ToUInt32(Key, 8),
                    ToUInt32(Key, 12),
                    ToUInt32(Key, 16),
                    ToUInt32(Key, 20),
                    ToUInt32(Key, 24),
                    ToUInt32(Key, 28)
                };
			}
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    this.key[i] = ToUInt32(Key, i * 4);
                }
            }

			if (this.nonce == null)
			{
				this.nonce = new uint[]
				{
					ToUInt32(Nonce, 0),
					ToUInt32(Nonce, 4),
					ToUInt32(Nonce, 8)
				};
			}
            else
            {
                for (int i = 0; i < 3 ; i++)
                {
                    this.nonce[i] = ToUInt32(Nonce, i * 4);
                }
            }

            if (this.state == null)
            {
                this.state = new uint[16];
			}
            else
            {
				this.state.ForCall((x, y) => { this.state[y] = 0; });
            }

            if (this.prev == null)
            {
				this.prev = new uint[16];
			}
            else
            {
                this.prev.ForCall((x, y) => { this.prev[y] = 0; });
            }

            if (this.block == null)
            {
				this.block = new byte[64];
			}
            else
            {
                this.block.ForCall((x, y) => { this.block[y] = 0; });
            }

			//this.mCache.ForCall((x,y)=>this.mCache[y] = 0);
            this.pos = 64;

		}

		public void EncryptOrDecrypt(byte[] Data)
		{
			this.EncryptOrDecrypt(Data, Data.Length);
		}

		public void EncryptOrDecrypt(byte[] Data, int length)
		{
			int offset = 0;
			int remainSize = length;

			int calCount = 0;
			while (remainSize > 0)
			{
				int calSize = CACHE_SIZE;
				if (remainSize - calSize < 0)
				{
					calSize = remainSize;
				}

				this.EncryptOrDecryptInternal(Data, offset, calSize);
				remainSize -= calSize;
				offset += calSize;
				calCount++;
			}
		}

		private void EncryptOrDecryptInternal(byte[] Data, int offset, int length)
		{
			this.GetBytes(length);
			for (int i = 0; i < length; i++)
			{
				unchecked
				{
					Data[i + offset] ^= this.mCache[i];
				}
			}
		}
	}
}
