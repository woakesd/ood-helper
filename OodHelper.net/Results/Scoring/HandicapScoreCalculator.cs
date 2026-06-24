using System;
using System.Collections.Generic;
using System.Linq;
using OodHelper.Data.Entities;

namespace OodHelper.Results.Scoring
{
    /// <summary>
    /// Pure (no <c>Db</c>, no EF, no WPF) port of the open / rolling handicap scoring math that used
    /// to live in <c>OpenHandicap</c> and <c>RollingHandicap</c>. Operates directly on the supplied
    /// <see cref="Race"/> rows, mutating them in place (elapsed, corrected, places, points, achieved
    /// and new rolling handicaps, fast/slow flags), and returns the run's scalars via
    /// <see cref="HandicapScoreOutcome"/>. The two engines differed only in <see cref="CorrectedTime"/>,
    /// so they are unified here and switched by <see cref="HandicapScoreInputs.Mode"/>.
    /// </summary>
    public static class HandicapScoreCalculator
    {
        public static HandicapScoreOutcome Calculate(IList<Race> rows, HandicapScoreInputs inputs)
        {
            return new Runner(rows, inputs).Run();
        }

        private sealed class Runner
        {
            private readonly IList<Race> _rows;
            private readonly HandicapScoreInputs _in;
            private readonly List<string> _warnings = new();

            private DateTime? _timeLimit;
            private double _sct;
            private double _slowLimit;
            private double _fastLimit;
            private int _finishers;

            public Runner(IList<Race> rows, HandicapScoreInputs inputs)
            {
                _rows = rows;
                _in = inputs;
                _timeLimit = inputs.TimeLimit;
            }

            public HandicapScoreOutcome Run()
            {
                //
                // Finishers are boats whose finish is inside the (un-extended) time limit. Note the
                // lifted comparison: when there is no time limit this is always 0 — preserved from
                // the original DataTable code.
                //
                _finishers = _rows.Count(r => r.FinishDate <= _timeLimit);
                if (_finishers == 0)
                {
                    _warnings.Add(
                        "All boats have finished outside the timelimit\nNo calculation can be performed.");
                    return new HandicapScoreOutcome(0, 0, false, _warnings);
                }

                FlagDidNotFinish();
                InitialiseFields();
                CorrectedTime();
                CalculateSct();
                Score();
                UpdateHandicaps();

                return new HandicapScoreOutcome(_sct, _finishers, true, _warnings);
            }

            private void FlagDidNotFinish()
            {
                var nonFinishers = false;
                if (_timeLimit.HasValue)
                {
                    if (_in.Extension.HasValue)
                        _timeLimit = _timeLimit.Value.AddSeconds(_in.Extension.Value);

                    foreach (var r in _rows.Where(r => r.FinishDate > _timeLimit))
                    {
                        r.FinishCode = "DNF";
                        nonFinishers = true;
                    }
                }

                if (nonFinishers)
                    _warnings.Add(
                        "Some boats have finished outside the timelimit\n(plus extension if applicable) and have been marked DNF");
            }

            private void InitialiseFields()
            {
                foreach (var r in _rows)
                {
                    r.Elapsed = null;
                    r.Corrected = null;
                    r.StandardCorrected = null;
                    r.Place = 999;
                    r.Points = null;
                    if (r.RollingHandicap == null)
                    {
                        int? nrh = _in.PreviousNewRollingHandicaps.TryGetValue(r.Bid, out var v)
                            ? v
                            : (int?)null;
                        if (!nrh.HasValue)
                            nrh = r.OpenHandicap.Value;
                        r.RollingHandicap = nrh.Value;
                    }
                    var newhc = r.RollingHandicap.Value;
                    if (r.RestrictedSail == true)
                        newhc = (int)Math.Round(newhc / _in.RsCoefficient);

                    r.NewRollingHandicap = newhc;
                    r.AchievedHandicap = r.RollingHandicap;
                    r.PerformanceIndex = null;
                    r.C = null;
                    r.A = "N";

                    //
                    // If average lap and the user enters a finish time but no laps, default to 1.
                    //
                    if ((_in.RaceType == CalendarModel.RaceTypes.AverageLap
                         || _in.RaceType == CalendarModel.RaceTypes.Hybrid
                         || _in.RaceType == CalendarModel.RaceTypes.HybridOld)
                        && r.Laps == null && r.FinishDate != null)
                        r.Laps = 1;
                }
            }

