using Agro_test.Models;
using NetTopologySuite.Geometries;

public interface IFieldsService
{
    Task<IEnumerable<FieldDTO>> GetAllAsync();
    Task<FieldDTO?> GetAsync(int id);
    Polygon CreatePolygon(IEnumerable<FieldPoint> coordinates);
}