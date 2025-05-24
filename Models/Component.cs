namespace WebBanMayTinh.Models
{
    public class Component
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Type { get; set; } // Loại linh kiện: CPU, GPU, RAM, Storage, v.v.
        public string? Socket { get; set; } // Cho CPU/Mainboard
        public string? RamType { get; set; } // Cho RAM/Mainboard
        public int PowerConsumption { get; set; } // Cho GPU/PSU

    }
}