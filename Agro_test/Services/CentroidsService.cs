using Agro_test.Models;
using System.Globalization;
using System.Xml;

namespace Agro_test.Services
{
    public class CentroidsService : ICentroidsService
    {
        private const string FileName = "Data/centroids.kml";
        private const double EarthRadius = 6371000; // Earth radius in meters
        private static readonly object _cacheLock = new();
        private static List<CentroidDTO>? _cachedCentroids;
        private readonly ILogger<CentroidsService> _logger;

        public CentroidsService(ILogger<CentroidsService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<CentroidDTO>> GetAllAsync()
        {
            if (_cachedCentroids != null)
                return _cachedCentroids;

            lock (_cacheLock)
            {
                if (_cachedCentroids != null)
                    return _cachedCentroids;

                try
                {
                    _cachedCentroids = ReadCentroids(FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading centroids file");
                    _cachedCentroids = new List<CentroidDTO>();
                }
            }
            return _cachedCentroids;
        }

        public async Task<CentroidDTO?> GetAsync(int id)
        {
            var centroids = await GetAllAsync();
            return centroids.FirstOrDefault(x => x.Id == id);
        }

        public double CalculateDistanceInMeters(FieldPoint coord1, FieldPoint coord2)
        {
            static double ToRadians(double angle) => angle * Math.PI / 180;

            var lat1 = ToRadians(coord1.Lat);
            var lon1 = ToRadians(coord1.Lng);
            var lat2 = ToRadians(coord2.Lat);
            var lon2 = ToRadians(coord2.Lng);

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Pow(Math.Sin(dLon / 2), 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        private List<CentroidDTO> ReadCentroids(string filePath)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);

            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

            return doc.SelectNodes("//kml:Placemark", nsManager)!
                .Cast<XmlNode>()
                .Select(ParseCentroid)
                .Where(centroid => centroid != null)
                .ToList()!;
        }

        private CentroidDTO? ParseCentroid(XmlNode placemark)
        {
            var nsManager = new XmlNamespaceManager(placemark.OwnerDocument.NameTable);
            nsManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

            var idNode = placemark.SelectSingleNode(".//kml:Data[@name='id']/kml:value", nsManager);
            var centerNode = placemark.SelectSingleNode(".//kml:Data[@name='center']/kml:value", nsManager);

            if (idNode == null || centerNode == null)
            {
                _logger.LogWarning("Missing required nodes in Placemark");
                return null;
            }

            if (!int.TryParse(idNode.InnerText, out int id))
            {
                _logger.LogWarning($"Invalid ID format: {idNode.InnerText}");
                return null;
            }

            var centerCoords = centerNode.InnerText.Trim().Split(',');
            if (centerCoords.Length < 2)
            {
                _logger.LogWarning($"Invalid coordinate format: {centerNode.InnerText}");
                return null;
            }

            if (!double.TryParse(centerCoords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double lng) ||
                !double.TryParse(centerCoords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
            {
                _logger.LogWarning($"Coordinate parsing failed: {centerNode.InnerText}");
                return null;
            }

            return new CentroidDTO(id, new FieldPoint(lat, lng));
        }
    }
}