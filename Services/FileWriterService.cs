using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Serialization;
using Interfaces;
namespace Services
{
    public class FileWriterService : IFileWriterService
    {
        private string _dictionaryPath;
        private DateTime _dateTime;
        public FileWriterService()
        {
            _dateTime = DateTime.Now;
            string dateTimeString = _dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
            _dictionaryPath = Path.Combine(Directory.GetCurrentDirectory(), "output", dateTimeString);
            // สร้างโฟลเดอร์ output ถ้ายังไม่มี
            Directory.CreateDirectory(_dictionaryPath);
        }
        public string GetDictionaryPath()
        {
            return _dictionaryPath;
        }
        public DateTime GetDateTime()
        {
            return _dateTime;
        }
        public void WriteJson<T>(string fileName, T data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            string filePath = Path.Combine(_dictionaryPath, fileName);
            string directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;
            Directory.CreateDirectory(directoryPath);
            using (StreamWriter writer = new StreamWriter(filePath, append: false))
            {
                writer.Write(json);
            }
        }

        public void WriteXml<T>(string fileName, T data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            string filePath = Path.Combine(_dictionaryPath, fileName);
            using (StreamWriter writer = new StreamWriter(filePath, append: false))
            {
                serializer.Serialize(writer, data);
            }
        }

        public void WriteText(string fileName, string text)
        {
            string _filePath = Path.Combine(_dictionaryPath, fileName);
            using (StreamWriter writer = new StreamWriter(_filePath, append: false))
            {
                writer.Write(text);
            }
        }
    }
}