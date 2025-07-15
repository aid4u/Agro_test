namespace Agro_test.Models
{
    public class HarvestField : FieldBase
    {
        public double Size { get; set; }
        public Location Location { get; set; }

        public HarvestField(int id, string name, double size, Location location) : base(id, name) 
        {
            Size = size;
            Location = location;
        }

        public HarvestField(FieldDTO field, CentroidDTO centroid) : base(field.Id, field.Name) 
        {
            Id = field.Id;
            Name = field.Name;
            Size = field.Size;
            Location = new(centroid.Center, field.Polygon);
        }

    }
}
