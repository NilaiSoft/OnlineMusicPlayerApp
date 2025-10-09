using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Models
{
    public class LotteryItem
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
        public string Mobile { get; set; }
        public int PeopleId { get; set; }
        //[MaxLength(200)]
        public int GroupId { get; set; }
        public byte[] ProfilePictureByte { get; set; }
    }
}
