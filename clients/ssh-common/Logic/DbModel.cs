using System.Collections.Generic;

namespace ssh_common.Logic
{
  public class DbModel
  {
    public List<SshConnectionModel> Connections { get; set; } = new();
  }

  public class SshConnectionModel
  {
    public bool Selected { get; set; }
    public string Name { get; set; }
    public string Ip { get; set; }
    public string Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string LocalPort { get; set; }
  }
}
