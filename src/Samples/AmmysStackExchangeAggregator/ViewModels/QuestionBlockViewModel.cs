using System;
using PropertyChanged;
using ReactiveUI;
using AmmySEA.StackExchangeApi;
using System.Reactive;
using System.Net;

namespace AmmySEA.ViewModels
{
    [ImplementPropertyChanged]
    class QuestionBlockViewModel
    {
        public ReactiveList<Question> Questions { get; private set; }
        public ReactiveCommand<int, QuestionList> ReloadQuestions { get; private set; }
        public StackExchangeSite Site { get; set; }
        public Exception LatestException { get; set; }

        public QuestionBlockViewModel(Api api, StackExchangeSite site)
        {
            Questions = new ReactiveList<Question>();
            Site = site;

            ReloadQuestions = ReactiveCommand.CreateFromTask<int, QuestionList>(questionCount => api.GetQuestions(site.api_site_parameter, questionCount));
            ReloadQuestions.ThrownExceptions.BindTo(this, vm => vm.LatestException);
            ReloadQuestions.Subscribe(UpdateQuestions);
        }

        private void UpdateQuestions(QuestionList questionList)
        {
            foreach (var question in questionList.items)
                question.title = WebUtility.HtmlDecode(question.title);

            using (Questions.SuppressChangeNotifications()) {
                Questions.Clear();
                Questions.AddRange(questionList.items);
            }
        }
    }
}
