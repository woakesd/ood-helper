using System;
using System.Collections.Generic;
using OodHelper.Data;

namespace OodHelper.Results
{
    /// <summary>
    /// Stern-chase races aren't handicap-scored: the boats finish in order, so calculation just
    /// stamps the calendar as raced/calculated. Ported off the legacy <c>Db</c> helper onto
    /// <see cref="IRaceScoreRepository"/>.
    /// </summary>
    internal sealed class SternChaseScorer : IRaceScore
    {
        private readonly IRaceScoreRepository _repo;

        public SternChaseScorer(IRaceScoreRepository repo)
        {
            _repo = repo;
        }

        public double StandardCorrectedTime => 0.0;

        public int Finishers { get; private set; }

        public IReadOnlyList<string> Warnings { get; } = Array.Empty<string>();

        public void Calculate(int rid)
        {
            try
            {
                Finishers = _repo.GetScoringRows(rid).Count;
                _repo.MarkResultCalculated(rid);
            }
            catch { }
        }
    }
}
