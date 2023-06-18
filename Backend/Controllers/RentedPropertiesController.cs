using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Context;
using BusinessLogic.Entities;

namespace Backend.Controllers
{
    [Route("api/RentedProperties")]
    [ApiController]
    public class RentedPropertiesController : ControllerBase
    {
        private readonly ES2DbContext _context;

        public RentedPropertiesController(ES2DbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> HandleCreateRentedProperty(
            [FromQuery] string name,
            [FromQuery] string location,
            [FromQuery] decimal propertyValue,
            [FromQuery] decimal rentalValue,
            [FromQuery] decimal monthlyCondominiumFee,
            [FromQuery] decimal estimatedAnnualExpenses,
            [FromQuery] DateOnly startDate,
            [FromQuery] int durationInMonths,
            [FromQuery] decimal taxPercentage)
        {
            try
            {
                // Create an asset first
                var asset = new Asset
                {
                    UserId = UserSession.GetLoggedId(),
                    AssetType = "Rented Property",
                    StartDate = startDate,
                    DurationInMonths = durationInMonths,
                    TaxPercentage = taxPercentage
                };

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();

                // Get the most recently created Asset
                var assetId = asset.AssetId;
                var createdAsset = await _context.Assets.FindAsync(assetId);

                // Create the rented property associated with the asset
                var rentedProperty = new RentedProperty
                {
                    IdNavigation = createdAsset,
                    Name = name,
                    Location = location,
                    PropertyValue = propertyValue,
                    RentalValue = rentalValue,
                    MonthlyCondominiumFee = monthlyCondominiumFee,
                    EstimatedAnnualExpenses = estimatedAnnualExpenses
                };

                _context.RentedProperties.Add(rentedProperty);
                await _context.SaveChangesAsync();

                return Ok("Rented property created successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("get")]
        public async Task<IActionResult> GetRentedProperties()
        {
            try
            {
                var rentedProperties = await _context.RentedProperties
                    .Include(rp => rp.IdNavigation) // Include the associated Asset
                    .ToListAsync();

                var rentedPropertiesDTOs = rentedProperties.Select(rp => new
                {
                    RentedPropertyId = rp.Id, // Use RentedPropertyId instead of Id
                    Name = rp.Name,
                    Location = rp.Location,
                    PropertyValue = rp.PropertyValue,
                    RentalValue = rp.RentalValue,
                    MonthlyCondominiumFee = rp.MonthlyCondominiumFee,
                    EstimatedAnnualExpenses = rp.EstimatedAnnualExpenses,
                    StartDate = rp.IdNavigation.StartDate,
                    DurationInMonths = rp.IdNavigation.DurationInMonths,
                    TaxPercentage = rp.IdNavigation.TaxPercentage
                });

                return Ok(rentedPropertiesDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("get/{rentedPropertyId}")]
        public async Task<IActionResult> GetRentedPropertyById(int rentedPropertyId)
        {
            try
            {
                var rentedProperty = await _context.RentedProperties
                    .FirstOrDefaultAsync(rp => rp.Id == rentedPropertyId);

                if (rentedProperty != null)
                {
                    var rentedPropertyDTO = new
                    {
                        RentedPropertyId = rentedProperty.Id,
                        Name = rentedProperty.Name,
                        Location = rentedProperty.Location,
                        PropertyValue = rentedProperty.PropertyValue,
                        RentalValue = rentedProperty.RentalValue,
                        MonthlyCondominiumFee = rentedProperty.MonthlyCondominiumFee,
                        EstimatedAnnualExpenses = rentedProperty.EstimatedAnnualExpenses
                    };

                    return Ok(rentedPropertyDTO);
                }
                else
                {
                    return NotFound("No rented property found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListRentedProperties(int userId)
        {
            try
            {
                var rentedProperties = await _context.RentedProperties
                    .Include(rp => rp.IdNavigation) // Include the associated Asset
                    .Where(rp => rp.IdNavigation.UserId == userId) // Filter by userId
                    .ToListAsync();

                var rentedPropertiesDTOs = rentedProperties.Select(rp => new
                {
                    RentedPropertyId = rp.Id, // Include the RentedPropertyId field
                    Name = rp.Name,
                    Location = rp.Location,
                    PropertyValue = rp.PropertyValue,
                    RentalValue = rp.RentalValue,
                    MonthlyCondominiumFee = rp.MonthlyCondominiumFee,
                    EstimatedAnnualExpenses = rp.EstimatedAnnualExpenses,
                    StartDate = rp.IdNavigation.StartDate,
                    DurationInMonths = rp.IdNavigation.DurationInMonths,
                    TaxPercentage = rp.IdNavigation.TaxPercentage
                });

                return Ok(rentedPropertiesDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("delete/{propertyId}")]
        public async Task<IActionResult> DeleteSelectedProperty(int propertyId)
        {
            try
            {
                var rentedProperty = await _context.RentedProperties
                    .Include(rp => rp.IdNavigation) // Include the associated Asset
                    .FirstOrDefaultAsync(rp => rp.Id == propertyId);

                if (rentedProperty != null)
                {
                    var assetId = rentedProperty.IdNavigation.AssetId;

                    _context.RentedProperties.Remove(rentedProperty);
                    await _context.SaveChangesAsync();

                    var asset = await _context.Assets
                        .FirstOrDefaultAsync(a => a.AssetId == assetId);

                    _context.Assets.Remove(asset);
                    await _context.SaveChangesAsync();

                    return Ok("Property deleted successfully.");
                }
                else
                {
                    return NotFound("No property found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

         [HttpPut("update/{propertyId}")]
        public async Task<IActionResult> UpdateSelectedProperty(int propertyId,
            [FromQuery] string name,
            [FromQuery] string location,
            [FromQuery] decimal propertyValue,
            [FromQuery] decimal rentalValue,
            [FromQuery] decimal monthlyCondominiumFee,
            [FromQuery] decimal estimatedAnnualExpenses,
            [FromQuery] DateOnly startDate,
            [FromQuery] int durationInMonths,
            [FromQuery] decimal taxPercentage)
        {
            try
            {
                var rentedProperty = await _context.RentedProperties
                    .Include(rp => rp.IdNavigation) // Include the associated Asset
                    .FirstOrDefaultAsync(rp => rp.Id == propertyId);

                if (rentedProperty != null)
                {
                    // Update rented property attributes
                    rentedProperty.Name = name;
                    rentedProperty.Location = location;
                    rentedProperty.PropertyValue = propertyValue;
                    rentedProperty.RentalValue = rentalValue;
                    rentedProperty.MonthlyCondominiumFee = monthlyCondominiumFee;
                    rentedProperty.EstimatedAnnualExpenses = estimatedAnnualExpenses;

                    // Update associated asset attributes
                    rentedProperty.IdNavigation.StartDate = startDate;
                    rentedProperty.IdNavigation.DurationInMonths = durationInMonths;
                    rentedProperty.IdNavigation.TaxPercentage = taxPercentage;

                    await _context.SaveChangesAsync();

                    return Ok("Property updated successfully.");
                }
                else
                {
                    return NotFound("No property found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
