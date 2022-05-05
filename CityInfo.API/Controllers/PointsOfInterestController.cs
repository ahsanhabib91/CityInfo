using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[Route("api/cities/{cityId}/[controller]")]
[ApiController]
public class PointsOfInterestController : ControllerBase
{

    public ILogger<PointsOfInterestController> _logger { get; }
    public IMailService _mailService { get; }
    private readonly ICityInfoRepository _cityInfoRepository;
    private readonly IMapper _mapper;


    public PointsOfInterestController(ILogger<PointsOfInterestController> logger, 
        IMailService mailService,
        ICityInfoRepository cityInfoRepository,
        IMapper mapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
    {
        try
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointsOfInterestForCity = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity));
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Exception while getting points of interest for city with Id {cityId}", ex);
            return StatusCode(500, "A problem happened while your request");
        }

    }

    [HttpGet("{pointofinterestId}", Name = nameof(GetPointOfInterest))]
    public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointofinterestId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }

        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestId);

        if (pointOfInterest == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
    }

    [HttpPost]
    public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }

        var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

        await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);

        await _cityInfoRepository.SaveChangesAsync();

        var createdPointOfInterestToReturn = _mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);

        return CreatedAtRoute(nameof(GetPointOfInterest),
                new
                {
                    cityId = cityId,
                    pointofinterestId = createdPointOfInterestToReturn.Id
                },
                createdPointOfInterestToReturn
            );
    }

    [HttpPut("{pointofinterestId}")]
    public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointofinterestId, PointOfInterestForUpdateDto pointOfInterest)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }

        var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestId);
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }

        _mapper.Map(pointOfInterest, pointOfInterestEntity);

        await _cityInfoRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{pointofinterestId}")]
    public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointofinterestId,
        JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }

        var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestId);
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }

        var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

        patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryValidateModel(pointOfInterestToPatch))
        {
            return BadRequest(ModelState);
        }

        _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

        await _cityInfoRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{pointofinterestId}")]
    public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointofinterestId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }

        var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestId);
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }

        _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
        await _cityInfoRepository.SaveChangesAsync();

        _mailService.Send(
            "Point of interest deleted.",
            $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

        return NoContent();
    }
}
