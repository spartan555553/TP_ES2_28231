using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Context;
using BusinessLogic.Entities;

namespace Backend.Controllers
{
    [Route("api/Funds")]
    [ApiController]
    public class FundsController : ControllerBase
    {
        private readonly ES2DbContext _context;

        public FundsController(ES2DbContext context)
        {
            _context = context;
        }

        //Method to get create an investment fund and respective asset object
        [HttpPost("create")]
        public async Task<IActionResult> HandleCreateFund(
            [FromQuery] string name,
            [FromQuery] decimal investmentAmount,
            [FromQuery] decimal defaultInterestRate,
            [FromQuery] DateOnly startDate,
            [FromQuery] int durationInMonths,
            [FromQuery] decimal taxPercentage)
        {
            try
            {
                if (taxPercentage <= 0)
                {
                    return BadRequest("Tax percentage must be a positive value.");
                }
                
                // Create an asset first
                var asset = new Asset
                {
                    UserId = UserSession.GetLoggedId(),
                    AssetType = "Investment Fund",
                    StartDate = startDate,
                    DurationInMonths = durationInMonths,
                    TaxPercentage = taxPercentage
                };

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();

                // Get the most recently created Asset
                var assetId = asset.AssetId;
                var createdAsset = await _context.Assets.FindAsync(assetId);

                // Create the investment fund associated with the asset
                var fund = new InvestmentFund
                {
                    Name = name,
                    InvestmentAmount = investmentAmount,
                    DefaultInterestRate = defaultInterestRate,
                    IdNavigation = createdAsset
                };

                _context.InvestmentFunds.Add(fund);
                await _context.SaveChangesAsync();

                // Create monthly interest rates for the investment fund
                var startDateMonth = new DateOnly(startDate.Year, startDate.Month, 1);
                for (int i = 0; i < durationInMonths; i++)
                {
                    var month = startDateMonth.AddMonths(i);
                    var interestRate = defaultInterestRate;

                    var monthlyInterestRate = new MonthlyInterestRate
                    {
                        FundId = fund.Id,
                        Month = month,
                        InterestRate = interestRate
                    };

                    _context.MonthlyInterestRates.Add(monthlyInterestRate);
                }
                await _context.SaveChangesAsync();

                return Ok("Fund created successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        //Method to get all investment funds
        [HttpGet("get")]
        public async Task<IActionResult> GetFunds()
        {
            try
            {
                var funds = await _context.InvestmentFunds
                    .Include(f => f.IdNavigation) // Include the associated Asset
                    .ToListAsync();

                var fundsDTOs = funds.Select(f => new
                {
                    FundId = f.Id,
                    Name = f.Name,
                    InvestmentAmount = f.InvestmentAmount,
                    DefaultInterestRate = f.DefaultInterestRate,
                    AssetId = f.IdNavigation.AssetId,
                    AssetType = f.IdNavigation.AssetType,
                    StartDate = f.IdNavigation.StartDate,
                    DurationInMonths = f.IdNavigation.DurationInMonths,
                    TaxPercentage = f.IdNavigation.TaxPercentage
                });

                return Ok(fundsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Method to get list all investment funds belonging to a user
        [HttpGet("list")]
        public async Task<IActionResult> ListFunds(int userId)
        {
            try
            {
                var funds = await _context.InvestmentFunds
                    .Include(f => f.IdNavigation) // Include the associated Asset
                    .Where(f => f.IdNavigation.UserId == userId) // Filter by userId
                    .ToListAsync();

                var fundsDTOs = funds.Select(f => new
                {
                    FundId = f.Id,
                    Name = f.Name,
                    InvestmentAmount = f.InvestmentAmount,
                    DefaultInterestRate = f.DefaultInterestRate,
                    StartDate = f.IdNavigation.StartDate,
                    DurationInMonths = f.IdNavigation.DurationInMonths,
                    TaxPercentage = f.IdNavigation.TaxPercentage
                });

                return Ok(fundsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Method to get delete a investment fund and respective asset based on a fund id
        [HttpDelete("delete/{fundId}")]
        public async Task<IActionResult> DeleteSelectedFund(int fundId)
        {
            try
            {
                var fund = await _context.InvestmentFunds
                    .Include(f => f.IdNavigation) // Include the associated Asset
                    .FirstOrDefaultAsync(f => f.Id == fundId);

                if (fund != null)
                {
                    var assetId = fund.IdNavigation.AssetId;

                    // Delete associated MonthlyInterestRate records
                    var monthlyInterestRates = await _context.MonthlyInterestRates
                        .Where(m => m.FundId == fundId)
                        .ToListAsync();
            
                    _context.MonthlyInterestRates.RemoveRange(monthlyInterestRates);

                    // Delete the fund
                    _context.InvestmentFunds.Remove(fund);
                    await _context.SaveChangesAsync();

                    // Delete the asset
                    var asset = await _context.Assets
                        .FirstOrDefaultAsync(a => a.AssetId == assetId);

                    _context.Assets.Remove(asset);
                    await _context.SaveChangesAsync();

                    return Ok("Fund and associated records deleted successfully.");
                }
                else
                {
                    return NotFound("No fund found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Method to get update a investment fund and respective asset object based on a fund id
        [HttpPost("update/{fundId}")]
        public async Task<IActionResult> UpdateSelectedFund(int fundId,
            [FromQuery] string name,
            [FromQuery] decimal investmentAmount,
            [FromQuery] decimal defaultInterestRate,
            [FromQuery] DateOnly startDate,
            [FromQuery] int durationInMonths,
            [FromQuery] decimal taxPercentage)
        {
            try
            {
                var fund = await _context.InvestmentFunds
                    .Include(f => f.IdNavigation) // Include the associated Asset
                    .FirstOrDefaultAsync(f => f.Id == fundId);

                if (fund != null)
                {
                    // Update fund attributes
                    fund.Name = name;
                    fund.InvestmentAmount = investmentAmount;
                    fund.DefaultInterestRate = defaultInterestRate;

                    // Update associated asset attributes
                    fund.IdNavigation.StartDate = startDate;
                    fund.IdNavigation.DurationInMonths = durationInMonths;
                    fund.IdNavigation.TaxPercentage = taxPercentage;

                    await _context.SaveChangesAsync();

                    // Update monthly interest rates
                    var startDateMonth = new DateOnly(startDate.Year, startDate.Month, 1);
                    var monthlyInterestRates = await _context.MonthlyInterestRates
                        .Where(m => m.FundId == fundId)
                        .ToListAsync();

                    for (int i = 0; i < durationInMonths; i++)
                    {
                        var month = startDateMonth.AddMonths(i);
                        var interestRate = defaultInterestRate;

                        var monthlyInterestRate = monthlyInterestRates.FirstOrDefault(m => m.Month == month);
                        if (monthlyInterestRate != null)
                        {
                            monthlyInterestRate.InterestRate = interestRate;
                        }
                        else
                        {
                            monthlyInterestRate = new MonthlyInterestRate
                            {
                                FundId = fundId,
                                Month = month,
                                InterestRate = interestRate
                            };
                            _context.MonthlyInterestRates.Add(monthlyInterestRate);
                        }
                    }
                    await _context.SaveChangesAsync();

                    return Ok("Fund updated successfully.");
                }
                else
                {
                    return NotFound("No fund found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
