namespace Interfaces
{
    using System.Threading.Tasks;
    using Models;

    public interface IFileWriterService
    {
        string GetDictionaryPath();
        void WriteJson<T>(string fileName, T obj);
        void WriteXml<T>(string fileName, T obj);
        void WriteText(string fileName, string text);
    }
}