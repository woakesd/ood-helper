using System.Collections.Generic;

namespace OodHelper
{
    public interface IRaceScore
    {
        double StandardCorrectedTime { get; }

        int Finishers { get; }

        /// <summary>User-facing notices produced by the last <see cref="Calculate"/> (shown by the caller).</summary>
        IReadOnlyList<string> Warnings { get; }

        void Calculate(int rid);
    }
}
