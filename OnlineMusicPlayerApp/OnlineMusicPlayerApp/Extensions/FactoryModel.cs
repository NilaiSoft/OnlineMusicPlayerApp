using Android.Webkit;
using OnlineMusicPlayerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.Extensions
{
    public static class FactoryModel
    {
        public static List<Detail> DetailFactory(this IList<Detail> details)
        {
            return details
                .OrderByDescending(item =>
                {
                    //string audioFileName = $"{item.ParentId}_{item.Id}{Path.GetExtension(item.Href)}";
                    string audioFileName = $"{item.Id}{Path.GetExtension(item.Href)}";
                    string audioPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), audioFileName);
                    return File.Exists(audioPath);
                })
                .ThenByDescending(item => item.Id)
                .ToList();
        }
    }
}
