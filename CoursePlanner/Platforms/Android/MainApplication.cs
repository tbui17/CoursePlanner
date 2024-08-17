using Android;
using Android.App;
using Android.Runtime;

[assembly: UsesPermission(Manifest.Permission.WakeLock)]
[assembly: UsesPermission(Manifest.Permission.ReceiveBootCompleted)]
[assembly: UsesPermission(Manifest.Permission.Vibrate)]
[assembly: UsesPermission(Manifest.Permission.PostNotifications)]
namespace CoursePlanner
{

    
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership) { }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}