/* Copyright (C) 2018 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Text;

namespace HttpPack
{
    // base frame protocol
    // http://tools.ietf.org/html/rfc6455#section-5.1
    /*
     0               1               2               3              
     0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
    +-+-+-+-+-------+-+-------------+-------------------------------+
    |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
    |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
    |N|V|V|V|       |S|             |   (if payload len==126/127)   |
    | |1|2|3|       |K|             |                               |
    +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
     4               5               6               7              
    + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
    |     Extended payload length continued, if payload len == 127  |
    + - - - - - - - - - - - - - - - +-------------------------------+
     8               9               10              11             
    + - - - - - - - - - - - - - - - +-------------------------------+
    |                               |Masking-key, if MASK set to 1  |
    +-------------------------------+-------------------------------+
     12              13              14              15
    +-------------------------------+-------------------------------+
    | Masking-key (continued)       |          Payload Data         |
    +-------------------------------- - - - - - - - - - - - - - - - +
    :                     Payload Data continued ...                :
    + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
    |                     Payload Data continued ...                |
    +---------------------------------------------------------------+
     */

    public enum OPCODE
    {
        ContinuationFrame = 0x0,
        TextFrame = 0x1,
        BinaryFrame = 0x2,
        ConnectionClose = 0x8,
        Ping = 0x9,
        Pong = 0xA
    }

    public class HttpWebSocketFrame
    {
        public HttpWebSocketFrame(bool fin, bool mask, byte[] maskKey, OPCODE opCode, byte[] payload)
        {
            Fin = fin;
            Mask = mask;
            MaskKey = maskKey;
            OpCode = opCode;
            PayloadLen = (uint) payload.Length;
            Payload = payload;
        }

        public bool Fin { get; }
        public bool Mask { get; }
        public byte[] MaskKey { get; }
        public OPCODE OpCode { get; }
        public uint PayloadLen { get; }
        public byte[] Payload { get; }

        public byte[] EncodedFrame
        {
            get
            {
                var message = new byte[] {0, 0};
                message[0] ^= (byte) (Fin ? 0x80 : 0x0);
                message[0] ^= (byte) (int) OpCode;
                message[1] ^= (byte) (Mask ? 0x80 : 0x0);

                if (PayloadLen < 126)
                {
                    message[1] ^= (byte) PayloadLen;
                }
                else if (PayloadLen <= ushort.MaxValue)
                {
                    message[1] ^= 126;
                    message = CombineByteArrays(message, Split((ushort) PayloadLen));
                }
                else
                {
                    message[1] ^= 127;
                    message = CombineByteArrays(message, Split(PayloadLen));
                }

                var encodedPayload = Payload;

                if (Mask)
                {
                    message = CombineByteArrays(message, MaskKey);
                    for (var i = 0; i < PayloadLen; i++)
                    {
                        encodedPayload[i] = (byte) (Payload[i] ^ MaskKey[i % 4]);
                    }
                }

                message = CombineByteArrays(message, encodedPayload);

                return message;
            }
        }

        public static HttpWebSocketFrame EncodeFrame(byte[] payload)
        {
            return new HttpWebSocketFrame(true, false, new byte[] { }, OPCODE.BinaryFrame, payload);
        }

        public static HttpWebSocketFrame EncodeFrame(string text)
        {
            return new HttpWebSocketFrame(true, false, new byte[] { }, OPCODE.TextFrame, Encoding.ASCII.GetBytes(text));
        }

        public static HttpWebSocketFrame DecodeFrame(byte[] message)
        {
            var fin = (message[0] & 0x80) > 0;
            var opCode = (OPCODE) (message[0] & ~0xF0);

            var mask = (message[1] & 0x80) > 0;
            var payloadLength = (uint) (message[1] & ~0x80);

            uint payloadStartByte = 2;
            if (payloadLength == 126)
            {
                payloadStartByte = 4;

                payloadLength = BitConverter.ToUInt16(ReverseBytes(SubSet(message, 2, 2)), 0);
            }
            else if (payloadLength == 127)
            {
                payloadStartByte = 8;
                payloadLength = BitConverter.ToUInt16(ReverseBytes(SubSet(message, 4, 4)), 0);
            }

            var maskKey = new byte[] { };
            if (mask)
            {
                maskKey = SubSet(message, (int) payloadStartByte, 4);
                payloadStartByte += 4;
            }

            var payload = SubSet(message, (int) payloadStartByte, (int) payloadLength);

            // decode payload
            if (mask)
            {
                for (var i = 0; i < payloadLength; i++)
                {
                    payload[i] = (byte) (payload[i] ^ maskKey[i % 4]);
                }
            }

            return new HttpWebSocketFrame(fin, mask, maskKey, opCode, payload);
        }

        private static byte[] CombineByteArrays(byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);

            return result;
        }

        private static byte[] Split(ushort u16)
        {
            var lower = (byte) (u16 & 0xff);
            var upper = (byte) (u16 >> 8);
            return new[] {upper, lower};
        }

        private static byte[] Split(uint u32)
        {
            var lower = (ushort) (u32 & 0xffff);
            var upper = (ushort) (u32 >> 16);

            return CombineByteArrays(Split(upper), Split(lower));
        }

        private static uint[] SplitU64(ulong u64)
        {
            var lower = (uint) (u64 & 0xffffffff);
            var upper = (uint) (u64 >> 32);

            return new[] {upper, lower};
        }

        private static byte[] ReverseBytes(byte[] source)
        {
            Array.Reverse(source);
            return source;
        }

        private static byte[] SubSet(byte[] source, int first, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(source, first, result, 0, length);

            return result;
        }
    }
}