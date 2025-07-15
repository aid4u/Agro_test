using Agro_test.Models;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

namespace Agro_test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgroController : ControllerBase
    {
        private readonly IFieldsService _fieldsService;
        private readonly ICentroidsService _centroidsService;

        public AgroController(IFieldsService fieldsService,ICentroidsService centroidsService)
        {
            _fieldsService = fieldsService;
            _centroidsService = centroidsService;
        }

        /// <summary>
        /// Get all fields with centroids
        /// </summary>
        [HttpGet("fields")]
        public async Task<ActionResult<IEnumerable<HarvestField>>> GetAllFields()
        {
            var fields = await _fieldsService.GetAllAsync();
            var centroids = await _centroidsService.GetAllAsync();

            if (fields == null || centroids == null)
                return NotFound("Data not available");

            var result = fields
                .Join(centroids,
                    field => field.Id,
                    centroid => centroid.Id,
                    (field, centroid) => new HarvestField(field, centroid))
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get field size by ID
        /// </summary>
        [HttpGet("size/{id}")]
        public async Task<ActionResult<double>> GetFieldSize(int id)
        {
            var field = await _fieldsService.GetAsync(id);
            return field != null
                ? Ok(field.Size)
                : NotFound($"Field with ID {id} not found");
        }

        /// <summary>
        /// Calculate distance from point to field center
        /// </summary>
        [HttpGet("calcDistance/{lat:double}/{lng:double}/{id:int}")]
        public async Task<ActionResult<double>> CalculateDistance(
            double lat, double lng, int id)
        {
            var centroid = await _centroidsService.GetAsync(id);
            return centroid != null
                ? Ok(_centroidsService.CalculateDistanceInMeters(
                    new FieldPoint(lat, lng), centroid.Center))
                : NotFound($"Centroid for field ID {id} not found");
        }

        /// <summary>
        /// Get the fields if the coordinates are hit
        /// </summary>
        [HttpGet("matchFields/{lat:double}/{lng:double}")]
        public async Task<ActionResult<object>> GetFieldsByPoint(double lat, double lng)
        {
            var fields = await _fieldsService.GetAllAsync();
            if (fields == null)
                return Ok(false);

            var point = new Point(new Coordinate(lat, lng));
            var result = new List<FieldBase>();

            foreach (var field in fields)
            {
                var polygon = _fieldsService.CreatePolygon(field.Polygon);
                if (polygon != null && polygon.Covers(point))
                {
                    result.Add(new FieldBase(field.Id, field.Name));
                }
            }
            return result.Count > 0 ? Ok(result) : Ok(false);
        }


    }
}