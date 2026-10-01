namespace Core.Save
{
    public static class Crc32
    {
        private static readonly uint[] Table = BuildTable();

        private static uint[] BuildTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int k = 0; k < 8; k++)
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                table[i] = c;
            }
            return table;
        }

        public static uint Compute(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                byte index = (byte)((crc ^ b) & 0xFF);
                crc = (crc >> 8) ^ Table[index];
            }
            return crc ^ 0xFFFFFFFF;
        }
    }
}
