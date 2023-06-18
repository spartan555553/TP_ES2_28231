using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Context;
using BusinessLogic.Entities;

namespace Backend.Controllers
{
    [Route("api/Deposits")]
    [ApiController]
    public class DepositsController : ControllerBase
    {
        private readonly ES2DbContext _context;

        public DepositsController(ES2DbContext context)
        {
            _context = context;
        }

        //Method to get create a new deposit and respective asset object
        [HttpPost("create")]
        public async Task<IActionResult> HandleCreateDeposit(
            [FromQuery] decimal value,
            [FromQuery] string bank,
            [FromQuery] string accountNumber,
            [FromQuery] string accountHolders,
            [FromQuery] decimal annualInterestRate,
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
                    AssetType = "Fixed Deposit",
                    StartDate = startDate,
                    DurationInMonths = durationInMonths,
                    TaxPercentage = taxPercentage
                };

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();

                // Get the most recently created Asset
                var assetId = asset.AssetId;
                var createdAsset = await _context.Assets.FindAsync(assetId);

                // Create the deposit associated with the asset
                var deposit = new FixedDeposit
                {
                    IdNavigation = createdAsset,
                    Value = value,
                    Bank = bank,
                    AccountNumber = accountNumber,
                    AccountHolders = accountHolders,
                    AnnualInterestRate = annualInterestRate
                };

                _context.FixedDeposits.Add(deposit);
                await _context.SaveChangesAsync();
                
                

                return Ok("Deposit created successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        //Method to get all deposits
        [HttpGet("get")]
        public async Task<IActionResult> GetDeposits()
        {
            try
            {
                var deposits = await _context.FixedDeposits
                    .Include(d => d.IdNavigation) // Include the associated Asset
                    .ToListAsync();

                var depositsDTOs = deposits.Select(d => new
                {
                    DepositId = d.Id,
                    Value = d.Value,
                    Bank = d.Bank,
                    AccountNumber = d.AccountNumber,
                    AccountHolders = d.AccountHolders,
                    AnnualInterestRate = d.AnnualInterestRate,
                    AssetId = d.IdNavigation.AssetId,
                    AssetType = d.IdNavigation.AssetType,
                    StartDate = d.IdNavigation.StartDate,
                    DurationInMonths = d.IdNavigation.DurationInMonths,
                    TaxPercentage = d.IdNavigation.TaxPercentage
                });

                return Ok(depositsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        // Method to get a deposit by ID
        [HttpGet("get/{depositId}")]
        public async Task<IActionResult> GetDepositById(int depositId)
        {
            try
            {
                var deposit = await _context.FixedDeposits
                    .FirstOrDefaultAsync(d => d.Id == depositId);

                if (deposit != null)
                {
                    var depositDTO = new
                    {
                        DepositId = deposit.Id,
                        Value = deposit.Value,
                        Bank = deposit.Bank,
                        AccountNumber = deposit.AccountNumber,
                        AccountHolders = deposit.AccountHolders,
                        AnnualInterestRate = deposit.AnnualInterestRate
                    };

                    return Ok(depositDTO);
                }
                else
                {
                    return NotFound("No deposit found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        //Method to get list all deposits belonging to a user
        [HttpGet("list")]
        public async Task<IActionResult> ListDeposits(int userId)
        {
            try
            {
                var deposits = await _context.FixedDeposits
                    .Include(d => d.IdNavigation) // Include the associated Asset
                    .Where(d => d.IdNavigation.UserId == userId) // Filter by userId
                    .ToListAsync();

                var depositsDTOs = deposits.Select(d => new
                {
                    DepositId = d.Id, // Include the DepositId field
                    Value = d.Value,
                    Bank = d.Bank,
                    AccountNumber = d.AccountNumber,
                    AccountHolders = d.AccountHolders,
                    AnnualInterestRate = d.AnnualInterestRate,
                    StartDate = d.IdNavigation.StartDate,
                    DurationInMonths = d.IdNavigation.DurationInMonths,
                    TaxPercentage = d.IdNavigation.TaxPercentage
                });

                return Ok(depositsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Method to delete a deposit and respective asset
        [HttpDelete("delete/{depositId}")]
        public async Task<IActionResult> DeleteSelectedDeposit(int depositId)
        {
            try
            {
                var deposit = await _context.FixedDeposits
                    .Include(d => d.IdNavigation) // Include the associated Asset
                    .FirstOrDefaultAsync(d => d.Id == depositId);

                if (deposit != null)
                {
                    var assetId = deposit.IdNavigation.AssetId;

                    _context.FixedDeposits.Remove(deposit);
                    await _context.SaveChangesAsync();

                    var asset = await _context.Assets
                        .FirstOrDefaultAsync(a => a.AssetId == assetId);

                    _context.Assets.Remove(asset);
                    await _context.SaveChangesAsync();

                    return Ok("Deposit deleted successfully.");
                }
                else
                {
                    return NotFound("No deposit found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Method to update a deposit attributes
        [HttpPut("update/{depositId}")]
        public async Task<IActionResult> UpdateSelectedDeposit(int depositId,
            [FromQuery] decimal value,
            [FromQuery] string bank,
            [FromQuery] string accountNumber,
            [FromQuery] string accountHolders,
            [FromQuery] decimal annualInterestRate)
        {
            try
            {
                var deposit = await _context.FixedDeposits
                    .Include(d => d.IdNavigation)
                    .FirstOrDefaultAsync(d => d.Id == depositId);

                if (deposit != null)
                {
                    deposit.Value = value;
                    deposit.Bank = bank;
                    deposit.AccountNumber = accountNumber;
                    deposit.AccountHolders = accountHolders;
                    deposit.AnnualInterestRate = annualInterestRate;

                    await _context.SaveChangesAsync();

                    return Ok("Deposit updated successfully.");
                }
                else
                {
                    return NotFound("No deposit found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
