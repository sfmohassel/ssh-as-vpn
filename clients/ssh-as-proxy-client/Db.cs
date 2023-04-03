using ssh_common;
using ssh_common.Logic;

namespace ssh_as_proxy_client;

public class Db : JsonFileDb<DbModel>
{
  public static Db Instance = new();

  private Db() : base(FileSystem.Current.AppDataDirectory, "ssh-proxifier.json")
  {
  }
}