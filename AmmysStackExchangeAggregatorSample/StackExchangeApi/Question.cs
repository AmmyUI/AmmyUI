namespace AmmySEA.StackExchangeApi
{
    public class Question
    {
        public string[] Tags { get; set; }
        public bool is_answered { get; set; }
        public int view_count { get; set; }
        public int answer_count { get; set; }
        public int score { get; set; }
        public string link { get; set; }
        public string title { get; set; }

        public string AnswerCountText
        {
            get {
                return answer_count == 0 || answer_count > 1
                       ? "answers"
                       : "answer";
            }
        }
    }
}