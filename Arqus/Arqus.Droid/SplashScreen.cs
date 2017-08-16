using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Arqus.Droid
{

    [Activity (Label = "Qualisys Viewfinder", Icon = "@drawable/ic_launcher", MainLauncher = true, Theme = "@style/Theme.Splash", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    // This class is used to display a splash screen and load the main activity
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Load main application
            StartActivity(typeof(MainActivity));
        }
        
        // Called when coming back from main menu
        protected override void OnRestart()
        {
            base.OnRestart();

            // Say good night!
            Finish();
            Process.KillProcess(Process.MyPid());
        }
    }
}