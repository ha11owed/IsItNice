using ClientLogic.WinRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ClientLogic.Model
{
    /// <summary>
    /// A request for a review.
    /// </summary>
    public class NiceRequest : BaseEntity
    {
        public double PriceValue { get; set; }
        public string Currency { get; set; }

        public string Description { get; set; }
        public bool IsClosed { get; set; }

        public string Image { get; set; }

        public long OwnerId { get; set; }

        public DateTime CreatedAt { get; set; }

        #region Local attributes
        
        [IgnoreDataMember]
        public string Price { get { return PriceValue + " " + Currency; } }

        [IgnoreDataMember]
        public User User { get; set; }
        
        [IgnoreDataMember]
        public List<Comment> FriendComments { get { return _friendComments; } }
        private readonly List<Comment> _friendComments = new List<Comment>();

        [IgnoreDataMember]
        public string ImageURI
        {
            get
            {
                string imageUri = Settings.MissingNiceRequestImageURI;
                if (_fileContainer != null && !string.IsNullOrEmpty(_fileContainer.URI))
                {
                    imageUri = _fileContainer.URI;
                }
                return imageUri;
            }
        }
        
        public async Task LoadImage(FileContainer file)
        {
            _fileContainer = file;
            if (null != _fileContainer && string.IsNullOrEmpty(Image))
            {
                Image = await file.GetBase64String();
            }
        }

        public async Task LoadImage()
        {
            if (null == _fileContainer)
            {
                var fileName = GetImageName();
                _fileContainer = await FileContainer.Load(fileName);
                if (null == _fileContainer && !string.IsNullOrEmpty(Image))
                {
                    _fileContainer = await FileContainer.Save(Image, fileName);
                }
            }
        }

        public async Task SaveImage()
        {
            if (null == _fileContainer && !string.IsNullOrEmpty(Image))
            {
                var fileName = GetImageName();
                _fileContainer = await FileContainer.Save(Image, fileName);
            }
            else if (null != _fileContainer)
            {
                var fileName = GetImageName();
                await _fileContainer.Rename(fileName);
            }
        }

        private FileContainer _fileContainer = null;
        private string GetImageName()
        {
            return string.Format("niceR{0:00000000}.png", ID);
        }
        
        #endregion
    }
}
