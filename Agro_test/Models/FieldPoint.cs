namespace Agro_test.Models
{
    public struct FieldPoint
    {
        public double Lat { get; set; } // широта
        public double Lng { get; set; } // долгота

        public FieldPoint(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }
    }
}
