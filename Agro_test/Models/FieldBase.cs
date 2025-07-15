namespace Agro_test.Models
{
    public class FieldBase
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public FieldBase(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