            private void CorrectedTime()
            {
                var query = _rows.Where(r =>
                        r.FinishCode != "DNF"
                        && r.FinishCode != "DSQ"
                        && r.FinishCode != "DNE"
                        && r.StartDate != null
                        && r.FinishDate != null
                        && (_in.RaceType != CalendarModel.RaceTypes.HybridOld
                            || r.InterimDate != null && r.Laps != null)
                        && (_in.RaceType != CalendarModel.RaceTypes.Hybrid
                            || r.InterimDate != null && r.Laps != null)
                        && (_in.RaceType != CalendarModel.RaceTypes.AverageLap || r.Laps != null))
                    .ToList();

                //
                // Average laps (only used by the Hybrid case but computed exactly as before).
                //
                var avgLaps = Math.Round(query.Average(r => r.Laps ?? 0), 1);

                foreach (var dr in query)
                {
                    var start = dr.StartDate;
                    var finish = dr.FinishDate;
                    var interim = dr.InterimDate;

                    var elapsed = finish - start;
                    dr.Elapsed = Convert.ToInt32(elapsed.Value.TotalSeconds);

                    var laps = dr.Laps;
                    //
                    // The open-handicap engine clamps a zero lap count to 1; the rolling engine
                    // historically did not — preserved.
                    //
                    if (_in.Mode == HandicapMode.Open && laps.HasValue && laps.Value == 0)
                        laps = 1;

                    var hcap = _in.Mode == HandicapMode.Open
                        ? dr.OpenHandicap.Value
                        : dr.RollingHandicap.Value;
                    var ohp = dr.OpenHandicap.Value;

                    TimeSpan? fixedPart;
                    TimeSpan? averageLapPart;

                    switch (_in.RaceType)
                    {
                        case CalendarModel.RaceTypes.AverageLap:
                            dr.Corrected = Math.Round(elapsed.Value.TotalSeconds * 1000 / hcap) / laps.Value;
                            break;
                        case CalendarModel.RaceTypes.HybridOld:
                            fixedPart = interim - start;
                            averageLapPart = finish - interim;
                            dr.Corrected = Math.Round(fixedPart.Value.TotalSeconds * 1000 / hcap) +
                                           Math.Round(averageLapPart.Value.TotalSeconds * 1000 / hcap) / laps.Value;
                            break;
                        case CalendarModel.RaceTypes.Hybrid:
                            fixedPart = interim - start;
                            averageLapPart = finish - interim;
                            dr.Corrected = Math.Round(fixedPart.Value.TotalSeconds * 1000 / hcap) +
                                           Math.Round(averageLapPart.Value.TotalSeconds * 1000 / hcap * avgLaps) / laps.Value;
                            break;
                        case CalendarModel.RaceTypes.FixedLength:
                        case CalendarModel.RaceTypes.TimeGate:
                            dr.Corrected = Math.Round(elapsed.Value.TotalSeconds * 1000 / hcap);
                            break;
                    }

                    if (_in.Mode == HandicapMode.Open)
                    {
                        //
                        // Open: standard corrected == corrected (both use the open handicap).
                        //
                        dr.StandardCorrected = dr.Corrected;
                    }
                    else
                    {
                        //
                        // Rolling: standard corrected is the same computation against the open
                        // handicap (corrected used the rolling handicap above).
                        //
                        switch (_in.RaceType)
                        {
                            case CalendarModel.RaceTypes.AverageLap:
                                dr.StandardCorrected = Math.Round(elapsed.Value.TotalSeconds * 1000 / ohp) / laps.Value;
                                break;
                            case CalendarModel.RaceTypes.HybridOld:
                                fixedPart = interim - start;
                                averageLapPart = finish - interim;
                                dr.StandardCorrected = Math.Round(fixedPart.Value.TotalSeconds * 1000 / ohp) +
                                                       Math.Round(averageLapPart.Value.TotalSeconds * 1000 / ohp) / laps.Value;
                                break;
                            case CalendarModel.RaceTypes.Hybrid:
                                fixedPart = interim - start;
                                averageLapPart = finish - interim;
                                dr.StandardCorrected = Math.Round(fixedPart.Value.TotalSeconds * 1000 / ohp) +
                                                       Math.Round(averageLapPart.Value.TotalSeconds * 1000 / ohp * avgLaps) / laps.Value;
                                break;
                            case CalendarModel.RaceTypes.FixedLength:
                            case CalendarModel.RaceTypes.TimeGate:
                                dr.StandardCorrected = Math.Round(elapsed.Value.TotalSeconds * 1000 / ohp);
                                break;
                        }
                    }

                    dr.Place = 0;
                }
            }

