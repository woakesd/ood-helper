using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class PortsmouthNumber
{
    public Guid Id { get; set; }

    public string?ClassName { get; set; }

    public int? NoOfCrew { get; set; }

    public string?Rig { get; set; }

    public string?Spinnaker { get; set; }

    public string?Engine { get; set; }

    public string?Keel { get; set; }

    public int? Number { get; set; }

    public string?Status { get; set; }

    public string?Notes { get; set; }
}
