using System;
using System.Collections.Generic;

namespace BusinessLogic.Entities;

public partial class Asset
{
    public int AssetId { get; set; }

    public int UserId { get; set; }

    public string AssetType { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public int DurationInMonths { get; set; }

    public decimal TaxPercentage { get; set; }

    public virtual FixedDeposit? FixedDeposit { get; set; }

    public virtual InvestmentFund? InvestmentFund { get; set; }

    public virtual RentedProperty? RentedProperty { get; set; }

    public virtual User User { get; set; } = null!;
}
