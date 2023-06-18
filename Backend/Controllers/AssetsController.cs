using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Context;
using BusinessLogic.Entities;

namespace Backend.Controllers
{
    [Route("api/Assets")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly ES2DbContext _context;

        public AssetsController(ES2DbContext context)
        {
            _context = context;
        }

        //Method to get all assets
        [HttpGet("get")]
        public async Task<IActionResult> GetAssets()
        {
            try
            {
                var assets = await _context.Assets.ToListAsync();

                var assetsDTOs = assets.Select(a => new
                {
                    AssetId = a.AssetId,
                    AssetType = a.AssetType,
                    StartDate = a.StartDate,
                    DurationInMonths = a.DurationInMonths,
                    TaxPercentage = a.TaxPercentage
                });

                return Ok(assetsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Method to list all assets belonging to a user
        [HttpGet("list")]
        public async Task<IActionResult> ListAssets(int userId)
        {
            try
            {
                var assets = await _context.Assets
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                var assetsDTOs = assets.Select(a => new
                {
                    AssetId = a.AssetId,
                    AssetType = a.AssetType,
                    StartDate = a.StartDate,
                    DurationInMonths = a.DurationInMonths,
                    TaxPercentage = a.TaxPercentage
                });

                return Ok(assetsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // Method to search assets by type, name, or amount
        [HttpGet("search")]
        public async Task<IActionResult> SearchAssets(string searchTerm)
        {
            try
            {
                var assetsQuery = _context.Assets.AsQueryable();

                var assets = await assetsQuery
                    .Where(a => a.AssetType.Contains(searchTerm) ||
                                (a.AssetType == "Rented Property" && a.RentedProperty.Name.Contains(searchTerm)) ||
                                (a.AssetType == "Investment Fund" && a.InvestmentFund.InvestmentAmount.ToString().Contains(searchTerm)) || 
                                (a.AssetType == "Rented Property" && a.RentedProperty.Name.Contains(searchTerm)) ||
                                (a.AssetType == "Fixed Deposit" && a.FixedDeposit.Value.ToString().Contains(searchTerm)))
                    .ToListAsync();

                var assetsDTOs = assets.Select(a => new
                {
                    AssetId = a.AssetId,
                    AssetType = a.AssetType,
                    StartDate = a.StartDate,
                    DurationInMonths = a.DurationInMonths,
                    TaxPercentage = a.TaxPercentage
                });

                return Ok(assetsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // Method to generate a report of assets owned by the currently logged-in user active between two dates
        [HttpGet("report/profit")]
        public async Task<IActionResult> GenerateAssetReportOfLoggedInUser(DateOnly startDate, DateOnly endDate)
        {
            try
            {
                // Get the user ID of the currently logged-in user
                var userId = UserSession.GetLoggedId();

                var assets = await _context.Assets
                    .Include(a => a.FixedDeposit)
                    .Include(a => a.RentedProperty)
                    .Include(a => a.InvestmentFund)
                    .Where(a => a.UserId == userId && a.StartDate >= startDate && a.StartDate <= endDate)
                    .ToListAsync();

                // Generate the report for the user's assets
                var reportData = assets.Select(a =>
                {
                    decimal totalProfitBeforeTax = 0;
                    decimal totalProfitAfterTax = 0;
                    decimal averageMonthlyProfitBeforeTax = 0;
                    decimal averageMonthlyProfitAfterTax = 0;

                    if (a.AssetType == "Fixed Deposit")
                    {
                        // Access the FixedDeposit object associated with the asset
                        var fixedDeposit = a.FixedDeposit;

                        // Calculation logic for AssetType Fixed Deposit
                        totalProfitBeforeTax = fixedDeposit.Value;
                        totalProfitAfterTax = fixedDeposit.Value * a.TaxPercentage;
                        averageMonthlyProfitBeforeTax = fixedDeposit.Value / a.DurationInMonths;
                        averageMonthlyProfitAfterTax = (fixedDeposit.Value * a.TaxPercentage) / a.DurationInMonths;
                    }
                    else if (a.AssetType == "Rented Properties")
                    {
                        // Access the rentedProperty object associated with the asset
                        var rentedProperty = a.RentedProperty;
                        
                        // Calculation logic for AssetType Rented Properties
                        totalProfitBeforeTax = rentedProperty.RentalValue-rentedProperty.MonthlyCondominiumFee-rentedProperty.EstimatedAnnualExpenses;
                        totalProfitAfterTax = (rentedProperty.RentalValue-rentedProperty.MonthlyCondominiumFee-rentedProperty.EstimatedAnnualExpenses)*a.TaxPercentage;
                        averageMonthlyProfitBeforeTax = (rentedProperty.RentalValue-rentedProperty.MonthlyCondominiumFee-rentedProperty.EstimatedAnnualExpenses)/a.DurationInMonths;
                        averageMonthlyProfitAfterTax = ((rentedProperty.RentalValue-rentedProperty.MonthlyCondominiumFee-rentedProperty.EstimatedAnnualExpenses)*a.TaxPercentage)/a.DurationInMonths;
                    }
                    else if (a.AssetType == "Investment Fund")
                    {
                        // Access the rentedProperty object associated with the asset
                        var fund = a.InvestmentFund;
                        
                        // Calculation logic for AssetType Rented Properties
                        totalProfitBeforeTax = fund.InvestmentAmount*fund.DefaultInterestRate;
                        totalProfitAfterTax = (fund.InvestmentAmount*fund.DefaultInterestRate)*a.TaxPercentage;
                        averageMonthlyProfitBeforeTax = (fund.InvestmentAmount*fund.DefaultInterestRate)/a.DurationInMonths;
                        averageMonthlyProfitAfterTax = ((fund.InvestmentAmount * fund.DefaultInterestRate) * a.TaxPercentage) / a.DurationInMonths;
                    }

                    return new
                    {
                        AssetId = a.AssetId,
                        AssetType = a.AssetType,
                        StartDate = a.StartDate,
                        DurationInMonths = a.DurationInMonths,
                        TaxPercentage = a.TaxPercentage,
                        TotalProfitBeforeTax = totalProfitBeforeTax,
                        TotalProfitAfterTax = totalProfitAfterTax,
                        AverageMonthlyProfitBeforeTax = averageMonthlyProfitBeforeTax,
                        AverageMonthlyProfitAfterTax = averageMonthlyProfitAfterTax
                    };
                });

                return Ok(reportData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}