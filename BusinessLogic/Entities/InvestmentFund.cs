using System;
using System.Collections.Generic;

namespace BusinessLogic.Entities;

public partial class InvestmentFund
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal InvestmentAmount { get; set; }

    public decimal DefaultInterestRate { get; set; }

    public virtual Asset IdNavigation { get; set; } = null!;

    public virtual ICollection<MonthlyInterestRate> MonthlyInterestRtes { get; set; } = new List<MonthlyInterestRate>();
}
