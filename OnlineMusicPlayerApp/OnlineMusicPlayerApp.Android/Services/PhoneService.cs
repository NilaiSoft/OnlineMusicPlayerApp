using Android.Widget;
using OnlineMusicPlayerApp.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(PhoneService))]
public class PhoneService : IPhoneService
{
    public void Call(string number)
    {
        PhoneDialer.Open(number);
    }
}