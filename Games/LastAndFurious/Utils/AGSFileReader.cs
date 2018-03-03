using System.IO;

namespace LastAndFurious
{
    /// <summary>
    /// Helps to read custom data files written by AGS engine.
    /// </summary>
    public class AGSFileReader
    {
        BinaryReader _r;

        public AGSFileReader(Stream s)
        {
            _r = new BinaryReader(s);
        }

        public int ReadInt()
        {
            if (_r.ReadByte() != 'I')
                throw new IOException("Not a valid integer in AGS safe file format; file could be read in wrong order.");
            return _r.ReadInt32();
        }
    }
}
