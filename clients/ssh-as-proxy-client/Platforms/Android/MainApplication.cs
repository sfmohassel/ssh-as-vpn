﻿using Android.App;
using Android.Runtime;

namespace ssh_as_proxy_client;

[Application]
public class MainApplication : MauiApplication
{
  public MainApplication(IntPtr handle, JniHandleOwnership ownership)
    : base(handle, ownership)
  {
  }

  protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}