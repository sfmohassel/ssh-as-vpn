using System.IO;
using System.Text.Json;
using System.Threading;

namespace ssh_common
{
  public abstract class JsonFileDb<TModel> where TModel : new()
  {
    private readonly AutoResetEvent lck = new(true);
    public string DirName { get; private set; }
    public string FileName { get; private set; }

    protected JsonFileDb(string dirName, string fileName)
    {
      DirName = dirName;
      FileName = fileName;
    }

    protected string GetFilePath()
    {
      return Path.Combine(DirName, FileName);
    }

    public virtual void Save(TModel model)
    {
      try
      {
        lck.WaitOne();

        File.WriteAllText(GetFilePath(), JsonSerializer.Serialize(model, new JsonSerializerOptions
        {
          WriteIndented = true
        }));
      }
      finally
      {
        lck.Set();
      }
    }

    public virtual TModel Load()
    {
      try
      {
        lck.WaitOne();

        var file = GetFilePath();
        if (!File.Exists(file))
        {
          File.WriteAllText(file, JsonSerializer.Serialize(new TModel(), new JsonSerializerOptions
          {
            WriteIndented = true
          }));
          return Load();
        }
        return JsonSerializer.Deserialize<TModel>(File.ReadAllText(file));
      }
      finally
      {
        lck.Set();
      }
    }
  }
}