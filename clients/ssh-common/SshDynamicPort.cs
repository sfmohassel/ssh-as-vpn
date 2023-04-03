using Renci.SshNet;
using System;

namespace ssh_common
{
  public class SshDynamicPort
  {
    SshClient client;
    ForwardedPort forwardedPort;

    public void Open(OpenSshDynamicPortArgs args)
    {
      // connect to server
      client = new SshClient(args.Ip.Trim(), (int)uint.Parse(args.Port), args.Username.Trim(), args.Password)
      {
        KeepAliveInterval = TimeSpan.FromSeconds(5),
        ConnectionInfo =
        {
          Timeout = TimeSpan.FromSeconds(4),
        }
      };
      client.Connect();

      // create dyanmic port forwarding
      forwardedPort = new ForwardedPortDynamic(Constants.LocalHost, uint.Parse(args.LocalPort));
      client.AddForwardedPort(forwardedPort);
      forwardedPort.Start();
    }

    public void Close()
    {
      try
      {
        forwardedPort?.Stop();
        client?.Disconnect();
      }
      catch { }
    }

    public bool ClientConnected => client?.IsConnected ?? false;
    public bool PortConnected => forwardedPort?.IsStarted ?? false;
  }

  public class OpenSshDynamicPortArgs
  {
    public string Ip { get; set; }
    public string Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public TimeSpan KeepAliveInterval { get; set; }
    public TimeSpan Timeout { get; set; }
    public string LocalPort { get; set; }
  }
}
