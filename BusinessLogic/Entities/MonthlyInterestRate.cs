using System;
using System.Collections.Generic;

namespace BusinessLogic.Entities;

public partial class MonthlyInterestRate
{
    public int Id { get; set; }

    public int FundId { get; set; }

    public DateOnly Month { get; set; }

    public decimal InterestRate { get; set; }

    public virtual InvestmentFund Fund { get; set; } = null!;
}
