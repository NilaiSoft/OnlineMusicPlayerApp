using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NilaiSoft.Api.Samples
{
    internal class Followers : IDemoSample
    {
        private readonly IInstaApi _instaApi;

        public Followers(IInstaApi instaApi)
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
        public async Task<Tuple<IResult<InstaUserShortList>, string>> GetFollowersAsync()
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
            var followersList = await _instaApi.GetUserFollowersAsync(anotherResult.Value.UserName, PaginationParameters.MaxPagesToLoad(1));

            string followers = "";
            foreach (var item in followersList.Value)
            {
                followers = followers + $"{item.FullName} {item.UserName} {(item.IsPrivate ? "Private" : "Public")}" + '\n';
            }

            return new Tuple<IResult<InstaUserShortList>, string>(followersList, followers);
        }

        public async Task<int> FollowerAddUser(List<long> userIDs)
        {
            int isFollowed = 0;
            int isNotFollowed = 0;
            foreach (var userID in userIDs)
            {
                var followMe = await _instaApi.FollowUserAsync(userID);
                if (followMe.Succeeded)
                {
                    isFollowed++;
                }
                if (!followMe.Succeeded)
                {
                    isNotFollowed++;
                }
            }
            return isFollowed;
        }
    }
}
