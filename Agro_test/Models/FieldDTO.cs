namespace Agro_test.Models
{
    public class FieldDTO : FieldBase
    {
        public double Size { get; set; }
        public List<FieldPoint> Polygon { get; set; }

        public FieldDTO(int id, string name, double size, List<FieldPoint> polygon) : base(id, name) 
        {
            //Id = id;
            //Name = name;
            Size = size;
            Polygon = polygon;
        }
    }
}
