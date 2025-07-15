using Agro_test.Models;

public interface ICentroidsService
{
    Task<IEnumerable<CentroidDTO>> GetAllAsync();
    Task<CentroidDTO?> GetAsync(int id);
    double CalculateDistanceInMeters(FieldPoint coord1, FieldPoint coord2);
}