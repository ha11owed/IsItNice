using ClientLogic.DataAccess;
using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.WinRT
{
    /// <summary>
    /// Wrapper for the WinRT Login API
    /// </summary>
    internal class LoginConnector
    {
        private UserDA UserDa { get { return _userDa; } }
        private UserDA _userDa = new UserDA();

        public async Task<LiveConnectSession> GetLiveConnectSession()
        {
            LiveConnectSession session = null;
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // Two step Auth.
                var mobileServiceClient = BaseDA.MobileServiceClient;

                // First step: we authenticate against live connect, but with a redirect to our azure service.
                var scopes = new List<string>() { "wl.signin", "wl.basic" };
                var authClient = new LiveAuthClient(mobileServiceClient.ApplicationUri.AbsoluteUri);
                LiveLoginResult authResult = await authClient.InitializeAsync(scopes);
                if (authResult.Status != LiveConnectSessionStatus.Connected)
                {
                    authResult = await authClient.LoginAsync(scopes);
                }
                if (authResult.Status == LiveConnectSessionStatus.Connected)
                {
                    session = authClient.Session;
                }

                // Second step: we use the authentication token to authenticate on our service.
                var servicesUser = mobileServiceClient.CurrentUser;
                if (null == servicesUser)
                {
                    servicesUser = await mobileServiceClient.LoginAsync(session.AuthenticationToken);
                }
            }
            return session;
        }

    }
}
