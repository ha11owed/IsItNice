﻿using ClientLogic;
using ClientLogic.DataAccess;
using ClientLogic.ViewModel;
using IsItNiceGUI.Common;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace IsItNiceGUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://isitnice.azure-mobile.net/",
            "UhrcyRBywvPDXOvWfGhiYkxSvchUod56"
        );

        private async void IsItNiceInitialize()
        {
            string navigateState = null;

            MainVM.Instance.NewNiceRequestCreated += (s, e) =>
            {
                Frame rootFrame = Window.Current.Content as Frame;
                navigateState = rootFrame.GetNavigationState();
                if (!rootFrame.Navigate(typeof(NewNiceRequest), "newNiceRequest"))
                {
                    throw new Exception("Failed to navigate to NewNiceRequest");
                }
            };
            MainVM.Instance.NewNiceRequestSaved += (s, e) =>
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.SetNavigationState(navigateState);
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to go to initial page");
                }
            };

            BusinessLogic.Instance.Notifications.ReceivedCommentShown += (s, e) =>
            {
                MainVM.Instance.UpdateAsync();
            };
            BusinessLogic.Instance.Notifications.ReceivedNiceRequestShown += (s, e) =>
            {
                MainVM.Instance.UpdateAsync();
            };

            BusinessLogic.Instance.Notifications.ReceivedCommentActivated += (s, e) =>
            {
                var item = MainVM.Instance.GetNiceRequestByID(e.RequestId);
                if (null != item)
                {
                    MainVM.Instance.ExecuteInCreatorContext((x) =>
                    {
                        Frame rootFrame = Window.Current.Content as Frame;
                        if (!rootFrame.Navigate(typeof(ItemDetailPage), item.UniqueId))
                        {
                            throw new Exception("Failed to go to recieved notification");
                        }
                    });
                }
            };
            BusinessLogic.Instance.Notifications.ReceivedNiceRequestActivated += (s, e) =>
            {
                var item = MainVM.Instance.GetNiceRequestByID(e.ID);
                if (null != item)
                {
                    MainVM.Instance.ExecuteInCreatorContext((x) =>
                    {
                        Frame rootFrame = Window.Current.Content as Frame;
                        if (!rootFrame.Navigate(typeof(ItemDetailPage), item.UniqueId))
                        {
                            throw new Exception("Failed to go to recieved notification");
                        }
                    });
                }
            };
            
            BusinessLogic.Instance.ConnectionError += (s, e) =>
            {
#if RELEASE
                var dialog = new MessageDialog("No internet connection or the user account is invalid", "Cannot connect to your account");
                dialog.Commands.Add(new UICommand
                {
                    Label = "Ok",
                    Invoked = (cmd) =>
                    {
                        App.Current.Exit();
                    }
                });
                var task = dialog.ShowAsync();
#endif
            };
            await BusinessLogic.Instance.InitApp(MobileService);
        }

        private void IsItNiceSettings()
        {
            Settings.MissingNiceRequestImageURI = "ms-appx:///Assets/NoImage.png";
            Settings.MissingUserImageURI = "ms-appx:///Assets/NoUserImage.png";
        }


        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            IsItNiceSettings();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.IsItNiceInitialize();

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            BusinessLogic.Instance.Notifications.SaveState();
            BusinessLogic.Instance.Users.SaveState();
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
