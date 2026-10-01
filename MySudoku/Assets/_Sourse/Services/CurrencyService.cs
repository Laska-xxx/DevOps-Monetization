using System.IO;
using Zenject;

namespace Services
{
    public interface ICurrencyService
    {
        int Balance { get; }
        void Add(int amount);
        bool TrySpend(int amount);
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly string _filePath;
        private int _balance;

        public int Balance => _balance;

        public CurrencyService(string currencyFilePath)
        {
            _filePath = currencyFilePath;
            Load();
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _balance += amount;
            Save();
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || _balance < amount) return false;
            _balance -= amount;
            Save();
            return true;
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _balance = 0;
                return;
            }

            try
            {
                using var stream = new FileStream(_filePath, FileMode.Open);
                using var reader = new BinaryReader(stream);
                _balance = reader.ReadInt32();
            }
            catch (IOException)
            {
                _balance = 0;
            }
        }

        private void Save()
        {
            using var stream = new FileStream(_filePath, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            writer.Write(_balance);
        }
    }
}
