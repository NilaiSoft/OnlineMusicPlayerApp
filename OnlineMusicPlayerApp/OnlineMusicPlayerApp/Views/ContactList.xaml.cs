using OnlineMusicPlayerApp.BLL;
using OnlineMusicPlayerApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OnlineMusicPlayerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContactList : ContentPage
    {
        private readonly PeoplesRepository _PeopleRepository;
        private static int _groupId;
        public ContactList(int groupId)
        {
            InitializeComponent();
            if (_PeopleRepository == null)
            {
                _PeopleRepository = new PeoplesRepository(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dbLottery.db3"));
            }
            _groupId = groupId;
        }

        [Obsolete]
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            // cancellationToken parameter is optional
            var cancellationToken = default(CancellationToken);
            var contacts = await Contacts.GetAllAsync(cancellationToken);

            if (contacts == null)
                return;

            //List<ListItem> listItems = new List<ListItem>();
            var contactList = new List<ContactInfo>();

            foreach (var contact in contacts)
                foreach (var phone in contact.Phones)
                {
                    contactList.Add(new ContactInfo
                    {
                        PhoneNumber = phone.PhoneNumber,
                        DisplayName = contact.DisplayName
                    });
                }

            collectionList.ItemsSource = contactList;
        }

        private class ContactInfo
        {
            public string DisplayName { get; set; }
            public string PhoneNumber { get; set; }
        }
    }
}