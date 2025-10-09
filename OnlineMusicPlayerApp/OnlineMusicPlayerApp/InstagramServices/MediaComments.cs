using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using NilaiSoft.Api.Samples;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.InstagramServices
{
    internal class MediaComments : IDemoSample
    {
        private readonly IInstaApi _instaApi;

        public MediaComments(IInstaApi instaApi)
        {
            _instaApi = instaApi;
        }

        public async Task DoShow()
        {
            var stream = _instaApi.GetStateDataAsStream();
            var anotherInstance = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.Empty)
                .SetRequestDelay(RequestDelay.FromSeconds(2, 2))
                .Build();
            anotherInstance.LoadStateDataFromStream(stream);
            var anotherResult = await anotherInstance.GetCurrentUserAsync();
            if (!anotherResult.Succeeded)
            {
                Console.WriteLine($"Unable to get current user using current API instance: {anotherResult.Info}");
                return;
            }

            string followers = "";
            var result1 = await _instaApi.GetUserFollowersAsync(anotherResult.Value.UserName, PaginationParameters.MaxPagesToLoad(1));
            foreach (var item in result1.Value)
            {
                followers = followers + $"{item.FullName} {item.UserName} {(item.IsPrivate ? "Private" : "Public")}" + '\n';
            }

            //MessageBox.Show(followers);
        }
        public async Task<IResult<InstaCommentList>> GetMediaCommentInfoFromUrlAsync(string mediaUrl)
        {
            var stream = _instaApi.GetStateDataAsStream();
            var anotherInstance = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.Empty)
                .SetRequestDelay(RequestDelay.FromSeconds(2, 2))
                .Build();
            anotherInstance.LoadStateDataFromStream(stream);
            var anotherResult = await anotherInstance.GetCurrentUserAsync();
            if (!anotherResult.Succeeded)
            {
                Console.WriteLine($"Unable to get current user using current API instance: {anotherResult.Info}");
                return null;
            }
            var mediaId = await _instaApi.GetMediaIdFromUrlAsync(new System.Uri(mediaUrl));
            return await _instaApi.GetMediaCommentsAsync(mediaId.Value, PaginationParameters.MaxPagesToLoad(1));
        }

        public async Task<IResult<InstaMedia>> GetMediaFromUrlAsync(string mediaUrl)
        {
            var stream = _instaApi.GetStateDataAsStream();
            var anotherInstance = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.Empty)
                .SetRequestDelay(RequestDelay.FromSeconds(2, 2))
                .Build();
            anotherInstance.LoadStateDataFromStream(stream);
            var anotherResult = await anotherInstance.GetCurrentUserAsync();
            if (!anotherResult.Succeeded)
            {
                Console.WriteLine($"Unable to get current user using current API instance: {anotherResult.Info}");
                return null;
            }
            var mediaId = await _instaApi.GetMediaIdFromUrlAsync(new System.Uri(mediaUrl));
            var media = await _instaApi.GetMediaByIdAsync(mediaId.Value);
            return media;
        }      
    }
}
