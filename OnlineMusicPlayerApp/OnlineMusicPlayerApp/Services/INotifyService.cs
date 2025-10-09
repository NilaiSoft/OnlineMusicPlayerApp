using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.Services
{
    public interface INotifyService
    {
        Task ShowNotification(int badgeNumber = 1,
            string description = "",
            string title = "",
            string returningData = "",
            int notificationId = 1,
            string subtitle = "",
            int notifyTime = 0);

        Task<int> TotalCount();
        Task<int> CreateAsync(Notification notify);

    }
}
