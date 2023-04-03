using System.Runtime.InteropServices;

namespace ssh_common.Platforms.Windows
{
  public static class InternetAvailability
  {
    [DllImport("wininet.dll")]
    private extern static bool InternetGetConnectedState(out int description, int reservedValue);

    public static bool IsAvailable()
    {
      int description;
      return InternetGetConnectedState(out description, 0);
    }
  }
}
