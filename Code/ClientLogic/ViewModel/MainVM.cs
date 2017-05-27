using ClientLogic.Model;
using ClientLogic.ViewModel.Events;
using ClientLogic.WinRT;
using ClientLogic.WinRT.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Popups;

namespace ClientLogic.ViewModel
{
    public class MainVM : BaseViewModel
    {
        #region Singleton And Construct

        public static MainVM Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new MainVM();
                }
                return _instance;
            }
        }
        private static MainVM _instance = null;

        private MainVM()
        {
            Groups.Add(new NiceRequestGroupVM()
            {
                filter = (x) => { return x.OwnerId == BusinessLogic.Instance.Users.Me.ID && !x.IsClosed; },
                UniqueId = "myRequestsGroup",
                Title = "My Requests",
                IsMine = true
            });
            Groups.Add(new NiceRequestGroupVM()
            {
                filter = (x) => { return x.OwnerId != BusinessLogic.Instance.Users.Me.ID && !x.IsClosed; },
                UniqueId = "friendsRequestsGroup",
                Title = "Friends Requests",
                IsMine = false
            });
        }

        #endregion

        #region Proprierties

        public void UpdateAsync()
        {
            ExecuteInCreatorContext((x) =>
            {
                Update();
            });
        }

        public void Update()
        {
            // The contacts only have a live ID
            foreach (var u in BusinessLogic.Instance.Users.Contacts)
            {
                if (!Friends.Any(f => f.user.LiveId == u.LiveId))
                {
                    Friends.Add(new UserVM(u));
                }
            }
            foreach (var fr in Friends)
            {
                fr.OnEntityChanged();
            }
            foreach (var gr in Groups)
            {
                gr.OnEntityChanged();
                gr.UpdateRequests();
            }
        }

        public ObservableCollection<NiceRequestGroupVM> Groups { get { return _groups; } }
        private readonly ObservableCollection<NiceRequestGroupVM> _groups = new ObservableCollection<NiceRequestGroupVM>();

        public ObservableCollection<UserVM> Friends { get { return _friends; } }
        private readonly ObservableCollection<UserVM> _friends = new ObservableCollection<UserVM>();

        public NiceRequestVM GetNiceRequest(string uniqueId)
        {
            foreach (var gr in Groups)
            {
                var rVM = gr.NiceRequests.FirstOrDefault(r => r.UniqueId == uniqueId);
                if (rVM != null)
                {
                    return rVM;
                }
            }
            return null;
        }

        public NiceRequestVM GetNiceRequestByID(long niceRequestId)
        {
            foreach (var gr in Groups)
            {
                var rVM = gr.NiceRequests.FirstOrDefault(r => r.niceRequest.ID == niceRequestId);
                if (rVM != null)
                {
                    return rVM;
                }
            }
            return null;
        }

        public NiceRequestGroupVM GetGroup(string uniqueId)
        {
            return Groups.FirstOrDefault(gr => gr.UniqueId == uniqueId);
        }

        #endregion

        #region New Nice Request

        public ICommand NewNiceRequestCommand
        {
            get
            {
                if (null == _createNiceRequest)
                {
                    _createNiceRequest = new DelegateCommand(OnCreateNiceRequest);
                }
                return _createNiceRequest;
            }
        }
        private ICommand _createNiceRequest = null;

        private async void OnCreateNiceRequest()
        {
            var businessLogic = BusinessLogic.Instance;

            var camera = new Camera();
            var pictureFile = await camera.TakePicture();
            if (pictureFile != null)
            {
                var niceRequest = new NiceRequest()
                {
                    Currency = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol,
                    PriceValue = 0,
                    Description = ""
                };
                await niceRequest.LoadImage(pictureFile);
                NewNiceRequest = new NiceRequestVM(Groups[0], niceRequest, false);

                if (null != NewNiceRequestCreated)
                {
                    NewNiceRequestCreated(this, new EventArgs());
                }
            }
        }

        public EventHandler NewNiceRequestCreated;

        public ICommand SaveNewNiceRequestCommand
        {
            get
            {
                if (null == _saveNewNiceRequestCommand)
                {
                    _saveNewNiceRequestCommand = new DelegateCommand(SaveNewNiceRequest);
                }
                return _saveNewNiceRequestCommand;
            }
        }
        private ICommand _saveNewNiceRequestCommand = null;

        private async void SaveNewNiceRequest()
        {
            if (null != NewNiceRequest)
            {
                var friends = SelectedFriends.Select(u => u.user).ToList();
                if (friends.Count > 0)
                {
                    var niceRequest = NewNiceRequest.niceRequest;
                    bool ok = await BusinessLogic.Instance.Notifications.SendRequestToUsers(niceRequest, friends);
                    await niceRequest.SaveImage();
                    if (ok)
                    {
                        friends.Clear();
                        Update();
                        if (null != NewNiceRequestSaved)
                        {
                            NewNiceRequestSaved(this, new EventArgs());
                        }
                    }
                    else
                    {
                        OnErrorOccured("NewNiceRequest");
                    }
                }
            }
        }
        public EventHandler NewNiceRequestSaved;

        public NiceRequestVM NewNiceRequest
        {
            get { return _newNiceRequest; }
            internal set
            {
                _newNiceRequest = value;
                OnPropertyChanged();
            }
        }
        private NiceRequestVM _newNiceRequest = null;

        public List<UserVM> SelectedFriends
        {
            get { return _selectedFriends; }
        }
        private readonly List<UserVM> _selectedFriends = new List<UserVM>();

        #endregion

        #region Help Button

        public ICommand HelpCommand
        {
            get
            {
                if (null == _helpCommand)
                {
                    _helpCommand = new DelegateCommand<object>((x) =>
                    {
                        string msg = @"The help menu is in progress!
But there are two common issues:
1. The app might be starting slowly and show an empty screen for a long time because the server is in the US instead of EU (Will be changed)
2. The app might not work if the 160MB trafic limit for the trial Azure account is reached.
";
                        var dialog = new MessageDialog(msg, "Help");
                        dialog.Commands.Add(new UICommand
                        {
                            Label = "Ok",
                        });
                        var task = dialog.ShowAsync();
                    });
                }
                return _helpCommand;
            }
        }
        private ICommand _helpCommand = null;

        #endregion

        #region Error Handling

        private void OnErrorOccured(string errorType)
        {
            if (null != ErrorOccured)
            {
                ErrorOccured(this, new ErrorEventArgs(errorType));
            }
        }
        public EventHandler<ErrorEventArgs> ErrorOccured;

        #endregion
    }
}
