using System;

namespace Core.Generation.PuzzleBank
{
    [Serializable]
    public struct PuzzleBankEntry
    {
        public int Size;                 
        public int Difficulty;    
        public byte[] FullSolution;  
        public byte[] PuzzleMask;   

        public static byte[] ApplyMask(byte[] solution, byte[] mask)
        {
            var result = new byte[solution.Length];
            for (int i = 0; i < solution.Length; i++)
                result[i] = mask[i] == 1 ? (byte)0 : solution[i];
            return result;
        }
    }
}
