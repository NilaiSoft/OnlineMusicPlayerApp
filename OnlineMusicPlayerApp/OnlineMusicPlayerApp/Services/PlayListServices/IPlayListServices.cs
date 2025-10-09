using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.Services.PlayListServices
{
    public interface IPlayListServices
    {
        public Task<List<Category>> GetCategoriesFromJson();
    }
}
