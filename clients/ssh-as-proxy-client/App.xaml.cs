using ssh_common;

namespace ssh_as_proxy_client;

public partial class App : Application
{
  public ISystemProxy SystemProxy { get; private init; }

  public App()
  {
    InitializeComponent();

#if WINDOWS
    SystemProxy = ssh_common.Platforms.Windows.WindowsSystemProxy.Instance;
#endif

    MainPage = new AppShell();
  }

  protected override Window CreateWindow(IActivationState activationState)
  {
    var window = base.CreateWindow(activationState);

    window.Destroying += (s, e) =>
    {
      try
      {
        SystemProxy?.Unset();
      }
      catch { }
    };

    return window;
  }
}