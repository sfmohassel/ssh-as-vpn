using ssh_common.Logic;

namespace ssh_as_proxy_client;

public partial class MainPage : ContentPage
{
  public MainPage()
  {
    InitializeComponent();

    var vm = (MainPageVm)BindingContext;
    vm.YesNo += (title, question) => DisplayAlert(title, question, "Yes", "No");
  }
}

public class MainPageVm : PageViewModel<Color>, IDisposable
{
  public MainPageVm() : base(((App)Application.Current).SystemProxy, Db.Instance)
  {
    checkConnectionTimer = Application.Current.Dispatcher.CreateTimer();
    checkConnectionTimer.Interval = TimeSpan.FromSeconds(2);
    checkConnectionTimer.Tick += async (o, e) => { await CheckConnectionAsync(checkConnectionTimer.IsRunning); };
    checkConnectionTimer.Start();
  }

  private readonly IDispatcherTimer checkConnectionTimer;

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
    return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
  }

  protected override Color GetColor(OurColors color)
  {
    switch (color)
    {
      case OurColors.Green:
        return Colors.Green;
      case OurColors.DarkOrange:
        return Colors.DarkOrange;
      case OurColors.Red:
        return Colors.Red;
      case OurColors.Black:
      default:
        return Colors.Black;
    }
  }

  protected override async Task OpenBrowser(string uri)
  {
    await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
  }
}
