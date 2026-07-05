using System;
using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    public interface IPortsmouthNumberRepository
    {
        /// <summary>All classes ordered by name, optionally filtered (substring, case-insensitive).</summary>
        IReadOnlyList<PortsmouthNumber> GetAll(string filter);
        PortsmouthNumber Get(Guid id);
        /// <summary>Insert or update a single class. Assigns a new id when the entity has none.</summary>
        void Save(PortsmouthNumber pn);
        void Delete(Guid id);
        /// <summary>Replace the whole table with the supplied rows (used by the CSV import).</summary>
        void ReplaceAll(IEnumerable<PortsmouthNumber> rows);
    }
}
