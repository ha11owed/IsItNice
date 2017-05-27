using ClientLogic.Model;
using ClientLogic.WinRT.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace ClientLogic.ViewModel
{
    public class CommentVM : BaseViewModel
    {
        internal NiceRequestVM niceRequestVM;
        internal readonly Comment comment;

        public CommentVM(NiceRequestVM niceRequestVM, Comment comment, bool isSaved)
        {
            this.niceRequestVM = niceRequestVM;
            this.comment = comment;
            this._isSaved = isSaved;
        }

        internal override void OnEntityChanged()
        {
            // Reset the image to null. This will force the image to be generated again.
            _userImage = null;
            base.OnEntityChanged();
        }

        public string UniqueId
        {
            get { return "comment" + comment.ID; }
        }

        public string Message
        {
            get { return comment.Message; }
            set { SetEntityProperty(comment, value); }
        }

        public bool IsThumbsUp
        {
            get { return comment.Score == 1; }
            set
            {
                if (comment.Score != 1)
                {
                    comment.Score = 1;
                    OnPropertyChanged("IsThumbsUp");
                    OnPropertyChanged("IsThumbsDown");
                }
            }
        }

        public bool IsThumbsDown
        {
            get { return comment.Score == -1; }
            set
            {
                if (comment.Score != -1)
                {
                    comment.Score = -1;
                    OnPropertyChanged("IsThumbsUp");
                    OnPropertyChanged("IsThumbsDown");
                }
            }
        }

        public String FromName
        {
            get
            {
                if (_fromName == null)
                {
                    var user = BusinessLogic.Instance.Users.Contacts.FirstOrDefault(u => u.ID == comment.FromId);
                    if (user != null)
                    {
                        _fromName = user.Name;
                    }
                }
                return _fromName;
            }
        }
        private String _fromName;

        public BitmapImage UserImage
        {
            get
            {
                if (null == _userImage)
                {
                    string imageUri = Settings.MissingUserImageURI;
                    var user = BusinessLogic.Instance.Users.Contacts.FirstOrDefault(u => u.ID == comment.FromId);
                    if (user != null)
                    {
                        imageUri = user.ImageURI;
                    }
                    _userImage = new BitmapImage(new Uri(imageUri));
                }
                return _userImage;
            }
        }
        private BitmapImage _userImage;

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
        private ICommand _saveCommand = null;
        private bool _isSaved = true;

        private async void OnSaveCommand()
        {
            await BusinessLogic.Instance.Notifications.AddComment(niceRequestVM.niceRequest, comment);
            _isSaved = true;
        }
    }
}
