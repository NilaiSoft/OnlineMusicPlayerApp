using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Models
{
    public enum MenuItemType
    {
        ActivityMain = 1,
        About=2,
        People=3,
        PleyList=4,
        frmSettings=5,
        LottoryGroupList = 6,
        LotteryGroups = 7,
        LogouFromInstagram = 8,
        LogInToInstagram = 9
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
        public string Icon { get; set; }
    }
}
