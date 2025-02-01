using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PdfAnalyzer.Searchers
{
    public static class DefaultSearcher
    {
        public static Dictionary<string, List<int>> DefaultSearch(this string text, List<string> lines, bool findAny, CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, List<int>>(StringComparer.Ordinal);

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(text) || lines == null || lines.Any() == false)
            {
                return result;
            }

            foreach (var line in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var index = text.IndexOf(line, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    if (!result.ContainsKey(line))
                    {
                        result.Add(line, new List<int>());
                    }

                    result[line].Add(index);

                    if (findAny == false)
                    {
                        return result;
                    }

                    if (findAny && result.Count == lines.Count)
                    {
                        return result;
                    }
                }
            }

            return result;
        }
    }
}
