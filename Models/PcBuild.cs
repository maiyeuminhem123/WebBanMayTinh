namespace WebBanMayTinh.Models
{
    public class PcBuild
    {
        public WebBanMayTinh.Models.Component Cpu { get; set; }
        public WebBanMayTinh.Models.Component Gpu { get; set; }
        public WebBanMayTinh.Models.Component Ram { get; set; }
        public WebBanMayTinh.Models.Component Storage { get; set; }
        public WebBanMayTinh.Models.Component Mainboard { get; set; }
        public WebBanMayTinh.Models.Component Psu { get; set; }
        public WebBanMayTinh.Models.Component Case { get; set; }
        public WebBanMayTinh.Models.Component Cooling { get; set; }
        public decimal TotalPrice => (Cpu?.Price ?? 0) + (Gpu?.Price ?? 0) + (Ram?.Price ?? 0) +
                                     (Storage?.Price ?? 0) + (Mainboard?.Price ?? 0) +
                                     (Psu?.Price ?? 0) + (Case?.Price ?? 0) + (Cooling?.Price ?? 0);
    }
}