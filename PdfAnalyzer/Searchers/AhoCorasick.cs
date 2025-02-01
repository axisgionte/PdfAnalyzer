using System;
using System.Collections.Generic;
using System.Threading;

namespace PdfAnalyzer.Searchers
{
    public static class AhoCorasickExtensions
    {
        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children = new Dictionary<char, TrieNode>();
            public TrieNode FailureLink;
            public List<string> Output = new List<string>();
        }

        private static TrieNode BuildTrie(List<string> keywords, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var root = new TrieNode();
            foreach (var keyword in keywords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var node = root;
                foreach (var ch in keyword)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (node.Children.ContainsKey(ch) == false)
                    {
                        node.Children[ch] = new TrieNode();
                    }
                    node = node.Children[ch];
                }
                node.Output.Add(keyword);
            }
            return root;
        }

        private static void BuildFailureLinks(TrieNode root, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var queue = new Queue<TrieNode>();
            foreach (var child in root.Children.Values)
            {
                child.FailureLink = root;
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var node = queue.Dequeue();
                foreach (var kvp in node.Children)
                {
                    char ch = kvp.Key;
                    TrieNode child = kvp.Value;

                    cancellationToken.ThrowIfCancellationRequested();

                    queue.Enqueue(child);

                    TrieNode fail = node.FailureLink;
                    while (fail != null && fail.Children.ContainsKey(ch) == false)
                    {
                        fail = fail.FailureLink;
                    }

                    child.FailureLink = fail?.Children[ch] ?? root;
                    child.Output.AddRange(child.FailureLink.Output);
                }
            }
        }

        public static Dictionary<string, List<int>> AhoCorasickSearch(this string text, List<string> lines, bool findAny, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var root = BuildTrie(lines, cancellationToken);
            BuildFailureLinks(root, cancellationToken);
            var result = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            var node = root;


            cancellationToken.ThrowIfCancellationRequested();

            for (int i = 0; i < text.Length; i++)
            {

                cancellationToken.ThrowIfCancellationRequested();

                while (node != null && !node.Children.ContainsKey(text[i]))
                {
                    node = node.FailureLink;
                }

                node = node == null ? root : node.Children[text[i]];

                foreach (var match in node.Output)
                {
                    if (result.ContainsKey(match) == false)
                    {
                        result.Add(match, new List<int>());
                    }
                    result[match].Add(i - match.Length + 1);

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
