using ssh_as_proxy_client;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace ssh_common.Logic
{
  public abstract class PageViewModel<TColor> : ViewModel
  {
    public PageViewModel(ISystemProxy systemProxy, JsonFileDb<DbModel> db)
    {
      this.systemProxy = systemProxy;
      this.db = db;
      Connections.CollectionChanged += CallDebouncedSave;
      debouncedSave = Debouncer.Debounce(new Action(Save), 1000);
      port = new SshDynamicPort();

      Connected = false;

      Load();
      if (Connections.Count == 1 && !isLoading)
      {
        Connections[0].SelectCommand.Execute(Connections[0]);
      }
    }

    public event Func<string, string, Task<bool>> YesNo;

    readonly AutoResetEvent lck = new(true);

    readonly ISystemProxy systemProxy;
    readonly JsonFileDb<DbModel> db;

    protected abstract bool IsConnectedToInternet();
    protected abstract TColor GetColor(OurColors color);
    protected abstract Task OpenBrowser(string uri);

    readonly Action debouncedSave;
    readonly SshDynamicPort port;

    private volatile bool isLoading = false;

    private SshConnectionViewModel activeConn;
    private SshConnectionViewModel ActiveConn
    {
      get => activeConn;
      set
      {
        activeConn = value;
        OnPropertyChanged(nameof(CanSetSystemProxy));
        OnPropertyChanged();
      }
    }

    public ObservableCollection<SshConnectionViewModel> Connections { get; } = new ObservableCollection<SshConnectionViewModel>();

    private SshConnectionViewModel editingConn;
    public SshConnectionViewModel EditingConn
    {
      get { return editingConn; }
      set
      {
        if (editingConn != null)
        {
          editingConn.PropertyChanged -= CallDebouncedSave;
        }
        editingConn = value;
        if (editingConn != null)
        {
          editingConn.PropertyChanged += CallDebouncedSave;
        }
        OnPropertyChanged();
      }
    }

    private void Load()
    {
      try
      {
        isLoading = true;

        var model = db.Load();
        foreach (var c in model.Connections)
        {
          AddConnection(new SshConnectionViewModel
          {
            Selected = c.Selected,
            Ip = c.Ip,
            Port = c.Port,
            Username = c.Username,
            Password = c.Password,
            ShowPassword = false,
            LocalPort = c.LocalPort,
            Name = c.Name,
          });
        }
      }
      finally
      {
        isLoading = false;
      }
    }

    private void Save()
    {
      db.Save(new()
      {
        Connections = Connections.Select(c => new SshConnectionModel()
        {
          Ip = c.Ip,
          LocalPort = c.LocalPort,
          Name = c.Name,
          Password = c.Password,
          Port = c.Port,
          Selected = c.Selected,
          Username = c.Username
        }).ToList()
      });
    }

    private void CallDebouncedSave(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (isLoading) return;
      debouncedSave.Invoke();
    }

    private void CallDebouncedSave(object sender, PropertyChangedEventArgs e)
    {
      if (isLoading) return;
      debouncedSave.Invoke();
    }

    private void AddConnection(SshConnectionViewModel conn)
    {
      conn.OnSelected += SelectConnection;
      conn.OnDeleted += DeleteConnection;
      Connections.Add(conn);

      if (Connections.Count == 1 && !isLoading)
      {
        conn.SelectCommand.Execute(conn);
      }
    }

    private void SelectConnection(SshConnectionViewModel sshConnection)
    {
      foreach (var item in Connections)
      {
        item.Selected = false;
      }
      EditingConn = sshConnection;
      if (EditingConn != null) EditingConn.Selected = true;
    }

    private async void DeleteConnection(SshConnectionViewModel sshConnection)
    {
      var answer = await YesNo.Invoke("Are you sure?", "You cannot recover a deleted connection!");
      if (!answer) return;

      sshConnection.OnSelected -= SelectConnection;
      sshConnection.OnDeleted -= DeleteConnection;

      Connections.Remove(sshConnection);

      if (sshConnection.Selected)
      {
        SelectConnection(Connections.Any() ? Connections[0] : null);
      }
    }

    private bool connected;
    public bool Connected
    {
      get { return connected; }
      set
      {
        connected = value;
        OnPropertyChanged(nameof(ButtonText));
        OnPropertyChanged();
      }
    }

    private string status;
    public string Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged();
      }
    }

    private TColor statusColor;
    public TColor StatusColor
    {
      get { return statusColor; }
      set
      {
        statusColor = value;
        OnPropertyChanged();
      }
    }

    private bool autoReconnect;
    public bool AutoConnect
    {
      get { return autoReconnect; }
      set
      {
        autoReconnect = value;
        OnPropertyChanged();
      }
    }

    private string buttonText;
    public string ButtonText
    {
      get { return buttonText; }
      set
      {
        buttonText = value;
        OnPropertyChanged();
      }
    }

    public bool CanSetSystemProxy => ActiveConn?.IsLocalPortValid ?? false;

    private string currentSystemProxy;
    public string CurrentSystemProxy
    {
      get { return currentSystemProxy; }
      set
      {
        currentSystemProxy = value;
        OnPropertyChanged();
      }
    }

    private Command toggleCommand;
    public Command ToggleCommand => toggleCommand ??= new Command(async (_) => { await ToggleConnectionAsync(); });

    private Command clearSystemProxyCommand;
    public Command ClearSystemProxyCommand => clearSystemProxyCommand ??= new Command(async (_) => await ClearSystemProxyAsync());

    private Command setSystemProxyCommand;
    public Command SetSystemProxyCommand => setSystemProxyCommand ??= new Command(async (_) => await SetSystemProxyAsync());

    private Command openMyIpCommand;
    public Command OpenMyIpCommand => openMyIpCommand ??= new Command(async (_) => { await OpenMyIpAsync(); });

    private Command addConnectionCommand;
    public Command AddConnectionCommand => addConnectionCommand ??= new Command((_) =>
    {
      var newConn = new SshConnectionViewModel
      {
        Ip = "x.y.z.m",
        Port = "22",
        LocalPort = "12000",
        Name = "New connection",
        Password = "",
        ShowPassword = true,
        Username = "",
      };
      AddConnection(newConn);

      newConn.SelectCommand.Execute(newConn);
    });

    private async Task ToggleConnectionAsync()
    {
      if (Connected)
      {
        await DisconnectAsync();
      }
      else
      {
        await ConnectAsync();
      }
    }

    private async Task ConnectAsync()
    {
      // disconnect from server
      await DisconnectAsync();

      if (EditingConn == null) return;

      ActiveConn = EditingConn;

      if (!ActiveConn.IsPortValid || !ActiveConn.IsLocalPortValid) return;

      try
      {
        lck.WaitOne();

        port.Open(new OpenSshDynamicPortArgs
        {
          Ip = ActiveConn.Ip,
          Port = ActiveConn.Port,
          Username = ActiveConn.Username,
          Password = ActiveConn.Password,
          KeepAliveInterval = TimeSpan.FromSeconds(10),
          Timeout = TimeSpan.FromSeconds(5),
          LocalPort = ActiveConn.LocalPort,
        });

        // set system-wide socks proxy
        await SetSystemProxyAsync();
      }
      catch
      {
        // ignore
      }
      finally
      {
        lck.Set();
      }

      UpdateStatus();
    }

    private async Task DisconnectAsync()
    {
      try
      {
        port.Close();

        ActiveConn = null;
        await ClearSystemProxyAsync();
      }
      finally
      {
        Connected = false;
      }
    }

    protected async Task CheckConnectionAsync(bool isTimerRunning)
    {
      try
      {
        if (!isTimerRunning) return;
        UpdateStatus();

        if (!Connected)
        {
          await ClearSystemProxyAsync();

          if (AutoConnect)
          {
            await ConnectAsync();
          }
        }
      }
      catch { }
    }

    private void UpdateStatus()
    {
      var netConnected = IsConnectedToInternet();

      Connected = netConnected && port.ClientConnected && port.PortConnected;
      CurrentSystemProxy = $"Current system proxy: {systemProxy?.Current() ?? "Unknown!"}";

      if (Connected)
      {
        Status = $"Connected to {ActiveConn?.Ip}:{ActiveConn?.Port}";
        StatusColor = GetColor(OurColors.Green);
        ButtonText = "Click to disconnect";
      }
      else if (AutoConnect)
      {
        Status = "Disconnected. Trying to reconnect...";
        StatusColor = GetColor(OurColors.DarkOrange);
        ButtonText = "Reconnecting...";
      }
      else
      {
        Status = "Disconnected!";
        StatusColor = GetColor(OurColors.Red);
        ButtonText = "Connect";
      }
    }

    private async Task OpenMyIpAsync()
    {
      await OpenBrowser("http://ip-api.com/json/?fields=query");
    }

    private async Task SetSystemProxyAsync()
    {
      if (!CanSetSystemProxy) return;

      await systemProxy?.SetSocks(Constants.LocalHost, ActiveConn.LocalPort.ToString());
    }

    private async Task ClearSystemProxyAsync()
    {
      await systemProxy?.Unset();
    }
  }
}
