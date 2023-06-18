using System;
using System.Collections.Generic;

namespace BusinessLogic.Entities;

public partial class FixedDeposit
{
    public int Id { get; set; }

    public decimal Value { get; set; }

    public string Bank { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolders { get; set; } = null!;

    public decimal AnnualInterestRate { get; set; }

    public virtual Asset IdNavigation { get; set; } = null!;
}
