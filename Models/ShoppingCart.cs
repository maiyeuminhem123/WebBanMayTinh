namespace WebBanMayTinh.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Thêm sản phẩm vào giỏ
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        // Xóa sản phẩm khỏi giỏ
        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

        // Cập nhật số lượng sản phẩm
        public void UpdateQuantity(int productId, int newQuantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = newQuantity;
            }
        }

        // Tổng số sản phẩm trong giỏ
        public int TotalItems()
        {
            return Items.Sum(i => i.Quantity);
        }

        // Tổng tiền
        public decimal TotalAmount()
        {
            return Items.Sum(i => i.TotalPrice);
        }

        // Xóa toàn bộ giỏ
        public void Clear()
        {
            Items.Clear();
        }
    }

}
