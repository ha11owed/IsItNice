using ClientLogic.Model;
using ClientLogic.WinRT.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace ClientLogic.ViewModel
{
    public class NiceRequestVM : BaseViewModel
    {
        internal NiceRequest niceRequest;
        internal NiceRequestGroupVM group;

        public NiceRequestVM(NiceRequestGroupVM group, NiceRequest niceRequest, bool isSaved)
        {
            this.group = group;
            this.niceRequest = niceRequest;
            this._isSaved = isSaved;
        }

        internal void UpdateComments()
        {
            var allComments = BusinessLogic.Instance.Notifications.GetAllComments();
            var requestComments = allComments.Where(c => c.RequestId == niceRequest.ID);
            var newComments = requestComments.Where(c => !Comments.Any(cVM => cVM.comment.ID == c.ID));
            foreach (var c in newComments)
            {
                Comments.Add(new CommentVM(this, c, true));
            }
            foreach (var c in Comments)
            {
                c.OnEntityChanged();
            }
        }

        internal override void OnEntityChanged()
        {
            // Delete any image cache. This will force the images to be regenerated.
            _image = null;
            _userImage = null;
            base.OnEntityChanged();
        }

        public string UniqueId
        {
            get { return "niceRequest" + niceRequest.ID; }
        }

        public string Description
        {
            get { return niceRequest.Description; }
            set { SetEntityProperty(niceRequest, value); }
        }

        public double PriceValue
        {
            get { return niceRequest.PriceValue; }
            set
            {
                SetEntityProperty(niceRequest, value);
                OnPropertyChanged("Price");
            }
        }

        public string Price
        {
            get { return niceRequest.Price; }
        }

        public DateTime CreatedAt
        {
            get { return niceRequest.CreatedAt; }
        }

        public BitmapImage Image
        {
            get
            {
                if (null == _image)
                {
                    _image = new BitmapImage(new Uri(niceRequest.ImageURI));
                }
                return _image;
            }
        }
        private BitmapImage _image = null;

        public BitmapImage UserImage
        {
            get
            {
                if (null == _userImage)
                {
                    string imageUri = Settings.MissingUserImageURI;
                    var owner = BusinessLogic.Instance.Users.GetContactsAndMe().FirstOrDefault(u => u.ID == niceRequest.OwnerId);
                    if (null != owner)
                    {
                        imageUri = owner.ImageURI;
                    }
                    _userImage = new BitmapImage(new Uri(imageUri));
                }
                return _userImage;
            }
        }
        private BitmapImage _userImage = null;

        public bool IsClosed
        {
            get { return niceRequest.IsClosed; }
        }

        public bool IsMine
        {
            get { return niceRequest.OwnerId == BusinessLogic.Instance.Users.Me.ID; }
        }

        public bool IsNotMine
        {
            get { return !IsMine; }
        }

        public bool HasMyFeedback
        {
            get { return IsNotMine && Comments.Count > 0; }
        }

        public bool MyThumbUp
        {
            get
            {
                var comment = Comments.FirstOrDefault(c => c.comment.FromId == BusinessLogic.Instance.Users.Me.ID);
                return comment != null && comment.IsThumbsUp;
            }
        }

        public bool MyThumbDown
        {
            get
            {
                var comment = Comments.FirstOrDefault(c => c.comment.FromId == BusinessLogic.Instance.Users.Me.ID);
                return comment != null && comment.IsThumbsDown;
            }
        }

        public int ThumbUpCount
        {
            get { return Comments.Count(c => c.IsThumbsUp); }
        }

        public int ThumbDownCount
        {
            get { return Comments.Count(c => c.IsThumbsDown); }
        }

        public int TotalScore
        {
            get { return ThumbUpCount - ThumbDownCount; }
        }

        #region Save Nice Request

        public ICommand SaveCommand
        {
            get
            {
                if (null == _saveCommand)
                {
                    _saveCommand = new DelegateCommand(OnSaveCommand);
                }
                return _saveCommand;
            }
        }
        public ICommand _saveCommand = null;
        private bool _isSaved = true;

        private async void OnSaveCommand()
        {
            List<User> chosenFriends = null;
            await BusinessLogic.Instance.Notifications.SendRequestToUsers(niceRequest, chosenFriends);
            _isSaved = true;
        }

        #endregion

        #region Save My Comment

        public ICommand SaveFeedbackCommand
        {
            get
            {
                if (null == _saveFeedbackCommand)
                {
                    _saveFeedbackCommand = new DelegateCommand<object>(OnSaveFeedbackCommand, (oScore) =>
                    {
                        int score = int.Parse(oScore.ToString());
                        return score == TotalScore || !HasMyFeedback;
                    });
                }
                return _saveFeedbackCommand;
            }
        }
        private ICommand _saveFeedbackCommand = null;

        private async void OnSaveFeedbackCommand(object oScore)
        {
            if (!HasMyFeedback)
            {
                int score = int.Parse(oScore.ToString());
                var comment = MyFeedback.comment;
                comment.Score = score;
                if (comment.Score != 0)
                {
                    await BusinessLogic.Instance.Notifications.AddComment(niceRequest, comment);
                    UpdateComments();
                    OnEntityChanged();
                    _myFeedback = null;
                }
            }
        }

        public CommentVM MyFeedback
        {
            get
            {
                if (HasMyFeedback)
                {
                    return Comments[0];
                }
                if (null == _myFeedback)
                {
                    _myFeedback = new CommentVM(this, new Comment(), false);
                }
                return _myFeedback;
            }
        }
        private CommentVM _myFeedback = null;

        #endregion

        #region Decided Command

        public ICommand DecidedCommand
        {
            get
            {
                if (null == _decidedCommand)
                {
                    _decidedCommand = new DelegateCommand(OnDecidedCommand, () => { return !niceRequest.IsClosed; });
                }
                return _decidedCommand;
            }
        }
        private ICommand _decidedCommand = null;

        private async void OnDecidedCommand()
        {
            await BusinessLogic.Instance.Notifications.CloseRequest(niceRequest);
            Group.UpdateRequests();
        }

        #endregion

        public NiceRequestGroupVM Group
        {
            get { return group; }
        }

        public ObservableCollection<CommentVM> Comments { get { return _comments; } }
        private readonly ObservableCollection<CommentVM> _comments = new ObservableCollection<CommentVM>();
    }
}
