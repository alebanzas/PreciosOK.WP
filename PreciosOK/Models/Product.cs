namespace PreciosOK.Models
{
    public class Product
    {
        public long Id { get; set; }
        public int Region { get; set; }
        public int Market { get; set; }
        public double Price { get; set; }
        public string Provider { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public long BarCode { get; set; }
        public string Name { get; set; }
    }
}