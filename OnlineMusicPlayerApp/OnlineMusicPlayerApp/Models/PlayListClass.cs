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
        public string Id { get; set; }
        public string Href { get; set; }
    }

    public class CategoryWrapper
    {
        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }
    }
}
