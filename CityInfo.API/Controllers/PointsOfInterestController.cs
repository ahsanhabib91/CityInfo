using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[Route("api/cities/{cityId}/[controller]")]
[ApiController]
public class PointsOfInterestController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
    {
        var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

        if (city == null)
            return NotFound();

        return Ok(city.PointsOfInterest);
    }

    [HttpGet("{pointofinterestId}", Name = nameof(GetPointOfInterest))]
    public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointofinterestId)
    {
        var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

        if (city == null)
            return NotFound();

        var pointOfInterest = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointofinterestId);
        
        if (pointOfInterest == null)
            return NotFound();

        return Ok(pointOfInterest);
    }

    [HttpPost]
    public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
    {
        var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }

        var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

        var finarPointOfInterest = new PointOfInterestDto()
        {
            Id = maxPointOfInterestId++,
            Name = pointOfInterest.Name,
            Description = pointOfInterest.Description,
        };

        city.PointsOfInterest.Add(finarPointOfInterest);
        
        return CreatedAtRoute(nameof(GetPointOfInterest), 
                new 
                { 
                    cityId = cityId, 
                    pointofinterestId = finarPointOfInterest.Id 
                }, 
                finarPointOfInterest
            );
    }

    [HttpPut("{pointofinterestId}")]
    public ActionResult UpdatePointOfInterest(int cityId, int pointofinterestId, PointOfInterestForUpdateDto pointOfInterest)
    {
        var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }

        var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointofinterestId);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }

        pointOfInterestFromStore.Name = pointOfInterest.Name;
        pointOfInterestFromStore.Description = pointOfInterest.Description;

        return NoContent();
    }

    [HttpPatch("{pointofinterestId}")]
    public ActionResult PartiallyUpdatePointOfInterest(int cityId, int pointofinterestId, 
        JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }

        var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointofinterestId);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }

        var pointOfInterestToPatch = 
            new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore?.Description,
            };

        patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryValidateModel(pointOfInterestToPatch))
        {
            return BadRequest(ModelState);
        }

        pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
        pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

        return NoContent();
    }

    [HttpDelete("{pointofinterestId}")]
    public ActionResult DeletePointOfInterest(int cityId, int pointofinterestId)
    {
        var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        if (city == null)
        {
            return NotFound();
        }

        var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointofinterestId);
        if (pointOfInterestFromStore == null)
        {
            return NotFound();
        }

        city.PointsOfInterest.Remove(pointOfInterestFromStore);

        return NoContent();
    }
}
