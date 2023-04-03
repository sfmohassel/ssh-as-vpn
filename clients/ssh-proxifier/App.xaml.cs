using ssh_common;
using ssh_common.Platforms.Windows;
using System.Windows;

namespace ssh_proxifier
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public ISystemProxy SystemProxy => WindowsSystemProxy.Instance;

    protected override void OnExit(ExitEventArgs e)
    {
      try
      {
        SystemProxy?.Unset();
      }
      catch { }
    }
  }
}