            private void CalculateSct()
            {
                //
                // Good boats: those that were scored (place 0) and aren't temporary numbers (TN),
                // ordered by standard corrected time.
                //
                var query = _rows
                    .Where(r => r.Place == 0 && r.HandicapStatus != "TN")
                    .OrderBy(r => r.StandardCorrected)
                    .ToList();

                var qual = query.Count;
                if (qual < 2)
                {
                    _sct = 0;
                }
                else
                {
                    //
                    // Average corrected time of the top 2/3 of the good boats.
                    //
                    double total = 0;
                    var n = (int)Math.Round(qual * 0.67);
                    for (var i = 0; i < n; i++)
                        total += query[i].StandardCorrected.Value;

                    var averageCorrectedTime = total / n;

                    //
                    // Slow limit = average + 5%. Good boats beating it set the SCT.
                    //
                    var avgSlowLimit = averageCorrectedTime * 1.05;

                    var goodBoats = 0;
                    _sct = 0;
                    foreach (var row in query)
                    {
                        if (row.StandardCorrected.Value < avgSlowLimit)
                        {
                            row.A = null;
                            goodBoats++;
                            _sct += row.StandardCorrected.Value;
                        }
                    }

                    if (goodBoats > 1)
                    {
                        _sct = Math.Round(_sct / goodBoats);
                        _slowLimit = Math.Round(_sct * 1.05);
                        _fastLimit = Math.Round(_sct * 0.95);
                    }
                    else
                    {
                        _sct = 0;
                        _slowLimit = 0;
                        _fastLimit = 0;
                    }
                }
            }

            private void Score()
            {
                //
                // Places (ties share a place; the following boat keeps the gap, per RRS).
                //
                var query = _rows
                    .Where(r => r.Place == 0)
                    .OrderBy(r => r.Corrected)
                    .ToList();

                for (var i = 0; i < query.Count; i++)
                {
                    if (i == 0)
                        query[i].Place = 1;
                    else if (query[i - 1].Corrected == query[i].Corrected)
                        query[i].Place = query[i - 1].Place;
                    else
                        query[i].Place = i + 1;
                }

                //
                // Points: normally the place, but tied boats share the points for the places they
                // occupy (two tied for 3rd share 3rd+4th = 3.5 each).
                //
                var j = 0;
                while (j < query.Count)
                {
                    var k = j + 1;
                    double psum = k;
                    while (k < query.Count)
                    {
                        if (query[j].Place != query[k].Place)
                            break;
                        k++;
                        psum += k;
                    }

                    var avg = Math.Round(psum / (k - j), 2);
                    for (var l = j; l < k; l++)
                        query[l].Points = avg;
                    j = k;
                }
            }

            private void UpdateHandicaps()
            {
                var query = _rows.Where(r => r.Place != 999).ToList();
                if (_sct > 0)
                {
                    foreach (var dr in query)
                    {
                        //
                        // Start from the current rolling handicap (restricted-sail boats have the
                        // 4% increase removed).
                        //
                        dr.AchievedHandicap = dr.OpenHandicap;
                        var oldhc = dr.RollingHandicap.Value;
                        if (dr.RestrictedSail == true)
                            dr.NewRollingHandicap = (int)Math.Round(oldhc / _in.RsCoefficient);
                        else
                            dr.NewRollingHandicap = oldhc;

                        //
                        // Achieved handicap = ratio of the boat's corrected time to the race SCT,
                        // scaled by the open handicap.
                        //
                        var achhc = (int)Math.Round(dr.StandardCorrected.Value / _sct * dr.OpenHandicap.Value);
                        dr.AchievedHandicap = achhc;

                        var sperf = false;
                        var sperfover = false;
                        if (dr.StandardCorrected.Value >= _slowLimit || dr.StandardCorrected.Value <= _fastLimit)
                        {
                            if (dr.StandardCorrected.Value >= _slowLimit)
                            {
                                dr.C = "s";
                                sperf = true;
                            }
                            else
                                dr.C = "F";

                            //
                            // Exceptional result: only allow the handicap to move if the previous
                            // result was similarly slow.
                            //
                            var p1 = _in.PriorPerformanceLookup(dr.Bid, dr.StartDate.Value);
                            if (p1.HasValue && p1.Value > 5)
                            {
                                sperfover = true;
                                dr.C = "S";
                            }
                        }

                        dr.PerformanceIndex = dr.AchievedHandicap.Value - dr.OpenHandicap.Value;

                        //
                        // Unless this is a (non-overridden) slow result, move the rolling handicap
                        // towards the achieved handicap, clamped to the +/-5% band.
                        //
                        if (!sperf || sperfover)
                        {
                            var working = achhc;
                            if (achhc > dr.OpenHandicap.Value * 1.05)
                                working = (int)Math.Round(1.05 * dr.OpenHandicap.Value, 0);
                            if (achhc < dr.OpenHandicap.Value * 0.95)
                                working = (int)Math.Round(0.95 * dr.OpenHandicap.Value, 0);

                            var newhc = (int)Math.Round(
                                dr.RollingHandicap.Value + (working - dr.RollingHandicap.Value) * _in.RhCoefficient);

                            if (dr.RestrictedSail == true)
                                newhc = (int)Math.Round(newhc / _in.RsCoefficient);

                            if (newhc >= dr.OpenHandicap.Value * 0.95 && newhc <= dr.OpenHandicap.Value * 1.05)
                                dr.NewRollingHandicap = newhc;
                        }
                    }
                }
            }
        }
    }
}
