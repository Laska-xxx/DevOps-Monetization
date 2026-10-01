using System;
using System.Collections.Generic;
using System.IO;
using Core.Board;

namespace Core.Save
{
    public class BinarySerializer
    {
        private const int SaveFormatVersion = 1;
        private static readonly byte[] Signature = { (byte)'S', (byte)'U', (byte)'D', (byte)'K' };

        public void SaveGame(GameSaveData data, string path)
        {
            byte[] payload = SerializeGame(data);
            WriteAtomic(path, payload);
        }

        public GameSaveData? LoadGame(string path)
        {
            if (!File.Exists(path)) return null;

            byte[] bytes = File.ReadAllBytes(path);
            if (!TryValidateAndStrip(bytes, out byte[] payload)) return null;

            using var stream = new MemoryStream(payload);
            using var reader = new BinaryReader(stream);

            int version = reader.ReadInt32();
            if (version != SaveFormatVersion)
            {
                return null;
            }

            var data = new GameSaveData
            {
                Size = (BoardSize)reader.ReadByte(),
                Difficulty = (DifficultyLevel)reader.ReadByte(),
                IsExtraMode = reader.ReadBoolean(),
                SolutionCells = ReadByteArray(reader),
                CurrentCells = ReadByteArray(reader),
                IsFixedCell = ReadBoolArrayPacked(reader)
            };

            int notesCount = reader.ReadInt32();
            data.Notes = new List<NoteEntry>(notesCount);
            for (int i = 0; i < notesCount; i++)
            {
                data.Notes.Add(new NoteEntry
                {
                    CellIndex = reader.ReadInt32(),
                    NotesMask = reader.ReadUInt16()
                });
            }

            data.Score = reader.ReadInt32();
            data.Health = reader.ReadInt32();
            data.ElapsedSeconds = reader.ReadSingle();
            data.ChillModeActive = reader.ReadBoolean();

            int historyCount = reader.ReadInt32();
            data.UndoHistory = new List<CommandSnapshot>(historyCount);
            for (int i = 0; i < historyCount; i++)
            {
                data.UndoHistory.Add(new CommandSnapshot
                {
                    CommandType = reader.ReadByte(),
                    Row = reader.ReadInt32(),
                    Col = reader.ReadInt32(),
                    PrevValue = reader.ReadInt32(),
                    NewValue = reader.ReadInt32()
                });
            }

            return data;
        }

        public void DeleteSave(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private byte[] SerializeGame(GameSaveData data)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(SaveFormatVersion);
                writer.Write((byte)data.Size);
                writer.Write((byte)data.Difficulty);
                writer.Write(data.IsExtraMode);

                WriteByteArray(writer, data.SolutionCells);
                WriteByteArray(writer, data.CurrentCells);
                WriteBoolArrayPacked(writer, data.IsFixedCell);

                writer.Write(data.Notes.Count);
                foreach (var note in data.Notes)
                {
                    writer.Write(note.CellIndex);
                    writer.Write(note.NotesMask);
                }

                writer.Write(data.Score);
                writer.Write(data.Health);
                writer.Write(data.ElapsedSeconds);
                writer.Write(data.ChillModeActive);

                writer.Write(data.UndoHistory.Count);
                foreach (var cmd in data.UndoHistory)
                {
                    writer.Write(cmd.CommandType);
                    writer.Write(cmd.Row);
                    writer.Write(cmd.Col);
                    writer.Write(cmd.PrevValue);
                    writer.Write(cmd.NewValue);
                }
            }

            return stream.ToArray();
        }

