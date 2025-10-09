using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Models
{
    public class LotteryGroups:BaseEntity
    {
        public string  Name { get; set; }
        public int LotteryType { get; set; }
        public bool Enabled { get; set; }
    }

    public enum LotteryType
    {
        Internal = 0,
        Followers = 1,
        Comments = 2
    }
}
