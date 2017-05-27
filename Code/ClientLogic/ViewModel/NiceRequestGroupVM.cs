using ClientLogic.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLogic.WinRT.Extensions;

namespace ClientLogic.ViewModel
{
    public class NiceRequestGroupVM : BaseViewModel
    {
        internal Func<NiceRequest, bool> filter;

        internal void UpdateRequests()
        {
            var allRequests = BusinessLogic.Instance.Notifications.GetAllNiceRequests();
            var filteredRequests = allRequests.Where(filter).OrderBy(r => r.CreatedAt);

            var newRequests = filteredRequests.Where(r => !NiceRequests.Any(rVM => rVM.niceRequest.ID == r.ID));
            foreach (var r in newRequests)
            {
                var rVM = new NiceRequestVM(this, r, true);
                NiceRequests.Insert(0, rVM);
            }
            var deletedRequests = NiceRequests.Where(rVM => !filteredRequests.Any(r => r.ID == rVM.niceRequest.ID)).ToList();
            foreach (var rVM in deletedRequests)
            {
                NiceRequests.Remove(rVM);
            }
            foreach (var rVM in NiceRequests)
            {
                rVM.OnEntityChanged();
                rVM.UpdateComments();
            }
        }

        public bool IsMine { get; set; }
        public bool IsNotMine
        {
            get
            {
                return !IsMine;
            }
        }

        public NiceRequestGroupVM()
        {
            NiceRequests.CollectionChanged += NiceRequests_CollectionChanged;
        }

        private void NiceRequests_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var items = NiceRequests.Take(Settings.MaxLatestItemsCount);
            bool equal = (items.Count() == LatestNiceRequests.Count);
            if (equal)
            {
                int i = 0;
                foreach (var it in items)
                {
                    if (LatestNiceRequests[i] != it) { equal = false; }
                }
            }
            // Only repopluate the colection if something changed, to avoid any flickering
            if (!equal)
            {
                LatestNiceRequests.Clear();
                LatestNiceRequests.AddRange(NiceRequests.Take(Settings.MaxLatestItemsCount));
            }
        }

        public string UniqueId { get; internal set; }

        public string Title { get; internal set; }

        /// <summary>
        /// A list with all the requests
        /// </summary>
        public ObservableCollection<NiceRequestVM> NiceRequests { get { return _niceRequests; } }
        private readonly ObservableCollection<NiceRequestVM> _niceRequests = new ObservableCollection<NiceRequestVM>();

        /// <summary>
        /// Show only the latest items
        /// </summary>
        public ObservableCollection<NiceRequestVM> LatestNiceRequests
        {
            get { return _latestNiceRequests; }
        }
        private readonly ObservableCollection<NiceRequestVM> _latestNiceRequests = new ObservableCollection<NiceRequestVM>();

    }
}
