using ssh_common;
using ssh_common.Logic;
using System;

namespace ssh_proxifier
{
  public class Db : JsonFileDb<DbModel>
  {
    public static Db Instance = new Db();

    private Db() : base(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ssh-proxifier.json")
    {
    }
  }
}
