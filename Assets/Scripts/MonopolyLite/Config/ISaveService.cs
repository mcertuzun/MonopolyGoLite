using MonopolyLite.Data;

namespace MonopolyLite.Config
{
    public interface ISaveService
    {
        bool HasSave();
        SaveData Load();
        void Save(SaveData data);
        void Delete();
    }
}
