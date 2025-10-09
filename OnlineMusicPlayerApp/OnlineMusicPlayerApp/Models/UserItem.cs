using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Models
{
    public class UserItem:BaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