        public void SaveStats(StatsData data, string path)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(SaveFormatVersion);
                writer.Write(data.Entries.Count);
                foreach (var kvp in data.Entries)
                {
                    writer.Write((byte)kvp.Key.Size);
                    writer.Write((byte)kvp.Key.Difficulty);
                    writer.Write(kvp.Key.IsExtraMode);

                    writer.Write(kvp.Value.GamesStarted);
                    writer.Write(kvp.Value.GamesCompleted);
                    writer.Write(kvp.Value.AverageScore);
                    writer.Write(kvp.Value.AverageTimeSeconds);
                    writer.Write(kvp.Value.AverageHealthSpent);
                    writer.Write(kvp.Value.BestScore);
                }
            }

            WriteAtomic(path, stream.ToArray());
        }

        public StatsData LoadStats(string path)
        {
            var empty = new StatsData { Entries = new Dictionary<StatsKey, DifficultyStats>() };
            if (!File.Exists(path)) return empty;

            byte[] bytes = File.ReadAllBytes(path);
            if (!TryValidateAndStrip(bytes, out byte[] payload)) return empty;

            using var stream = new MemoryStream(payload);
            using var reader = new BinaryReader(stream);

            int version = reader.ReadInt32();
            if (version != SaveFormatVersion) return empty;

            int count = reader.ReadInt32();
            var entries = new Dictionary<StatsKey, DifficultyStats>(count);

            for (int i = 0; i < count; i++)
            {
                var key = new StatsKey
                {
                    Size = (BoardSize)reader.ReadByte(),
                    Difficulty = (DifficultyLevel)reader.ReadByte(),
                    IsExtraMode = reader.ReadBoolean()
                };

                var value = new DifficultyStats
                {
                    GamesStarted = reader.ReadInt32(),
                    GamesCompleted = reader.ReadInt32(),
                    AverageScore = reader.ReadSingle(),
                    AverageTimeSeconds = reader.ReadSingle(),
                    AverageHealthSpent = reader.ReadSingle(),
                    BestScore = reader.ReadInt32()
                };

                entries[key] = value;
            }

            return new StatsData { Entries = entries };
        }

        private void WriteAtomic(string path, byte[] payload)
        {
            uint crc = Crc32.Compute(payload);
            string tmpPath = path + ".tmp";

            using (var stream = new FileStream(tmpPath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Signature);
                writer.Write(crc);
                writer.Write(payload.Length);
                writer.Write(payload);
            }

            if (File.Exists(path)) File.Delete(path);
            File.Move(tmpPath, path);
        }

        private bool TryValidateAndStrip(byte[] raw, out byte[] payload)
        {
            payload = null;
            if (raw.Length < Signature.Length + sizeof(uint) + sizeof(int)) return false;

            for (int i = 0; i < Signature.Length; i++)
                if (raw[i] != Signature[i]) return false;

            int offset = Signature.Length;
            uint expectedCrc = BitConverter.ToUInt32(raw, offset);
            offset += sizeof(uint);

            int payloadLength = BitConverter.ToInt32(raw, offset);
            offset += sizeof(int);

            if (raw.Length - offset < payloadLength) return false;

            payload = new byte[payloadLength];
            Array.Copy(raw, offset, payload, 0, payloadLength);

            uint actualCrc = Crc32.Compute(payload);
            return actualCrc == expectedCrc;
        }

        private static void WriteByteArray(BinaryWriter writer, byte[] array)
        {
            writer.Write(array.Length);
            writer.Write(array);
        }

        private static byte[] ReadByteArray(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }

        private static void WriteBoolArrayPacked(BinaryWriter writer, bool[] array)
        {
            writer.Write(array.Length);
            int byteCount = (array.Length + 7) / 8;
            var packed = new byte[byteCount];

            for (int i = 0; i < array.Length; i++)
                if (array[i]) packed[i / 8] |= (byte)(1 << (i % 8));

            writer.Write(packed);
        }

        private static bool[] ReadBoolArrayPacked(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            int byteCount = (length + 7) / 8;
            byte[] packed = reader.ReadBytes(byteCount);

            var result = new bool[length];
            for (int i = 0; i < length; i++)
                result[i] = (packed[i / 8] & (1 << (i % 8))) != 0;

            return result;
        }
    }
}
