using Newtonsoft.Json;
using OnlineMusicPlayerApp.Models;
using OnlineMusicPlayerApp.Services.PlayListServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;


[assembly: Dependency(typeof(PlayListServices))]
public class PlayListServices : IPlayListServices
{
    public async Task<List<Category>> GetCategoriesFromJson()
    {
        var json = await DependencyService.Get<IGoogleDriveServices>().GetMusicPlayList();
        var wrapper = JsonConvert.DeserializeObject<CategoryWrapper>(json);
        return wrapper?.Categories ?? new List<Category>();
    }
}
