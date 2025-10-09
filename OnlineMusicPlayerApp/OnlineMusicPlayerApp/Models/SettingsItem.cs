using OnlineMusicPlayerApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Model
{
    public class SettingsItem : BaseEntity
    {
        //public string GroupName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
    }
}
