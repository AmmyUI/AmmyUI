using System;
using PropertyChanged;
using ReactiveUI;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using AmmySEA.StackExchangeApi;

namespace AmmySEA.ViewModels
{
    [ImplementPropertyChanged]
    class QuestionBlockListViewModel
    {
        private Api _api;

        public IReactiveList<QuestionBlockViewModel> QuestionBlocks { get; set; }

        public QuestionBlockListViewModel(Api api, OptionsViewModel options)
        {
            _api = api;
            QuestionBlocks = new ReactiveList<QuestionBlockViewModel>();

            var selectedSiteList = options.SelectedSites;

            selectedSiteList.Changed
                            .Where(args => args.Action == NotifyCollectionChangedAction.Add)
                            .SelectMany(args => args.NewItems
                                                    .OfType<StackExchangeSite>()
                                                    .Select(site => CreateQuestionBlock(site, options.QuestionsPerBlock)))
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(block => QuestionBlocks.Add(block));

            selectedSiteList.Changed
                            .Where(args => args.Action == NotifyCollectionChangedAction.Remove)
                            .SelectMany(args => args.OldItems
                                                    .OfType<StackExchangeSite>()
                                                    .Select(site => QuestionBlocks.FirstOrDefault(qb => qb.Site.api_site_parameter == site.api_site_parameter)))
                            .Where(qb => qb != null)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(qb => QuestionBlocks.Remove(qb));

            options.WhenAnyValue(o => o.UpdateInterval)
                   .CombineLatest(options.WhenAnyValue(o => o.QuestionsPerBlock), (interval, questionCount) => new { interval, questionCount })
                   .Select(a => Observable.Interval(TimeSpan.FromMinutes(a.interval))
                                          .Select(_ => a.questionCount)
                                          .StartWith(a.questionCount))
                   .Switch()
                   .SelectMany(questionCount => ReloadAllQuestions(questionCount))
                   .Subscribe();
        }

        private QuestionBlockViewModel CreateQuestionBlock(StackExchangeSite site, int questionCount)
        {
            var block = new QuestionBlockViewModel(_api, site);
            block.ReloadQuestions.Execute(questionCount).Subscribe();
            return block;
        }

        private IObservable<QuestionList> ReloadAllQuestions(int questionCount)
        {
            return QuestionBlocks.ToObservable()
                                 .SelectMany(block => block.ReloadQuestions.Execute(questionCount));
        }
    }
}
