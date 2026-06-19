using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class Person
{
    public int Id { get; set; }

    public int? MainId { get; set; }

    public string Firstname { get; set; }

    public string Surname { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string Address3 { get; set; }

    public string Address4 { get; set; }

    public string Postcode { get; set; }

    public string Hometel { get; set; }

    public string Worktel { get; set; }

    public string Mobile { get; set; }

    public string Email { get; set; }

    public string Club { get; set; }

    public string Member { get; set; }

    public string Manmemo { get; set; }

    public bool? Cp { get; set; }

    public bool? S { get; set; }

    public bool? Novice { get; set; }

    public Guid? Uid { get; set; }

    public bool? Papernewsletter { get; set; }

    public bool? Handbookexclude { get; set; }

    public virtual ICollection<Boat> Boats { get; set; } = new List<Boat>();
}
