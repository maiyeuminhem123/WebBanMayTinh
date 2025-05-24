using System.Collections.Generic;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Data
{
    public static class ComponentData
    {
        public static List<WebBanMayTinh.Models.Component> GetComponents()
        {
            return new List<WebBanMayTinh.Models.Component>
            {
                // CPU
                new WebBanMayTinh.Models.Component { Id = 1, Name = "Intel Core i5-12400F", Price = 3500000, Type = "CPU" },
                new WebBanMayTinh.Models.Component { Id = 2, Name = "Intel Core i7-12700K", Price = 7500000, Type = "CPU" },
                new WebBanMayTinh.Models.Component { Id = 3, Name = "Intel Core i9-12900K", Price = 12000000, Type = "CPU" },
                new WebBanMayTinh.Models.Component { Id = 4, Name = "AMD Ryzen 5 5600X", Price = 4000000, Type = "CPU" },
                new WebBanMayTinh.Models.Component { Id = 5, Name = "AMD Ryzen 7 5800X", Price = 8000000, Type = "CPU" },
                new WebBanMayTinh.Models.Component { Id = 6, Name = "AMD Ryzen 9 5900X", Price = 11000000, Type = "CPU" },

                // GPU
                new WebBanMayTinh.Models.Component { Id = 7, Name = "NVIDIA RTX 3060", Price = 8000000, Type = "GPU" },
                new WebBanMayTinh.Models.Component { Id = 8, Name = "NVIDIA RTX 4070", Price = 15000000, Type = "GPU" },
                new WebBanMayTinh.Models.Component { Id = 9, Name = "NVIDIA RTX 4090", Price = 35000000, Type = "GPU" },
                new WebBanMayTinh.Models.Component { Id = 10, Name = "AMD RX 6700 XT", Price = 9000000, Type = "GPU" },
                new WebBanMayTinh.Models.Component { Id = 11, Name = "AMD RX 6800 XT", Price = 12000000, Type = "GPU" },
                new WebBanMayTinh.Models.Component { Id = 12, Name = "AMD RX 7900 XT", Price = 20000000, Type = "GPU" },

                // RAM
                new WebBanMayTinh.Models.Component { Id = 13, Name = "8GB DDR4 3200MHz", Price = 700000, Type = "RAM" },
                new WebBanMayTinh.Models.Component { Id = 14, Name = "16GB DDR4 3200MHz", Price = 1200000, Type = "RAM" },
                new WebBanMayTinh.Models.Component { Id = 15, Name = "32GB DDR4 3600MHz", Price = 2500000, Type = "RAM" },
                new WebBanMayTinh.Models.Component { Id = 16, Name = "16GB DDR5 5200MHz", Price = 2000000, Type = "RAM" },
                new WebBanMayTinh.Models.Component { Id = 17, Name = "32GB DDR5 6000MHz", Price = 4000000, Type = "RAM" },

                // Storage
                new WebBanMayTinh.Models.Component { Id = 18, Name = "500GB SSD NVMe", Price = 1000000, Type = "Storage" },
                new WebBanMayTinh.Models.Component { Id = 19, Name = "1TB SSD NVMe", Price = 2000000, Type = "Storage" },
                new WebBanMayTinh.Models.Component { Id = 20, Name = "2TB SSD NVMe", Price = 4000000, Type = "Storage" },
                new WebBanMayTinh.Models.Component { Id = 21, Name = "1TB HDD 7200RPM", Price = 800000, Type = "Storage" },
                new WebBanMayTinh.Models.Component { Id = 22, Name = "2TB HDD 7200RPM", Price = 1500000, Type = "Storage" },

                // Mainboard
                new WebBanMayTinh.Models.Component { Id = 23, Name = "ASUS ROG Strix B550-F", Price = 3500000, Type = "Mainboard" },
                new WebBanMayTinh.Models.Component { Id = 24, Name = "MSI B660 Tomahawk", Price = 4000000, Type = "Mainboard" },
                new WebBanMayTinh.Models.Component { Id = 25, Name = "Gigabyte Z690 Aorus Elite", Price = 5500000, Type = "Mainboard" },
                new WebBanMayTinh.Models.Component { Id = 26, Name = "ASRock B450 Steel Legend", Price = 2500000, Type = "Mainboard" },

                // PSU (Nguồn)
                new WebBanMayTinh.Models.Component { Id = 27, Name = "Cooler Master 550W 80+ Bronze", Price = 1200000, Type = "PSU" },
                new WebBanMayTinh.Models.Component { Id = 28, Name = "Corsair 750W 80+ Gold", Price = 2500000, Type = "PSU" },
                new WebBanMayTinh.Models.Component { Id = 29, Name = "Seasonic 850W 80+ Platinum", Price = 3500000, Type = "PSU" },
                new WebBanMayTinh.Models.Component { Id = 30, Name = "EVGA 650W 80+ Gold", Price = 2000000, Type = "PSU" },

                // Case (Vỏ máy)
                new WebBanMayTinh.Models.Component { Id = 31, Name = "NZXT H510", Price = 1500000, Type = "Case" },
                new WebBanMayTinh.Models.Component { Id = 32, Name = "Lian Li PC-O11 Dynamic", Price = 3000000, Type = "Case" },
                new WebBanMayTinh.Models.Component { Id = 33, Name = "Corsair 4000D Airflow", Price = 2500000, Type = "Case" },
                new WebBanMayTinh.Models.Component { Id = 34, Name = "Cooler Master MasterBox Q300L", Price = 1000000, Type = "Case" },

                // Cooling (Tản nhiệt)
                new WebBanMayTinh.Models.Component { Id = 35, Name = "Cooler Master Hyper 212", Price = 800000, Type = "Cooling" },
                new WebBanMayTinh.Models.Component { Id = 36, Name = "NZXT Kraken X63 AIO", Price = 3500000, Type = "Cooling" },
                new WebBanMayTinh.Models.Component { Id = 37, Name = "Noctua NH-U12S", Price = 1500000, Type = "Cooling" },
                new WebBanMayTinh.Models.Component { Id = 38, Name = "Corsair H100i RGB Platinum", Price = 4000000, Type = "Cooling" }
            };
        }
    }
}