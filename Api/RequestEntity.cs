namespace Api
{
    public class RequestEntity
    {
        public int mv_id { get; set; }
        public required string action { get; set; }
        public bool cache { get; set; }
    }
}
