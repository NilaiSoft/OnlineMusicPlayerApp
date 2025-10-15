using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Models
{
    public class Category
    {
        public string Master { get; set; }
        public List<Detail> Details { get; set; }
    }

    public class Detail
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public string Href { get; set; }
        public bool IsVisible { get; set; }
        public string TagImageSrc { get; set; }
        public int ParentId { get; set; }
        public List<Detail> Children { get; set; } = new List<Detail>();
    }


    public class CategoryWrapper
    {
        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }
    }
}
