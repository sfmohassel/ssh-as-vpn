using System;
using System.Threading.Tasks;

namespace ssh_common
{
  public interface ISystemProxy
  {
    Task<ProxyResult> SetSocks(string ip, string port);
    Task<ProxyResult> SetHttp(string ip, string port);
    Task<ProxyResult> SetHttps(string ip, string port);
    Task<ProxyResult> Unset();
    string? Current();
  }

  public class ProxyResult
  {
    public Exception Exception { get; set; }
    public bool Success { get; set; }
  }
}