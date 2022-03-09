using System;

using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Runtime;
using Android.OS;
using MediaManager;

namespace PartyLooper.Droid
{
    [Activity(
        Label = "PartyLooper", 
        Icon = "@mipmap/ic_launcher", 
        Theme = "@style/MainTheme", 
        LaunchMode = LaunchMode.SingleTask,
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize,
        ScreenOrientation = ScreenOrientation.Portrait
    )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CrossMediaManager.Current.Init(this);
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#6fccaa"));
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}