using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Models
{
    public class PostMedias:BaseEntity
    {
        public string LinkMedia { get; set; }
        public int GroupId { get; set; }
        public byte[] MediaFile { get; set; }
    }
}
