using System;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    // Index the complete document once; allocate text only for the visible viewport.
    internal sealed class DataPatchJsonText
    {
        private readonly string source;
        private readonly int[] starts;
        public int LineCount => starts.Length;
        public int MaxColumns { get; }

        public DataPatchJsonText(string source)
        {
            this.source = source;
            var lines = new List<int>{0};
            int start = 0, longest = 0;
            for (int i = 0; i < source.Length; ++i)
            {
                if (source[i] != '\n')
                    continue;
                longest = Math.Max(longest, i - start);
                start = i + 1;
                lines.Add(start);
            }

            MaxColumns = Math.Max(longest, source.Length - start);
            starts = lines.ToArray();
        }

        public string Slice(int line, int column, int count)
        {
            if (line < 0 || line >= starts.Length || count <= 0)
                return "";
            int end = line + 1 < starts.Length ? starts[line + 1] - 1 : source.Length;
            if (end > starts[line] && source[end - 1] == '\r')
                --end;
            int start = starts[line] + Math.Min(Math.Max(0, column), end - starts[line]);
            int stop = start + Math.Min(count, end - start);
            // Include complete surrogate pairs at viewport edges.
            if (start > starts[line] && start < end && char.IsLowSurrogate(source[start]) && char.IsHighSurrogate(source[start - 1]))
                --start;
            if (stop < end && stop > start && char.IsHighSurrogate(source[stop - 1]) && char.IsLowSurrogate(source[stop]))
                ++stop;
            return source.Substring(start, stop - start);
        }
    }
}
