namespace RayTracing.Web.Models
{
    public class NameIdModel<T>
    {
        public string Name { get; set; }

        public T Id { get; set; }
    }

    public class NameIdModel : NameIdModel<int>
    {
    }
}
