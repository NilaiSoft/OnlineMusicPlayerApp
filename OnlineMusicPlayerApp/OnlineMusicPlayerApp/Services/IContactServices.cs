using OnlineMusicPlayerApp.Models;
using System.Collections.Generic;

namespace OnlineMusicPlayerApp.Services
{
    public interface IContactServices
    {
        List<ContactModel> GetContacts();
        void ShowContactPiker();
    }
}
