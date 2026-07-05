using System;
using System.Collections.Generic;
using System.Linq;
using OodHelper.Data;

namespace OodHelper.Results
{
    /// <summary>
    /// Host view-model for the Race Results screen. Owns one
    /// <see cref="ResultsEditorViewModel"/> per race id and exposes the data operations behind
    /// the per-tab context-menu commands. The <see cref="RaceResults"/> code-behind keeps the
    /// WPF-specific plumbing (tabs, context menus, the print pipeline) and drives it from here.
    /// </summary>
    public sealed class RaceResultsViewModel
    {
        private readonly IRaceResultsRepository _repo;

        public IReadOnlyList<ResultsEditorViewModel> Editors { get; }

        public RaceResultsViewModel(int[] rids, IRaceResultsRepository repo,
            Func<int, ResultsEditorViewModel> editorFactory)
        {
            _repo = repo;
            Editors = rids.Select(editorFactory).ToList();
        }

        /// <summary>Copy the boat's current handicaps onto the given race entry (Edit Boat).</summary>
        public void ApplyEditedBoatHandicaps(int rid, int bid)
        {
            _repo.ApplyEditedBoatHandicaps(rid, bid);
        }

        /// <summary>Move a boat's entry from one race/fleet to another.</summary>
        public void MoveToFleet(int fromRid, int toRid, int bid)
        {
            _repo.MoveToFleet(fromRid, toRid, bid);
        }
    }
}
