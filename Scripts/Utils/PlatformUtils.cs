
using System;

public static class PlatformUtils
{
    private const int PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 30000; // How often to refresh the count, in milliseconds.
    private static volatile int s_processorCount; // The last count seen.
    private static volatile int s_lastProcessorCountRefreshTicks; // The last time we refreshed.

    public static int ProcessorCount
    {
        get
        {
            int now = Environment.TickCount;
            int procCount = s_processorCount;
            if (procCount == 0 || (now - s_lastProcessorCountRefreshTicks) >= PROCESSOR_COUNT_REFRESH_INTERVAL_MS)
            {
                s_processorCount = procCount = Environment.ProcessorCount;
                s_lastProcessorCountRefreshTicks = now;
            }

            return procCount;
        }
    }

    /// <summary>
    /// Gets whether the current machine has only a single processor.
    /// </summary>
    public static bool IsSingleProcessor
    {
        get { return ProcessorCount == 1; }
    }
}