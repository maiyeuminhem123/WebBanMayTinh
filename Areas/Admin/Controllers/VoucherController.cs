using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VoucherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VoucherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Voucher
        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.Vouchers.ToListAsync();
            return View(vouchers);
        }

        // GET: Admin/Voucher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null) return NotFound();

            return View(voucher);
        }

        // GET: Admin/Voucher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Voucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            ValidateVoucher(voucher);

            if (ModelState.IsValid)
            {
                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // GET: Admin/Voucher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            return View(voucher);
        }

        // POST: Admin/Voucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voucher voucher)
        {
            if (id != voucher.Id) return NotFound();

            ValidateVoucher(voucher);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // GET: Admin/Voucher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null) return NotFound();

            return View(voucher);
        }

        // POST: Admin/Voucher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(v => v.Id == id);
        }
        /// </summary>
        private void ValidateVoucher(Voucher voucher)
        {
            if (voucher == null)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu voucher không hợp lệ.");
                return;
            }

            // Kiểm tra mã voucher trống
            if (string.IsNullOrWhiteSpace(voucher.Code))
            {
                ModelState.AddModelError(nameof(voucher.Code), "Mã voucher không được để trống.");
            }
            else
            {
                // Kiểm tra trùng mã khi tạo mới
                bool isDuplicateCode = _context.Vouchers
                    .Any(v => v.Code == voucher.Code && v.Id != voucher.Id);
                if (isDuplicateCode)
                {
                    ModelState.AddModelError(nameof(voucher.Code), "Mã voucher đã tồn tại.");
                }
            }

            // Ít nhất phải có 1 giá trị giảm giá > 0
            if ((voucher.DiscountAmount <= 0) && (voucher.DiscountPercent <= 0))
            {
                ModelState.AddModelError(string.Empty, "Phải nhập số tiền giảm hoặc % giảm lớn hơn 0.");
            }

            // MinOrderValue nếu có thì phải >= 0
            if (voucher.MinOrderValue.HasValue && voucher.MinOrderValue < 0)
            {
                ModelState.AddModelError(nameof(voucher.MinOrderValue), "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0.");
            }

            // Ngày hết hạn (ExpiryDate) nên lớn hơn hoặc bằng ngày hiện tại nếu được nhập
            if (voucher.ExpiryDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(voucher.ExpiryDate), "Ngày hết hạn không được nhỏ hơn ngày hiện tại.");
            }
        }
    }
}
