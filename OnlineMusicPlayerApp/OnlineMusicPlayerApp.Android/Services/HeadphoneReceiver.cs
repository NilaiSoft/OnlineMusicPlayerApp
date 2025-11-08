using Android.App;
using Android.Content;
using Android.Media;
using Xamarin.Forms;

[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { AudioManager.ActionAudioBecomingNoisy })]
public class HeadphoneReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == AudioManager.ActionAudioBecomingNoisy)
        {
            DependencyService.Get<IAudioService>()?.Pause();
        }
    }
}
