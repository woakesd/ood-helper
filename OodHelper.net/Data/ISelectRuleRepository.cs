using System;
using System.Collections.Generic;
using OodHelper.Rules;

namespace OodHelper.Data
{
    /// <summary>A top-level rule as shown in the Select Rules list window.</summary>
    public sealed record SelectRuleListItem(Guid Id, string? Name);

    public interface ISelectRuleRepository
    {
        IReadOnlyList<SelectRuleListItem> GetTopLevel(string filter);
        BoatSelectRule? GetTree(Guid id);
        BoatSelectRule GetTree(string name);
        void Save(BoatSelectRule root);
        void Delete(Guid id);
    }
}
