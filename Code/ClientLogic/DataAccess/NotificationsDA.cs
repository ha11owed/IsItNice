using ClientLogic.Model;
using ClientLogic.WinRT.Extensions;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.DataAccess
{
    public partial class NotificationsDA
    {
        public ObservableCollection<NiceRequest> MyRequests { get { return _myRequests; } }
        private readonly ObservableCollection<NiceRequest> _myRequests = new ObservableCollection<NiceRequest>();

        public ObservableCollection<Comment> MyComments { get { return _myComments; } }
        private readonly ObservableCollection<Comment> _myComments = new ObservableCollection<Comment>();

        public ObservableCollection<BlockedUser> MyBlockedUsers { get { return _myBlockedUsers; } }
        private readonly ObservableCollection<BlockedUser> _myBlockedUsers = new ObservableCollection<BlockedUser>();

        public async Task<bool> CloseRequest(NiceRequest request)
        {
            bool ok = false;
            if (null != Me && null != request && request.OwnerId == Me.ID)
            {
                request.IsClosed = true;
                var niceRequestTable = GetTable<NiceRequest>();
                await niceRequestTable.UpdateAsync(request);
                ok = true;
            }
            return ok;
        }

        public async Task<bool> SendRequestToUsers(NiceRequest request, List<User> users)
        {
            bool ok = false;
            if (null != Me && null != request)
            {
                request.OwnerId = Me.ID;
                request.User = Me;
                var friendsDict = new Dictionary<string, object>();
                for (int i = 0; i < users.Count && i < Constants.MaxReviewers; i++)
                {
                    friendsDict.Add("f" + i, users[i].LiveId);
                }

                var niceRequestTable = GetTable<NiceRequest>(friendsDict);
                await niceRequestTable.InsertAsync(request);
                MyRequests.InsertFirstIfDoesNotExist(request);
                ok = true;
            }
            return ok;
        }

        public async Task<bool> BlockUser(User user)
        {
            bool ok = false;
            if (null != user && null != Me)
            {
                await GetTable<BlockedUser>().InsertAsync(new BlockedUser()
                {
                    OwnerId = Me.ID,
                    BlockedUserId = user.ID
                });
                ok = true;
            }
            return ok;
        }

        public async Task<bool> AddComment(NiceRequest request, Comment comment)
        {
            bool ok = false;
            if (null != Me && null != request)
            {
                comment.FromId = Me.ID;
                comment.RequestId = request.ID;
                comment.RequestOwnerId = request.OwnerId;
                await GetTable<Comment>().InsertAsync(comment);
                MyComments.InsertFirstIfDoesNotExist(comment);
                ok = true;
            }
            return ok;
        }

        internal async Task<bool> ReadFromServerAsync()
        {
            try
            {
                var myStuffParam = new Dictionary<string, object>(this.myStuffParam);
                var theirStuffParam = new Dictionary<string, object>(this.theirStuffParam);
                if (GetAllNiceRequests().Any())
                {
                    long lastRequestID = GetAllNiceRequests().Max(r => r.ID);
                    myStuffParam["lastRequestID"] = lastRequestID;
                    theirStuffParam["lastRequestID"] = lastRequestID;
                }
                if (GetAllComments().Any())
                {
                    long lastCommentID = GetAllComments().Max(r => r.ID);
                    myStuffParam["lastCommentID"] = lastCommentID;
                    theirStuffParam["lastCommentID"] = lastCommentID;
                }
                // Read my stuff
                {
                    var requests = await GetTable<NiceRequest>(myStuffParam).ToListAsync();
                    Merge(MyRequests, requests);
                    var comments = await GetTable<Comment>(myStuffParam).ToListAsync();
                    MyComments.AddRangeIfDoesNotExists(comments);
                }
                // Read their stuff
                {
                    var requests = await GetTable<NiceRequest>(theirStuffParam).ToListAsync();
                    Merge(RecievedNotifications, requests);
                    var comments = await GetTable<Comment>(theirStuffParam).ToListAsync();
                    RecievedComments.AddRangeIfDoesNotExists(comments);
                }
                // Load images
                foreach (var niceRequest in GetAllNiceRequests())
                {
                    await niceRequest.LoadImage();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ReadFromServerAsync failed: " + e);
            }
            return true;
        }

        private void Merge(ObservableCollection<NiceRequest> target, IEnumerable<NiceRequest> sources)
        {
            foreach (var src in sources)
            {
                var match = target.FirstOrDefault(t => t.ID == src.ID);
                if (null != match)
                {
                    match.IsClosed = src.IsClosed;
                }
                else
                {
                    target.Add(src);
                }
            }
        }

        private readonly Dictionary<string, object> myStuffParam = new Dictionary<string, object>() { { "mode", "mystuff" } };
        private readonly Dictionary<string, object> theirStuffParam = new Dictionary<string, object>() { { "mode", "theirstuff" } };

        private UserDA userDA;

        public void SetUserDA(UserDA userDA)
        {
            this.userDA = userDA;
        }

        private User Me
        {
            get
            {
                if (null == this.userDA) { return null; }
                else { return this.userDA.Me; }
            }
        }

        private ObservableCollection<User> Contacts
        {
            get
            {
                if (null == this.userDA) { return null; }
                else { return this.userDA.Contacts; }
            }
        }

        public IEnumerable<Comment> GetAllComments()
        {
            return MyComments.Union(RecievedComments);
        }

        public IEnumerable<NiceRequest> GetAllNiceRequests()
        {
            return MyRequests.Union(RecievedNotifications);
        }

        public async void SaveState()
        {
            SaveToStorage("meID", Me.ID);

            var allNiceRequests = GetAllNiceRequests();
            int i = 0;
            foreach (var niceRequest in allNiceRequests)
            {
                await niceRequest.SaveImage();
                SaveToStorage("niceRequestsID" + i, niceRequest.ID);
                SaveToStorage("niceRequestsO" + i, niceRequest.OwnerId);

                SaveToStorage("niceRequestsCA" + i, niceRequest.CreatedAt.Ticks);
                SaveToStorage("niceRequestsD" + i, niceRequest.Description);

                SaveToStorage("niceRequestsPV" + i, niceRequest.PriceValue);
                SaveToStorage("niceRequestsC" + i, niceRequest.Currency);
                SaveToStorage("niceRequestsIC" + i, niceRequest.IsClosed);
                i++;
            }
            SaveToStorage("nrNiceRequests", i);

            var allComments = GetAllComments();
            i = 0;
            foreach (var comment in allComments)
            {
                SaveToStorage("commentID" + i, comment.ID);
                SaveToStorage("commentFId" + i, comment.FromId);
                SaveToStorage("commentRId" + i, comment.RequestId);
                SaveToStorage("commentROId" + i, comment.RequestOwnerId);

                SaveToStorage("commentCA" + i, comment.CreatedAt.Ticks);
                SaveToStorage("commentM" + i, comment.Message);
                SaveToStorage("commentS" + i, comment.Score);
                i++;
            }
            SaveToStorage("nrComments", i);

            Debug.WriteLine("SaveState saved " + allComments.Count() + " comments and " + allNiceRequests.Count() + " nice requests");
        }

        private bool _loadStateDone = false;
        public async Task<bool> LoadState()
        {
            if (_loadStateDone) { return false; }
            if (Me.ID == 0)
            {
                Me.ID = LoadFromStorage<long>("meID");
            }
            if (Me.ID == 0) { return false; }

            var allNiceRequests = new List<NiceRequest>();
            int n = LoadFromStorage<int>("nrNiceRequests");
            for (int i = 0; i < n; i++)
            {
                var niceRequest = new NiceRequest();
                niceRequest.ID = LoadFromStorage<long>("niceRequestsID" + i);
                niceRequest.OwnerId = LoadFromStorage<long>("niceRequestsO" + i);

                niceRequest.CreatedAt = new DateTime(LoadFromStorage<long>("niceRequestsCA" + i));
                niceRequest.Description = LoadFromStorage<string>("niceRequestsD" + i);

                niceRequest.PriceValue = LoadFromStorage<double>("niceRequestsPV" + i);
                niceRequest.Currency = LoadFromStorage<string>("niceRequestsC" + i);
                niceRequest.IsClosed = LoadFromStorage<bool>("niceRequestsIC" + i);
                await niceRequest.LoadImage();
                allNiceRequests.Add(niceRequest);
            }

            if (allNiceRequests != null)
            {
                MyRequests.AddRangeIfDoesNotExists(allNiceRequests.Where(r => r.OwnerId == Me.ID));
                RecievedNotifications.AddRangeIfDoesNotExists(allNiceRequests.Where(r => r.OwnerId != Me.ID));
            }

            n = LoadFromStorage<int>("nrComments");
            var allComments = new List<Comment>();
            for (int i = 0; i < n; i++)
            {
                var comment = new Comment();
                comment.ID = LoadFromStorage<long>("commentID" + i);
                comment.FromId = LoadFromStorage<long>("commentFId" + i);
                comment.RequestId = LoadFromStorage<long>("commentRId" + i);
                comment.RequestOwnerId = LoadFromStorage<long>("commentROId" + i);

                comment.CreatedAt = new DateTime(LoadFromStorage<long>("commentCA" + i));
                comment.Message = LoadFromStorage<string>("commentM" + i);
                comment.Score = LoadFromStorage<int>("commentS" + i);
                allComments.Add(comment);
            }
            if (allComments != null)
            {
                MyComments.AddRangeIfDoesNotExists(allComments.Where(c => c.FromId == Me.ID));
                RecievedComments.AddRangeIfDoesNotExists(allComments.Where(c => c.FromId != Me.ID));
            }

            _loadStateDone = true;
            Debug.WriteLine("LoadState loaded " + allComments.Count + " comments and " + allNiceRequests.Count + " nice requests");
            return true;
        }
    }
}
