using Newtonsoft.Json;
using System.Text;

public static class Util
{
    public static class FileExtension
    {
        //Creates a file and all necessary subdirectories
        public static void CreateFileWithPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            string directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            File.Create(path).Dispose();
        }
    }
    public static T LoadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            FileExtension.CreateFileWithPath(path);
            return (T)Activator.CreateInstance(typeof(T));
        }
        using FileStream fs = File.OpenRead(path);
        using StreamReader sr = new StreamReader(fs, new UTF8Encoding(false));

        string output = sr.ReadToEnd();
        T obj = default;
        if (JsonConvert.DeserializeObject(output) != null)
        {
            obj = JsonConvert.DeserializeObject<T>(output);
        }
        return obj != null ? obj : (T)Activator.CreateInstance(typeof(T));
    }
    public static void SaveJson(object toSave, string path)
    {
        using StringWriter sw = new StringWriter();
        using JsonTextWriter jw = new JsonTextWriter(sw);

        JsonSerializer ser = new JsonSerializer();
        ser.Serialize(jw, toSave);
        sw.ToString();
        File.WriteAllText(path, sw.ToString());
    }
    public static void AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            TKey key,
            TValue value)
    {
        if (dict.TryGetValue(key, out _))
        {
            dict.Remove(key);
        }
        dict.Add(key, value);
    }
}
