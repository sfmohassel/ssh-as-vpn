using System;
using System.Threading;
using System.Threading.Tasks;

namespace ssh_as_proxy_client
{
  public static class Debouncer
  {
    public static Action Debounce(this Action func, int milliseconds = 300)
    {
      CancellationTokenSource cancelTokenSource = null;

      return () =>
      {
        cancelTokenSource?.Cancel();
        cancelTokenSource = new CancellationTokenSource();

        Task.Delay(milliseconds, cancelTokenSource.Token)
          .ContinueWith(t =>
          {
            if (t.IsCompleted)
            {
              func();
            }
          }, TaskScheduler.Default);
      };
    }

    public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300)
    {
      CancellationTokenSource cancelTokenSource = null;

      return arg =>
      {
        cancelTokenSource?.Cancel();
        cancelTokenSource = new CancellationTokenSource();

        Task.Delay(milliseconds, cancelTokenSource.Token)
          .ContinueWith(t =>
          {
            if (t.IsCompleted)
            {
              func(arg);
            }
          }, TaskScheduler.Default);
      };
    }
  }
}