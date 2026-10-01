using System.IO;
using Zenject;
using Core.Save;

namespace Services
{
    public interface ISaveService
    {
        bool HasSave { get; }
        void SaveCurrentGame(GameSaveData data);
        GameSaveData? LoadCurrentGame();
        void ClearSave();
    }

    public class SaveService : ISaveService
    {
        private readonly BinarySerializer _serializer = new BinarySerializer();
        private readonly string _savePath;

        public bool HasSave { get; private set; }

        public SaveService(string saveFilePath)
        {
            _savePath = saveFilePath;
            HasSave = File.Exists(_savePath);
        }

        public void SaveCurrentGame(GameSaveData data)
        {
            _serializer.SaveGame(data, _savePath);
            HasSave = true;
        }

        public GameSaveData? LoadCurrentGame() => _serializer.LoadGame(_savePath);

        public void ClearSave()
        {
            _serializer.DeleteSave(_savePath);
            HasSave = false;
        }
    }
}
