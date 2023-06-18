using System;
using System.Collections.Generic;

namespace BusinessLogic.Entities;

public partial class RentedProperty
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Location { get; set; } = null!;

    public decimal PropertyValue { get; set; }

    public decimal RentalValue { get; set; }

    public decimal MonthlyCondominiumFee { get; set; }

    public decimal EstimatedAnnualExpenses { get; set; }

    public virtual Asset IdNavigation { get; set; } = null!;
}
