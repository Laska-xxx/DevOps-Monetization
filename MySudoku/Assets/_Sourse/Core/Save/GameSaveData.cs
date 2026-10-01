using System.Collections.Generic;
using Core.Board;

namespace Core.Save
{
    public struct GameSaveData
    {
        public BoardSize Size;
        public DifficultyLevel Difficulty;
        public bool IsExtraMode;

        public byte[] SolutionCells;
        public byte[] CurrentCells;
        public bool[] IsFixedCell;

        public List<NoteEntry> Notes;

        public int Score;
        public int Health;
        public float ElapsedSeconds;
        public bool ChillModeActive;

        public List<CommandSnapshot> UndoHistory;
    }

    public struct NoteEntry
    {
        public int CellIndex;
        public ushort NotesMask;
    }

    public struct CommandSnapshot
    {
        public byte CommandType; 
        public int Row;
        public int Col;
        public int PrevValue;
        public int NewValue;
    }
}
