using ProtoBuf;
using Pulsar.Common.Messages.Other;
using System;
using System.IO;

namespace Pulsar.Common.Networking
{
    public class PayloadReader : IDisposable
    {
        private readonly Stream _innerStream;
        private readonly bool _leaveInnerStreamOpen;

        public PayloadReader(byte[] payload, int length, bool leaveInnerStreamOpen)
        {
            _innerStream = new BufferedStream(new MemoryStream(payload, 0, length, false, true));
            _leaveInnerStreamOpen = leaveInnerStreamOpen;
        }

        public PayloadReader(Stream stream, bool leaveInnerStreamOpen)
        {
            _innerStream = stream is BufferedStream ? stream : new BufferedStream(stream);
            _leaveInnerStreamOpen = leaveInnerStreamOpen;
        }

        public int ReadInteger()
        {
            byte[] buffer = new byte[4];
            ReadExactly(_innerStream, buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public byte[] ReadBytes(int length)
        {
            if (_innerStream.Position + length > _innerStream.Length)
                throw new OverflowException("Unable to read " + length + " bytes from stream");
            byte[] buffer = new byte[length];
            ReadExactly(_innerStream, buffer, 0, length);
            return buffer;
        }

        public IMessage ReadMessage()
        {
            ReadInteger();
            return Serializer.Deserialize<IMessage>(_innerStream);
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
        {
            int read, total = 0;
            while (total < count && (read = stream.Read(buffer, offset + total, count - total)) > 0)
                total += read;
            if (total < count)
                throw new EndOfStreamException();
        }

        public void Dispose()
        {
            if (!_leaveInnerStreamOpen)
                _innerStream.Close();
            else if (_innerStream is BufferedStream)
                ((BufferedStream)_innerStream).Flush();
        }
    }
}
