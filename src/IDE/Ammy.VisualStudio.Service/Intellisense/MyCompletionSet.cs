using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using static System.String;

namespace Ammy.VisualStudio.Service.Intellisense
{
    class MyCompletionSet : CompletionSet
    {
        private ICollection<Completion> _items;
        private readonly ICompletionSession _session;

        public MyCompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, ICollection<Completion> completions, IEnumerable<Completion> completionBuilders, ICompletionSession session) : base(moniker, displayName, applicableTo, completions, completionBuilders)
        {
            _items = completions;
            _session = session;
        }

        public void UpdateItems(ICollection<Completion> completions)
        {
            _items = completions;
        }

        public override void Recalculate()
        {}

        public override void SelectBestMatch()
        { }

        public override void Filter()
        {
            var text = ApplicableTo.GetText(ApplicableTo.TextBuffer.CurrentSnapshot);
            text = text.TrimStart('.').Trim(' ');

            //if (IsNullOrWhiteSpace(text)) {
            //    _session.Dismiss();
            //    return;
            //}
            
            var orderedByDistance = _items.Where(c => ContainsAllSymbols(text, c.InsertionText))
                                          .OfType<MyCompletion>()
                                          .OrderBy(c => {
                                              var distance = GetSymbolDistance(c, text);
                                              c.OrderIndex = distance;
                                              return distance;
                                          })
                                          .Distinct(new CompletionEqualityComparer());
            
            var zeroIndex = new List<Completion>();
            var properties = new List<Completion>();
            var nodes = new List<Completion>();
            var other = new List<Completion>();

            foreach (var item in orderedByDistance) {
                if (item.OrderIndex == 0)
                    zeroIndex.Add(item);
                else if (item.CompletionType == CompletionType.Property)
                    properties.Add(item);
                else if (item.CompletionType == CompletionType.Node)
                    nodes.Add(item);
                else
                    other.Add(item);
            }

            WritableCompletions.Clear();
            WritableCompletions.AddRange(zeroIndex.Concat(properties)
                                                  .Concat(nodes)
                                                  .Concat(other)
                                                  .ToList());

            if (WritableCompletions.Count > 0)
                SelectionStatus = new CompletionSelectionStatus(WritableCompletions[0], true, true);
        }

        private static int GetSymbolDistance(Completion c, string text)
        {
            var distance = LevenshteinDistance(c.InsertionText, text);

            if (c.InsertionText.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
                return 0;

            return distance;
        }

        private static int LevenshteinDistance(string first, string second)
        {
            if (IsNullOrEmpty(first) || IsNullOrEmpty(second))
                return 0;

            var firstLen = first.Length;
            var secondLen = second.Length;
            var distances = new int[firstLen + 1, secondLen + 1];

            for (var i = 0; i <= firstLen; i++) distances[i, 0] = i;
            for (var j = 0; j <= secondLen; j++) distances[0, j] = j;
            
            for (var i = 1; i <= firstLen; i++)
                for (var j = 1; j <= secondLen; j++) {
                    var cost = char.ToUpperInvariant(second[j - 1]) == char.ToUpperInvariant(first[i - 1]) ? 0 : 1;
                    var a = Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1);
                    var b = distances[i - 1, j - 1] + cost;

                    distances[i, j] = Math.Min(a,b);
                }

            return distances[firstLen, secondLen];
        }

        private bool ContainsAllSymbols(string searchTerm, string text)
        { 
            var a = searchTerm.ToUpper();
            var b = text.ToUpper();

            for (var i = 0; i < a.Length; i++)
                if (b.IndexOf(a[i]) == -1)
                    return false;

            return true;
        }

        public void UpdateApplicableTo(ITrackingSpan newApplicableTo)
        {
            ApplicableTo = newApplicableTo;
        }
    }

    internal class CompletionEqualityComparer : IEqualityComparer<MyCompletion>
    {
        public bool Equals(MyCompletion x, MyCompletion y)
        {
            return x.DisplayText == y.DisplayText && ReferenceEquals(x.IconSource, y.IconSource);
        }

        public int GetHashCode(MyCompletion obj)
        {
            if (obj.IconSource != null)
                return obj.DisplayText.GetHashCode() * 1234 ^ obj.IconSource.GetHashCode() * 4321;

            return obj.DisplayText.GetHashCode() * 1234;
        }
    }
}