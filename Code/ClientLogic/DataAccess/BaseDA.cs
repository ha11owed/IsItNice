using ClientLogic.Model;
using ClientLogic.WinRT.Extensions;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace ClientLogic.DataAccess
{
    /// <summary>
    /// Base class for all data access operations.
    /// Provides access to the mobile services API.
    /// </summary>
    public abstract class BaseDA
    {
        public static MobileServiceClient MobileServiceClient;

        public static void SetMobileServiceClient(MobileServiceClient msc)
        {
            MobileServiceClient = msc;
        }

        /// <summary>
        /// Get a mobile services table. Allows the creation of queries.
        /// </summary>
        /// <typeparam name="T">Table Type</typeparam>
        public static IMobileServiceTable<T> GetTable<T>() where T : BaseEntity
        {
            var table = MobileServiceClient.GetTable<T>();
            return table;
        }

        /// <summary>
        /// Get a mobile services table. Allows the creation of queries.
        /// </summary>
        /// <typeparam name="T">Table Type</typeparam>
        /// <param name="properties">Parameters to be send to the server with the query</param>
        /// <returns></returns>
        public static IMobileServiceTable<T> GetTable<T>(IDictionary<string, object> properties) where T : BaseEntity
        {
            var table = MobileServiceClient.GetTable<T>(properties);
            return table;
        }

        #region Local Data

        /// <summary>
        /// Load an object from the local data.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>The object or default(T)</returns>
        public static T LoadFromStorage<T>(string key)
        {
            if (null == Windows.Storage.ApplicationData.Current || !Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return default(T);
            }
            else
            {
                var data = (T)(Windows.Storage.ApplicationData.Current.LocalSettings.Values[key]);
                return data;
            }
        }

        /// <summary>
        /// Save an object to local storage.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="key">key</param>
        /// <param name="data">value</param>
        /// <returns>true on success</returns>
        public static bool SaveToStorage<T>(string key, T data)
        {
            if (Windows.Storage.ApplicationData.Current == null)
            {
                return false;
            }
            else
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = data;
                return true;
            }
        }

        #endregion
    }
}
