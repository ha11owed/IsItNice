using ClientLogic.Model;
using ClientLogic.WinRT.Extensions;
using ClientLogic.WinRT.Notifications.ToastContent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;

namespace ClientLogic.DataAccess
{
    public partial class NotificationsDA : BaseDA
    {
        #region Proprierties

        public readonly ObservableCollection<NiceRequest> RecievedNotifications = new ObservableCollection<NiceRequest>();
        public readonly ObservableCollection<Comment> RecievedComments = new ObservableCollection<Comment>();

        public PushNotificationChannel CurrentChannel { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// The newly received comment is displayed
        /// </summary>
        public EventHandler<Comment> ReceivedCommentShown;

        /// <summary>
        /// The newly received nice request is displayed
        /// </summary>
        public EventHandler<NiceRequest> ReceivedNiceRequestShown;

        /// <summary>
        /// The user clicks on a new received comment
        /// </summary>
        public EventHandler<Comment> ReceivedCommentActivated;

        /// <summary>
        /// The user clicks on a new received nice request
        /// </summary>
        public EventHandler<NiceRequest> ReceivedNiceRequestActivated;

        #endregion

        private static object globalLock = new object();

        public async Task<bool> AcquirePushChannel()
        {
            if (null != Me && !string.IsNullOrEmpty(Me.LiveId))
            {
                CurrentChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                string uri = CurrentChannel.Uri;


                var channelTable = GetTable<Channel>(new Dictionary<string, object>() { { "LiveId", Me.LiveId } });
                var channel = new Channel { Uri = uri, LiveId = Me.LiveId };
                await channelTable.InsertAsync(channel);

                CurrentChannel.PushNotificationReceived += CurrentChannel_PushNotificationReceived;

                var meDbList = await GetTable<Channel>().ReadAsync();
                if (meDbList.Count() == 1)
                {
                    var meDb = meDbList.First();
                    if (meDb.LiveId == Me.LiveId)
                    {
                        Me.ID = meDb.ID;
                        Me.UserId = meDb.UserId;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CurrentChannel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            Comment comment = null;
            var thisSender = this;

            if (args.NotificationType == PushNotificationType.Raw)
            {
                var json = JsonObject.Parse(args.RawNotification.Content);
                if (json.ContainsKey("notificationNiceRequestId"))
                {
                    #region NiceRequest Toast
                    long notificationNiceRequestId = (long)(json["notificationNiceRequestId"].GetNumber());
                    long ownerId = (long)(json["OwnerId"].GetNumber());
                    CreateNiceRequestToast(notificationNiceRequestId, ownerId);
                    #endregion
                }
                else
                {
                    #region Comment Toast
                    string strDate = json["CreatedAt"].GetString();
                    //string liveId = json["LiveId"].GetString();
                    comment = new Comment()
                    {
                        ID = (long)(json["id"].GetNumber()),
                        Message = json["Message"].ValueType == JsonValueType.String ? json["Message"].GetString() : "",
                        RequestId = (long)(json["RequestId"].GetNumber()),
                        RequestOwnerId = (long)(json["RequestOwnerId"].GetNumber()),
                        FromId = (long)(json["FromId"].GetNumber()),
                        Score = (int)(json["Score"].GetNumber()),
                        CreatedAt = DateTime.Parse(strDate)
                    };


                    User fromUser = null;
                    if (Contacts != null)
                    {
                        fromUser = Contacts.FirstOrDefault(c => c.ID == comment.FromId);
                    }

                    if (null != fromUser)
                    {
                        var toastContent = ToastContentFactory.CreateToastImageAndText04();

                        bool isOk = comment.Score > 0;
                        toastContent.TextHeading.Text = comment.Message;
                        toastContent.TextBody1.Text = isOk ? "Buy!" : "Don't Buy!";
                        toastContent.TextBody2.Text = string.Format("{0}, at {1}", fromUser.Name, comment.CreatedAt.ToString("HH:mm"));
                        toastContent.Image.Src = fromUser.ImageURI;

                        toastContent.Audio.Loop = false;
                        toastContent.Audio.Content = ToastAudioContent.IM;

                        var notification = toastContent.CreateNotification();
                        notification.Activated += (_tn, _arg) =>
                        {
                            if (null != ReceivedCommentActivated)
                            {
                                ReceivedCommentActivated(thisSender, comment);
                            }
                        };
                        ToastNotificationManager.CreateToastNotifier().Show(notification);

                        if (MyBlockedUsers.Any(u => u.BlockedUserId == comment.RequestOwnerId))
                        {
                            args.Cancel = true;
                        }
                        else
                        {
                            lock (globalLock)
                            {
                                RecievedComments.InsertFirstIfDoesNotExist(comment);
                            }
                            if (null != ReceivedCommentShown)
                            {
                                ReceivedCommentShown(thisSender, comment);
                            }
                        }
                    }
                    #endregion
                }
            }

        }

        private async void CreateNiceRequestToast(long niceRequestId, long ownerId)
        {
            if (!MyBlockedUsers.Any(u => u.BlockedUserId == ownerId))
            {
                var thisSender = this;
                var niceRequests = await GetTable<NiceRequest>(theirStuffParam).Where(r => r.ID == niceRequestId).ToListAsync();
                if (niceRequests.Count == 1)
                {
                    var niceRequest = niceRequests[0];

                    await niceRequest.SaveImage();

                    var toastContent = ToastContentFactory.CreateToastImageAndText04();

                    toastContent.TextHeading.Text = niceRequest.Description;
                    toastContent.TextBody1.Text = niceRequest.Price;
                    toastContent.TextBody2.Text = niceRequest.CreatedAt.ToString("HH:mm");
                    toastContent.Image.Src = niceRequest.ImageURI;

                    toastContent.Audio.Loop = false;
                    toastContent.Audio.Content = ToastAudioContent.IM;

                    var notification = toastContent.CreateNotification();
                    notification.Activated += (_tn, _arg) =>
                    {
                        if (null != ReceivedNiceRequestActivated)
                        {
                            ReceivedNiceRequestActivated(thisSender, niceRequest);
                        }
                    };
                    ToastNotificationManager.CreateToastNotifier().Show(notification);

                    lock (globalLock)
                    {
                        RecievedNotifications.InsertFirstIfDoesNotExist(niceRequest);
                    }
                    if (null != ReceivedNiceRequestShown)
                    {
                        ReceivedNiceRequestShown(thisSender, niceRequest);
                    }
                }
            }
        }

    }
}
