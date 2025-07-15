namespace Agro_test.Models
{
    public class CentroidDTO
    {
        public int Id { get; set; }
        public FieldPoint Center { get; set; }

        public CentroidDTO(int id, FieldPoint center)
        {
            Id = id;
            Center = center;
        }
    }

}
