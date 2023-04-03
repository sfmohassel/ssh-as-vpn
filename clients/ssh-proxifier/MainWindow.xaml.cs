using ssh_common.Logic;
using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using ssh_common.Platforms.Windows;
using System.Windows.Threading;

namespace ssh_proxifier
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      var vm = (MainPageVm)DataContext;
      vm.YesNo += (title, question) =>
      {
        var resp = MessageBox.Show(question, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        return Task.FromResult(resp);
      };
    }
  }

  public class MainPageVm : PageViewModel<Brush>, IDisposable
  {
    public MainPageVm() : base(((App)Application.Current).SystemProxy, Db.Instance)
    {
      checkConnectionTimer = new DispatcherTimer
      {
        Interval = TimeSpan.FromSeconds(2)
      };
      checkConnectionTimer.Tick += async (o, e) => { await CheckConnectionAsync(checkConnectionTimer.IsEnabled); };
      checkConnectionTimer.Start();
    }

    private readonly DispatcherTimer checkConnectionTimer;

    public void Dispose()
    {
      try
      {
        checkConnectionTimer.Stop();
      }
      catch { }
    }

    protected override bool IsConnectedToInternet()
    {
      return InternetAvailability.IsAvailable();
    }

    protected override Brush GetColor(OurColors color)
    {
      switch (color)
      {
        case OurColors.Green:
          return new SolidColorBrush(Colors.Green);
        case OurColors.DarkOrange:
          return new SolidColorBrush(Colors.DarkOrange);
        case OurColors.Red:
          return new SolidColorBrush(Colors.Red);
        case OurColors.Black:
        default:
          return new SolidColorBrush(Colors.Black);
      }
    }

    protected override Task OpenBrowser(string uri)
    {
      Process.Start(new ProcessStartInfo
      {
        FileName = uri,
        UseShellExecute = true
      });
      return Task.CompletedTask;
    }
  }

}
