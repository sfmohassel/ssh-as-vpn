using ssh_common;
using System;
using System.Windows.Input;

namespace ssh_as_proxy_client;

public class SshConnectionViewModel : ViewModel
{
  private bool selected;
  public bool Selected
  {
    get { return selected; }
    set
    {
      selected = value;
      OnPropertyChanged(nameof(Opacity));
      OnPropertyChanged();
    }
  }

  public double Opacity => Selected ? 1 : 0.8;

  private string name;
  public string Name
  {
    get { return name; }
    set
    {
      name = value;
      OnPropertyChanged();
    }
  }

  private string ip;
  public string Ip
  {
    get { return ip; }
    set
    {
      ip = value;
      OnPropertyChanged();
    }
  }

  private string port;
  public string Port
  {
    get { return port; }
    set
    {
      port = value;
      OnPropertyChanged(nameof(IsPortValid));
      OnPropertyChanged();
    }
  }

  public bool IsPortValid => uint.TryParse(Port, out var p) && p > 0;

  private string username;
  public string Username
  {
    get { return username; }
    set
    {
      username = value;
      OnPropertyChanged();
    }
  }

  private string password;
  public string Password
  {
    get { return password; }
    set
    {
      password = value;
      OnPropertyChanged();
    }
  }

  private bool showPassword;
  public bool ShowPassword
  {
    get { return showPassword; }
    set
    {
      showPassword = value;
      OnPropertyChanged(nameof(ShowPasswordText));
      OnPropertyChanged();
    }
  }

  public string ShowPasswordText => ShowPassword ? "Hide" : "Show";

  private ICommand toggleShowPasswordCommand;
  public ICommand ToggleShowPasswordCommand => toggleShowPasswordCommand ?? (toggleShowPasswordCommand = new Command((_) => ShowPassword = !ShowPassword));

  private string localPort;
  public string LocalPort
  {
    get { return localPort; }
    set
    {
      localPort = value;
      OnPropertyChanged(nameof(IsLocalPortValid));
      OnPropertyChanged();
    }
  }

  public bool IsLocalPortValid => uint.TryParse(LocalPort, out var p) && p > 0;

  private ICommand selectConnectionCommand;
  public ICommand SelectCommand => selectConnectionCommand ??= new Command((_) => { OnSelected?.Invoke(this); });

  private ICommand deleteCommand;
  public ICommand DeleteCommand => deleteCommand ??= new Command((_) => { OnDeleted?.Invoke(this); });

  public event Action<SshConnectionViewModel> OnSelected, OnDeleted;
}
