using Agro_test.Models;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Globalization;
using System.Xml;

namespace Agro_test.Services
{

    public class FieldsService : IFieldsService
    {
        private const string FileName = "Data/fields.kml";
        private static readonly object _cacheLock = new();
        private static List<FieldDTO>? _cachedFields;
        private readonly ILogger<FieldsService> _logger;

        public FieldsService(ILogger<FieldsService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<FieldDTO>> GetAllAsync()
        {
            if (_cachedFields != null)
                return _cachedFields;

            lock (_cacheLock)
            {
                if (_cachedFields != null)
                    return _cachedFields;

                try
                {
                    _cachedFields = ReadFields(FileName);
                    _logger.LogInformation($"Loaded {_cachedFields.Count} fields from cache");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading fields file");
                    _cachedFields = new List<FieldDTO>();
                }
            }
            return _cachedFields;
        }

        public async Task<FieldDTO?> GetAsync(int id)
        {
            var fields = await GetAllAsync();
            return fields.FirstOrDefault(x => x.Id == id);
        }

        public Polygon CreatePolygon(IEnumerable<FieldPoint> coordinates)
        {
            var ntsCoordinates = coordinates
                .Select(c => new Coordinate(c.Lng, c.Lat))
                .ToList();

            if (ntsCoordinates.Count == 0)
                throw new ArgumentException("Coordinates collection is empty");

            // Ensure polygon is closed
            if (!ntsCoordinates.First().Equals(ntsCoordinates.Last()))
                ntsCoordinates.Add(ntsCoordinates.First());

            return new Polygon(new LinearRing(ntsCoordinates.ToArray()));
        }

        private List<FieldDTO> ReadFields(string filePath)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);

            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

            return doc.SelectNodes("//kml:Placemark", nsManager)!
                .Cast<XmlNode>()
                .Select(ParseField)
                .Where(field => field != null)
                .ToList()!;
        }

        private FieldDTO? ParseField(XmlNode placemark)
        {
            var nsManager = new XmlNamespaceManager(placemark.OwnerDocument.NameTable);
            nsManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

            var idNode = placemark.SelectSingleNode(".//kml:Data[@name='id']/kml:value", nsManager);
            var nameNode = placemark.SelectSingleNode("./kml:name", nsManager);
            var sizeNode = placemark.SelectSingleNode(".//kml:Data[@name='size']/kml:value", nsManager);
            var coordNode = placemark.SelectSingleNode(".//kml:coordinates", nsManager);

            if (idNode == null || coordNode == null)
            {
                _logger.LogWarning("Missing required nodes in Placemark");
                return null;
            }

            if (!int.TryParse(idNode.InnerText, out int id))
            {
                _logger.LogWarning($"Invalid ID format: {idNode.InnerText}");
                return null;
            }

            double size = 0;
            if (sizeNode != null && !double.TryParse(
                sizeNode.InnerText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out size))
            {
                _logger.LogWarning($"Invalid size format: {sizeNode.InnerText}");
            }

            var polygon = ParseCoordinates(coordNode.InnerText);
            if (!polygon.Any())
            {
                _logger.LogWarning($"No valid coordinates for field ID {id}");
                return null;
            }

            return new FieldDTO(
                id: id,
                name: nameNode?.InnerText ?? $"Field {id}",
                size: size,
                polygon: polygon
            );
        }

        private List<FieldPoint> ParseCoordinates(string coordString)
        {
            var points = new List<FieldPoint>();
            var separators = new[] { ' ', '\n', '\t', '\r' };
            var tokens = coordString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                var trimmedToken = token.Trim();
                if (string.IsNullOrWhiteSpace(trimmedToken))
                    continue;

                var coords = trimmedToken.Split(',');
                if (coords.Length < 2)
                    continue;

                if (double.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double lng) &&
                    double.TryParse(coords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
                {
                    points.Add(new FieldPoint(lat, lng));
                }
            }
            return points;
        }
    }
}