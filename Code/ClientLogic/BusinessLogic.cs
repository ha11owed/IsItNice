using ClientLogic.DataAccess;
using ClientLogic.ViewModel;
using ClientLogic.WinRT;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic
{
    public class BusinessLogic
    {
        private bool _isInit = false;

        /// <summary>
        /// Login, Read the data from DB.
        /// 
        /// This method is responsible with the login to MobileServices and WindowsLive.
        /// This method also loads any cached data from local storage.
        /// The local data will then be updated with the data from the server.
        /// </summary>
        public async Task InitApp(MobileServiceClient mobileServiceClient)
        {
            if (!_isInit)
            {
                try
                {
                    NotificationsDA.SetMobileServiceClient(mobileServiceClient);

                    // Login and read my user info
                    var client = await Users.ReadUser();
                    Notifications.SetUserDA(Users);
                    bool ok = (client != null);
                    if (ok)
                    {
                        // Load any saved information (nice requests and comments)
                        bool stateLoaded = await Notifications.LoadState();
                        Users.LoadState();
                        // Aquire push channel, set my user ID
                        ok = await Notifications.AcquirePushChannel();
                        if (ok)
                        {
                            if (!stateLoaded)
                            {
                                // Load any saved information (nice requests and comments)
                                await Notifications.LoadState();
                                //MainVM.Instance.Update();
                            }
                            
                            // Read the nice requests and comments from server.
                            bool okData = await Notifications.ReadFromServerAsync();

                            var unknownFromIds = Notifications.RecievedComments.Select(c => c.FromId);
                            var unknownOwners = Notifications.RecievedNotifications.Select(r => r.OwnerId);
                            var unknownIds = unknownFromIds.Union(unknownOwners).Distinct().ToList();

                            // Read the contact list
                            bool okContacts = await Users.ReadContacts(client, unknownIds);

                            ok = okContacts && okData;
                        }
                    }
                    if (ok)
                    {
                        MainVM.Instance.Update();
                        _isInit = false;
                    }
                    else
                    {
                        Debug.WriteLine("BusinessLogic.InitApp error!");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Init error: " + e);
                    if (null != ConnectionError)
                    {
                        ConnectionError(this, new EventArgs());
                    }
                }
            }
        }
        
        public EventHandler ConnectionError;

        public NotificationsDA Notifications { get { return _notifications; } }
        private NotificationsDA _notifications = new NotificationsDA();

        public UserDA Users { get { return _userDA; } }
        private UserDA _userDA = new UserDA();

        private BusinessLogic()
        {
        }

        public static BusinessLogic Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new BusinessLogic();
                }
                return _instance;
            }
        }

        private static BusinessLogic _instance = null;
    }
}
