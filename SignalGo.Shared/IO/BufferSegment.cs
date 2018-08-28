using System.Linq;

namespace SignalGo.Shared.IO
{
    public class BufferSegment
    {
        public byte[] Buffer { get; set; }
        public int Position { get; set; } = 0;
        public bool IsFinished
        {
            get
            {
                return Position == Buffer.Length;
            }
        }

        public byte ReadFirstByte()
        {
            byte result = Buffer[Position];
            Position++;
            return result;
        }

        public byte[] ReadBufferSegment(int count, out int readCount)
        {
            if (count > Buffer.Length)
            {
                byte[] result = Buffer.Skip(Position).ToArray();
                readCount = result.Length;
                Position = Buffer.Length;
                return result;
            }
            else
            {
                byte[] result = Buffer.Skip(Position).Take(count).ToArray();
                readCount = result.Length;
                Position += readCount;
                return result;
            }
        }

        public byte[] Read(byte[] exitBytes, out bool isFound)
        {
            isFound = false;
            int startPosition = Position;
            for (int i = Position; i < Buffer.Length; i++)
            {
                if (Buffer.Skip(i).Take(exitBytes.Length).SequenceEqual(exitBytes))
                {
                    isFound = true;
                    Position += exitBytes.Length;
                    break;
                }
                Position++;
            }
            int endPosition = Position;
            return Buffer.Skip(startPosition).Take(endPosition).ToArray();
        }
    }
}
