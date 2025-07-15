namespace Agro_test.Models
{
    public class Location
    {
        public FieldPoint Center { get; set; }
        public List<FieldPoint> Polygon { get; set; }

        public Location(FieldPoint center, List<FieldPoint> polygon)
        {
            Center = center;
            Polygon = polygon;
        }
    }
}
