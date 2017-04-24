using Akavache;
using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AmmySEA.StackExchangeApi;
using System.Reactive.Threading.Tasks;
using System.Diagnostics;

namespace AmmySEA.ViewModels
{
    [ImplementPropertyChanged]
    public class OptionsViewModel
    {
        public IReactiveList<StackExchangeSite> Sites { get; set; }
        public IReactiveList<StackExchangeSite> SelectedSites { get; set; }
        public StackExchangeSite SelectedSite { get; set; }
        
        public Exception LatestException { get; set; }

        public int UpdateInterval { get; set; }
        public int QuestionsPerBlock { get; set; }

        private const string _intervalCacheKey = "_interval";
        private const string _questionCountCacheKey = "_question_count";
        private const string _selectedSitesCacheKey = "_selected_sites";
        private const string _siteListCacheKey = "_site_list";
        private readonly Api _api;

        public OptionsViewModel(Api api)
        {
            _api = api;

            UpdateInterval = 5;
            QuestionsPerBlock = 3;

            Sites = new ReactiveList<StackExchangeSite>();
            Sites.ChangeTrackingEnabled = true;
            Sites.ItemChanged
                 .Where(args => args.PropertyName == "IsSelected")
                 .Subscribe(args => ToggleSelected(args.Sender));

            SelectedSites = new ReactiveList<StackExchangeSite>();
            
            var loadSiteList = BlobCache.UserAccount.GetObject<SiteList>(_siteListCacheKey)
                                        .Catch((Exception _) => ReloadSiteList())
                                        .Do(siteList => Debug.WriteLine(siteList));            

            var loadSelectedSites = BlobCache.UserAccount.GetObject<HashSet<string>>(_selectedSitesCacheKey)
                                             .Catch(Observable.Return(new HashSet<string>()));
            loadSelectedSites.CombineLatest(loadSiteList, (selectedSites, siteList) => new { selectedSites, siteList })
                             .ObserveOn(RxApp.MainThreadScheduler)
                             .Subscribe(a => {
                                 UpdateSites(a.siteList);

                                 using (Sites.SuppressChangeNotifications()) {
                                     foreach (var site in a.siteList.items) {
                                         if (a.selectedSites.Contains(site.api_site_parameter)) {
                                             site.IsSelected = true;
                                             SelectedSites.Add(site);
                                         }
                                     }
                                 }
                             });

            this.WhenAnyValue(v => v.UpdateInterval)
                .Skip(1)
                .Subscribe(interval => BlobCache.UserAccount.InsertObject(_intervalCacheKey, interval));

            BlobCache.UserAccount.GetObject<int>(_intervalCacheKey)
                                 .Catch(Observable.Return(UpdateInterval))
                                 .Subscribe(interval => UpdateInterval = interval);

            this.WhenAnyValue(v => v.QuestionsPerBlock)
                .Skip(1)
                .Subscribe(questions => BlobCache.UserAccount.InsertObject(_questionCountCacheKey, questions));

            BlobCache.UserAccount.GetObject<int>(_questionCountCacheKey)
                                 .Catch(Observable.Return(QuestionsPerBlock))
                                 .Subscribe(questionCount => QuestionsPerBlock = questionCount);
        }

        private IObservable<SiteList> ReloadSiteList()
        {
            return _api.GetSites()
                       .ToObservable()
                       .Do(UpdateSiteListCache);
        }

        private IObservable<Unit> ToggleSelected(StackExchangeSite site)
        {
            if (site.IsSelected)
                SelectedSites.Add(site);
            else
                SelectedSites.Remove(site);

            var hashSet = new HashSet<string>(SelectedSites.Select(s => s.api_site_parameter));
            return BlobCache.UserAccount.InsertObject(_selectedSitesCacheKey, hashSet);
        }

        private void UpdateSites(SiteList siteList)
        {
            foreach (var site in siteList.items)
                site.name = WebUtility.HtmlDecode(site.name);
            
            // Add sites one by one with 50ms intervals
            siteList.items.ToObservable()
                    .Zip(Observable.Interval(TimeSpan.FromMilliseconds(50)), (item, _) => item)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(site => Sites.Add(site));
        }

        private void UpdateSiteListCache(SiteList siteList)
        {
            BlobCache.UserAccount.InsertObject(_siteListCacheKey, siteList, TimeSpan.FromDays(1));
        }
    }
}