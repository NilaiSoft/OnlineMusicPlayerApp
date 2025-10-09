using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(NotifyService))]
namespace OnlineMusicPlayerApp.Services
{
    public class NotifyService : INotifyService
    {
        readonly SQLiteAsyncConnection _database;

        public NotifyService()
        {
            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            _database.CreateTableAsync<Notification>().Wait();
        }
        public async Task ShowNotification(int BadgeNumber = 1,
            string description = "",
            string title = "",
            string returningData = "",
            int notificationId = 1,
            string subtitle = "",
            int notifyTime = 0)
        {
            var notify = new Notification
            {
                Title = title,
                Description = description,
                CreateDate = DateTime.Now
            };

            var res = CreateAsync(notify);

            var notifCount = await TotalCount();

            //var noti = new Plugin.LocalNotification.NotificationRequest
            //{
            //    BadgeNumber = notifCount,
            //    Description = description,
            //    Title = title,
            //    ReturningData = returningData,
            //    NotificationId = notifCount,
            //    Subtitle = subtitle,
            //    Schedule =
            //    {
            //        NotifyTime = DateTime.Now.AddSeconds(notifyTime) // Used for Scheduling local notification, if not specified notification will show immediately.
            //    },
            //};
            //await Plugin.LocalNotification.LocalNotificationCenter.Current.Show(noti);
        }

        public async Task<int> TotalCount() => await _database.Table<Notification>()
                .OrderByDescending(x => x.Id).CountAsync();

        public async Task<int> CreateAsync(Notification notify)
        {
            return await _database.InsertAsync(notify);
        }
    }
}
