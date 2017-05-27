using ClientLogic.Model;
using ClientLogic.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace IsItNiceGUI
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class NewNiceRequest : IsItNiceGUI.Common.LayoutAwarePage
    {
        public NewNiceRequest()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }
            this.DefaultViewModel["Main"] = MainVM.Instance;
            this.DefaultViewModel["NewNiceRequest"] = MainVM.Instance.NewNiceRequest;
            this.DefaultViewModel["Friends"] = MainVM.Instance.Friends;
            /*
            for (int i = 0; i < 20; i++)
            {
                MainVM.Instance.Friends.Add(new UserVM(new User()
                {
                    ImageURI = "http://www.bestfreeicons.com/smimages/girl-beauty-consultant-showing-icon.png",
                    Name = "Edna"+i

                }));
            }
            */

            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = this.flipView.SelectedItem;
            // TODO: Derive a serializable navigation parameter and assign it to pageState["SelectedItem"]
        }

        private void selectedFriendsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deleted = e.RemovedItems.Cast<UserVM>();
            var added = e.AddedItems.Cast<UserVM>();
            foreach (var d in deleted)
            {
                MainVM.Instance.SelectedFriends.Remove(d);
            }
            foreach (var a in added)
            {
                MainVM.Instance.SelectedFriends.Add(a);
            }
            SaveButton.IsEnabled = MainVM.Instance.SelectedFriends.Any();
        }
    }
}
