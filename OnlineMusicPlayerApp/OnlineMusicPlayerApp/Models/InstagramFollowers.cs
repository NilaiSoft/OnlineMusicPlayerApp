using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Model
{
    public class InstagramFollowers
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string ProfilePicture { get; set; }
        public byte[] ProfilePictureByte { get; set; }
        public string ProfilePictureId { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsVerified { get; set; }
        public long Pk { get; set; }
        public string CommentText { get; set; }
        public string MediaUrl { get; set; }
        public int GroupId { get; set; }
    }
}
