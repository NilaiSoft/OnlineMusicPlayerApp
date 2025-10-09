using OnlineMusicPlayerApp.Model;
using OnlineMusicPlayerApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.BLL
{
    public class UserNameRepository
    {
        readonly SQLiteAsyncConnection _database;

        public UserNameRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<UserItem>().Wait();
        }

        public async Task<List<UserItem>> GetUsersAsync()
        {
            return await _database.Table<UserItem>().ToListAsync();
        }

        public async Task<UserItem> GetUserAsync(int id)
        {
            return await _database.Table<UserItem>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }

        public async Task<UserItem> GetCurrentUserAsync()
        {
            return await _database.Table<UserItem>()
                            .FirstOrDefaultAsync();
        }

        public async Task<int> DeleteAsync(Expression<Func<UserItem, bool>> expression)
        {
            return await _database.Table<UserItem>()
                            .DeleteAsync(expression);
        }

        public async Task<int> SaveUserAsync(UserItem note)
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

        public async Task<int> DeleteNoteAsync(UserItem note)
        {
            return await _database.DeleteAsync(note);
        }

        public async Task<int> IsGroupName()
        {
            return await _database.Table<UserItem>().CountAsync();
        }
    }
}
