using OnlineMusicPlayerApp.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.BLL
{
    public class SettingsRepository
    {
        readonly SQLiteAsyncConnection _database;

        public SettingsRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<SettingsItem>().Wait();
            if (_database.Table<SettingsItem>().CountAsync().Result < 1)
            {
                var settingsList = new List<SettingsItem>();
                string[] strSettingKeys = new string[]{
                    "settings.lottery.peoples.Notification.Text",
                    "settings.lottery.peoples.SendSMSToLotterySelected",
                    "settings.lottery.instagram.followers.IsReloadInstaFollowers",
                    "settings.lottery.sendsms",
                    "settings.lottery.question.sendsms",
                    "settings.lottery.question.openWinnerPage"
                };
                string[] strSettingValues = new string[]{
                    "{0} عزیز،نام شما برای قرعه کشی {1} انتخاب شده است",
                    "true",
                    "true",
                    "false",
                    "true",
                    "false"
                };
                var index = strSettingValues.Count();
                for (int i = 0; i < index; i++)
                {
                    var key = strSettingKeys[i];
                    var val = strSettingValues[i];
                    var settingItem = new SettingsItem()
                    {
                        Key = key,
                        Value = val
                    };
                    settingItem.EntityName = settingItem.GetType().Name;
                    settingItem.EntityId = 0;
                    settingsList.Add(settingItem);
                }
                _database.InsertAllAsync(settingsList).Wait();
            }
        }

        public async Task<List<SettingsItem>> GetAsync()
        {
            return await _database.Table<SettingsItem>().ToListAsync();
        }

        public async Task<SettingsItem> GetSettingAsync(Expression<Func<SettingsItem, bool>> predicate)
        {
            return await _database.Table<SettingsItem>()
                            .Where(predicate)
                            .FirstOrDefaultAsync();
        }

        public async Task<SettingsItem> FindAsync(Expression<Func<SettingsItem, bool>> predicate)
        {
            return await _database.Table<SettingsItem>()
                            .Where(predicate)
                            .FirstOrDefaultAsync();
        }

        public async Task<SettingsItem> FindByKeyAsync(string key)
        {
            return await _database.Table<SettingsItem>()
                            .Where(x => x.Key == key)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveNoteAsync(SettingsItem note)
        {
            if (note.Id != 0)
            {
                return await _database.UpdateAsync(note);
            }
            else
            {
                return await _database.InsertAsync(note);
            }
        }

        public async Task<int> CreateAsync(SettingsItem setting)
        {
            //setting.Key = setting.Key.ToLower();
            //setting.Value = setting.Value.ToLower();
            return await _database.InsertAsync(setting);
        }
        public async Task<int> CreateAsync(IList<SettingsItem> setting)
        {
            return await _database.InsertAllAsync(setting);
        }


        internal async Task<bool> Any()
        {
            return await _database.Table<SettingsItem>().CountAsync() > 0 ? true : false;
        }

        public async Task<int> DeleteNoteAsync(SettingsItem note)
        {
            return await _database.DeleteAsync(note);
        }

        public async Task<int> Update(SettingsItem note)
        {
            return await _database.UpdateAsync(note);
        }

        public async Task<int> IsGroupName()
        {
            return await _database.Table<SettingsItem>().CountAsync();
        }
    }
}
