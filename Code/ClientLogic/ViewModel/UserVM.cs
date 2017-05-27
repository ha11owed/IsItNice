using ClientLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace ClientLogic.ViewModel
{
    public class UserVM : BaseViewModel
    {
        internal User user;

        public UserVM(User user)
        {
            this.user = user;
        }

        internal override void OnEntityChanged()
        {
            _image = null;
            base.OnEntityChanged();
        }

        public string Name
        {
            get { return user.Name; }
            set { SetEntityProperty(user, value); }
        }

        public BitmapImage Image
        {
            get
            {
                if (null == _image)
                {
                    _image = new BitmapImage(new Uri(user.ImageURI));
                }
                return _image;
            }
        }
        private BitmapImage _image = null;
    }
}
