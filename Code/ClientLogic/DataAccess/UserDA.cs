using ClientLogic.Model;
using ClientLogic.WinRT;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ClientLogic.DataAccess
{
    public class UserDA : BaseDA
    {
        public ObservableCollection<User> Contacts { get { return _contacts; } }
        private readonly ObservableCollection<User> _contacts = new ObservableCollection<User>();
        public User Me { get; private set; }

        public IEnumerable<User> GetContactsAndMe()
        {
            return Contacts.Union(new List<User>() { Me });
        }

        public UserDA()
        {
            Clear();
        }

        internal void Clear()
        {
            Me = null;
            Contacts.Clear();
        }

        public async Task<LiveConnectClient> ReadUser()
        {
            Clear();
            LiveConnectClient client = null;

            var loginConnector = new LoginConnector();
            var session = await loginConnector.GetLiveConnectSession();
            if (null != session)
            {
                try
                {
                    if (null != session)
                    {
                        client = new LiveConnectClient(session);
                    }
                    else { client = null; }

                    if (null != client)
                    {
                        var meOperationResult = await client.GetAsync("me");
                        User me = null;

                        dynamic meResult = meOperationResult.Result;
                        if (meResult.id != null)
                        {
                            me = new User();
                            me.LiveId = meResult.id;
                            me.FirstName = meResult.first_name;
                            me.LastName = meResult.last_name;
                            me.Name = meResult.name;
                            me.ImageURI = meResult.link;
                        }

                        this.Me = me;
#if DEBUG
                        // For debuging add me to the contact list
                        //me.ImageURI = "http://images.all-free-download.com/images/graphiclarge/fall_book_follow_me_100101.jpg";
                        //this.Contacts.Add(me);
#endif

                    }
                }
                catch (Exception ex)
                {
                    client = null;
                    this.Me = null;
                    Debug.WriteLine("Error: " + ex);
                }
            }
            return client;
        }

        public async Task<bool> ReadContacts(LiveConnectClient client, List<long> unknownUserIDs)
        {
            bool ok = false;

            try
            {
                if (null != client)
                {
                    var contactsOperation = await client.GetAsync("me/contacts");
                    ObservableCollection<User> contactList = null;

                    dynamic contacts = contactsOperation.Result;
                    if (contacts.data != null)
                    {
                        contactList = new ObservableCollection<User>();
                        foreach (var contact in contacts.data)
                        {
                            var c = new User();
                            c.LiveId = contact.user_id;
                            c.FirstName = contact.first_name;
                            c.LastName = contact.last_name;
                            c.Name = contact.name;

                            contactList.Add(c);

                            await ReadUserPicture(client, c);
                        }
                    }

                    MergeUsers(this.Contacts, contactList, true);

                    unknownUserIDs = unknownUserIDs.Except(Contacts.Select(u => u.ID)).ToList();
                    foreach (var uId in unknownUserIDs)
                    {
                        var unknownUsers = await GetTable<Channel>(new Dictionary<string, object>() { { "fId", uId } }).ToListAsync();
                        MergeUsers(this.Contacts, unknownUsers.Select(c => new User()
                        {
                            ID = c.ID,
                            LiveId = c.LiveId,
                        }).ToList(), false);
                    }

                    ok = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex);
            }
            return ok;
        }

        private async Task ReadUserPicture(LiveConnectClient client, User user)
        {
            if (user.ImageURI == null)
            {
                var picture = await client.GetAsync(user.LiveId + "/picture");
                user.ImageURI = picture.Result["location"].ToString();
            }
        }

        public void SaveState()
        {
            SaveToStorage("uCNr", Contacts.Count);
            for (int i = 0; i < Contacts.Count; i++)
            {
                SaveToStorage("uCID" + i, Contacts[i].ID);
                SaveToStorage("uCLId" + i, Contacts[i].LiveId);
            }
            Debug.WriteLine("SaveState " + Contacts.Count + " contacts");
        }

        private bool _loadStateDone = false;
        public void LoadState()
        {
            if (!_loadStateDone)
            {
                var contacts = new List<User>();
                int n = LoadFromStorage<int>("uCNr");
                for (int i = 0; i < n; i++)
                {
                    var u = new User();
                    u.ID = LoadFromStorage<long>("uCID" + i);
                    u.LiveId = LoadFromStorage<string>("uCLId" + i);
                    contacts.Add(u);
                }
                MergeUsers(Contacts, contacts, false);
                _loadStateDone = true;

                Debug.WriteLine("LoadState " + (contacts == null ? 0 : contacts.Count) + " contacts");
            }
        }

        public static void MergeUsers(ObservableCollection<User> targetList, IEnumerable<User> srcList, bool updateDetails)
        {
            if (null == srcList) { return; }
            foreach (var src in srcList)
            {
                var target = targetList.FirstOrDefault(t => t.LiveId == src.LiveId);
                if (target != null)
                {
                    if (target.ID == 0 && src.ID != 0) { target.ID = src.ID; }
                    if (updateDetails)
                    {
                        target.FirstName = src.FirstName;
                        target.LastName = src.LastName;
                        target.Name = src.Name;
                    }
                }
                else
                {
                    targetList.Add(src);
                }
            }
        }
    }
}
