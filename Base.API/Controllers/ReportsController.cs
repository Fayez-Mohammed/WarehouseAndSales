using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryProject.Specifications;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin,Accountant,StoreManager")] // الصلاحيات: محاسب، أدمن، مدير مخزن
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public ReportsController(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        /// <summary>
        /// تقرير المبيعات خلال فترة زمنية محددة
        /// </summary>
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            
            var start = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var end = toDate ?? DateTime.UtcNow;

            var spec = new BaseSpecification<Order>(o =>
                o.Status == OrderStatus.Approved &&
                o.ApprovedDate >= start &&
                o.ApprovedDate <= end);

            var orders = await _unitOfWork.Repository<Order>().ListAsync(spec);

            var report = new SalesReportDto
            {
                FromDate = start,
                ToDate = end,
                TotalOrders = orders.Count,
                TotalSales = orders.Sum(o => o.TotalAmount)
            };

            return Ok(report);
        }

        /// <summary>
        /// تقرير حركة صنف معين (Stock Movement)
        /// </summary>
        [HttpGet("stock-movement")]
        public async Task<IActionResult> GetStockMovementReport([FromQuery]string producName)
        {
            var specProduct = new BaseSpecification<Product>(p => p.Name == producName);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(specProduct);
            if (product == null) return NotFound("Product not found");

            
            var spec = new BaseSpecification<StockTransaction>(t => t.ProductId == product.Id);
            var transactions = await _unitOfWork.Repository<StockTransaction>().ListAsync(spec);

            var report = new StockMovementReportDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CurrentStock = product.CurrentStockQuantity,

                TotalIn = transactions
                            .Where(t => t.Type == TransactionType.StockIn || t.Type == TransactionType.Return || (t.Type == TransactionType.Adjustment && t.Quantity > 0)|| t.Type == TransactionType.UpdatedInByEmployee)
                            .Sum(t => Math.Abs(t.Quantity)),
                TotalInAdjusted = transactions
                            .Where(t => t.Type == TransactionType.Adjustment && t.Quantity > 0)
                            .Sum(t => t.Quantity),
                TotalInPurchased = transactions
                            .Where(t => t.Type == TransactionType.StockIn)
                            .Sum(t => t.Quantity),
                TotalInReturned = transactions
                            .Where(t => t.Type == TransactionType.Return)
                            .Sum(t => t.Quantity),
                TotalOut = transactions
                            .Where(t => t.Type == TransactionType.StockOut || (t.Type == TransactionType.Adjustment && t.Quantity < 0)|| t.Type == TransactionType.UpdatedOutByEmployee)
                            .Sum(t => Math.Abs(t.Quantity)),
                TotalOutAdjusted = transactions
                            .Where(t => t.Type == TransactionType.Adjustment && t.Quantity < 0)
                            .Sum(t => Math.Abs(t.Quantity)),
                TotalOutSold = transactions
                            .Where(t => t.Type == TransactionType.StockOut)
                            .Sum(t => Math.Abs(t.Quantity)),
                TotalInUpdatedByEmployee = transactions
                 .Where(t => t.Type == TransactionType.UpdatedInByEmployee)
                            .Sum(t => Math.Abs(t.Quantity)),
                TotalOutUpdatedByEmployee = transactions
                 .Where(t => t.Type == TransactionType.UpdatedOutByEmployee)
                            .Sum(t => Math.Abs(t.Quantity))
               

            };

            return Ok(report);
        }

        /// <summary>
        /// تقرير عمولات المندوبين (Commissions)
        /// </summary>
        [HttpGet("commissions")]
        public async Task<IActionResult> GetCommissionsReport([FromQuery] string? salesRepName)
        {
 
            var spec = new BaseSpecification<Invoice>(i => i.Type == InvoiceType.CommissionInvoice);

         
            spec.Includes.Add(i => i.Order);
            spec.Includes.Add(i => i.Order.SalesRep); 

            var invoices = await _unitOfWork.Repository<Invoice>().ListAsync(spec);


            if (!string.IsNullOrEmpty(salesRepName))
            {
                invoices = invoices.Where(i => i.Order.SalesRep.FullName.Contains(salesRepName)).ToList();
            }


            var report = invoices
                .GroupBy(i => i.Order.SalesRepId)
                .Select(g => new CommissionReportDto
                {
                    SalesRepId = g.Key,
                    SalesRepName = g.First().Order.SalesRep?.FullName ?? g.First().RecipientName,
                    TotalCommission = g.Sum(i => i.Amount),
                    TotalOrdersConfirmed = g.Count()
                })
                .ToList();

            return Ok(report);
        }


        /// <summary>
        /// تقرير شامل عن عميل معين بالاسم
        /// </summary>
        [HttpGet("customer-report")]
        public async Task<IActionResult> GetCustomerReport([FromQuery] string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                return BadRequest("Customer name is required.");

            var customers = await _userManager.Users
                .Where(u => u.FullName.Contains(customerName) && u.Type == UserTypes.Customer)
                .ToListAsync();

            if (!customers.Any())
                return NotFound($"No customer found with name containing '{customerName}'.");

            if (customers.Count > 1)
                return BadRequest($"Multiple customers found with name '{customerName}'. Please be more specific.");

            var customer = customers.First();

         
          
            var orderSpec = new BaseSpecification<Order>(o =>
                o.CustomerId == customer.Id &&
                o.Status == OrderStatus.Approved); 

            var orders = await _unitOfWork.Repository<Order>().ListAsync(orderSpec);
            var invoiceSpec = new BaseSpecification<Invoice>(i =>
        i.Order.CustomerId == customer.Id &&
        (i.Type == InvoiceType.CustomerInvoice || i.Type == InvoiceType.ReturnInvoice)); 

            var allInvoices = await _unitOfWork.Repository<Invoice>().ListAsync(invoiceSpec);

           
            var salesInvoices = allInvoices.Where(i => i.Type == InvoiceType.CustomerInvoice).ToList();
            var returnInvoices = allInvoices.Where(i => i.Type == InvoiceType.ReturnInvoice).ToList();

            var unpaidInvoicesList = salesInvoices
        .Where(i => i.RemainingAmount > 0) // هات اللي المتبقي فيه أكبر من صفر
        .OrderBy(i => i.DateOfCreation)    // رتبهم من الأقدم للأحدث (عشان المحاسب يعرف القديم)
        .Select(i => new UnpaidInvoiceSummaryDto
        {
           
            InvoiceCode = i.Code, // Assuming BaseEntity has Code
            OriginalAmount = i.Amount,
            RemainingAmount = i.RemainingAmount,
            InvoiceDate = i.DateOfCreation
        })
        .ToList();

            var unpaidReturnsList = returnInvoices
        .Where(i => i.RemainingAmount > 0)
        .OrderBy(i => i.DateOfCreation)
        .Select(i => new UnpaidInvoiceSummaryDto
        {
           
            InvoiceCode = i.Code,
            OriginalAmount = i.Amount,
            RemainingAmount = i.RemainingAmount, // المبلغ المتبقي للعميل (رصيد)
            InvoiceDate = i.DateOfCreation
        })
        .ToList();
            var report = new CustomerReportDto
            {
                UserNumber = customer.UserNumber,
                CustomerName = customer.FullName,
                PhoneNumber = customer.PhoneNumber??"لم يتم ادخالو",

                TotalReturnsCount = returnInvoices.Count,
                TotalReturnsAmount = returnInvoices.Sum(i => i.Amount), 

                TotalOrdersCount = orders.Count,
                TotalSalesAmount = orders.Sum(o => o.TotalAmount),
                LastOrderDate = orders.OrderByDescending(o => o.DateOfCreation).FirstOrDefault()?.DateOfCreation,

            
                TotalPaid = salesInvoices.Sum(i => i.PaidAmount),
                TotalDebt = salesInvoices.Sum(i => i.RemainingAmount),

                TotalNetDebt = salesInvoices.Sum(i => i.RemainingAmount) - returnInvoices.Sum(i => i.RemainingAmount),

               
                UnpaidInvoices = unpaidInvoicesList,
                UnpaidReturns = unpaidReturnsList


            };

            return Ok(report);
        }


        /// <summary>
        /// تقرير شامل عن مندوب مبيعات معين
        /// </summary>
        [HttpGet("sales-rep-report")]
        public async Task<IActionResult> GetSalesRepReport([FromQuery] string salesRepName)
        {
            if (string.IsNullOrWhiteSpace(salesRepName))
                return BadRequest("Sales Rep name is required.");

            // 1. البحث عن المندوب
            var salesReps = await _userManager.Users
                .Where(u => u.FullName.Contains(salesRepName) && u.Type == UserTypes.SalesRep)
                .ToListAsync();

            if (!salesReps.Any())
                return NotFound($"No Sales Rep found with name containing '{salesRepName}'.");

            if (salesReps.Count > 1)
                return BadRequest($"Multiple Sales Reps found with name '{salesRepName}'. Please be more specific.");

            var salesRep = salesReps.First();

            // 2. جلب الطلبات اللي هو عملها (عشان نحسب حجم المبيعات)
            var orderSpec = new BaseSpecification<Order>(o =>
                o.SalesRepId == salesRep.Id &&
                o.Status == OrderStatus.Approved);

            var orders = await _unitOfWork.Repository<Order>().ListAsync(orderSpec);

            // 3. جلب فواتير العمولات (فلوسه)
            var commissionSpec = new BaseSpecification<Invoice>(i =>
                i.Order.SalesRepId == salesRep.Id &&
                i.Type == InvoiceType.CommissionInvoice); // نوع فاتورة العمولة

            var commissionInvoices = await _unitOfWork.Repository<Invoice>().ListAsync(commissionSpec);

            // 4. جلب فواتير المرتجعات الخاصة بطلبات هذا المندوب (عشان نعرف المرتجعات اللي عليه)
            var returnSpec = new BaseSpecification<Invoice>(i =>
                i.Order.SalesRepId == salesRep.Id &&
                i.Type == InvoiceType.ReturnInvoice); // مرتجعات العملاء بتوعه

            var returnInvoices = await _unitOfWork.Repository<Invoice>().ListAsync(returnSpec);

            // 5. تجهيز قائمة العمولات غير المدفوعة (فلوس ليه عند الشركة)
            var unpaidCommissionsList = commissionInvoices
                .Where(i => i.RemainingAmount > 0)
                .OrderBy(i => i.DateOfCreation)
                .Select(i => new UnpaidInvoiceSummaryDto
                {
                    InvoiceCode = i.Code,
                    OriginalAmount = i.Amount,
                    RemainingAmount = i.RemainingAmount, // المبلغ اللي لسه مقبضهوش
                    InvoiceDate = i.DateOfCreation
                })
                .ToList();

            // 6. تجميع التقرير
            var report = new SalesRepReportDto
            {
                UserNumber = salesRep.UserNumber,
                SalesRepName = salesRep.FullName,
                PhoneNumber = salesRep.PhoneNumber ?? "لا يوجد رقم هاتف",

                // أداء البيع
                TotalOrdersCount = orders.Count,
                TotalSalesVolume = orders.Sum(o => o.TotalAmount), // باع بضاعة بكام

                // أداء المرتجعات (العملاء بتوعه رجعوا حاجات قد ايه)
                TotalReturnsCount = returnInvoices.Count,
                TotalReturnsVolume = returnInvoices.Sum(i => i.Amount),

                // مستحقاته المالية
                TotalCommissionEarned = commissionInvoices.Sum(i => i.Amount),      // إجمالي عمولاته
                TotalCommissionPaid = commissionInvoices.Sum(i => i.PaidAmount),    // اللي قبضه
                TotalCommissionDue = commissionInvoices.Sum(i => i.RemainingAmount), // اللي لسه ليه

                // القائمة التفصيلية
                UnpaidCommissions = unpaidCommissionsList
            };

            return Ok(report);
        }

        /// <summary>
        /// تقرير شامل عن مورد معين
        /// </summary>
        [HttpGet("supplier-report")]
        public async Task<IActionResult> GetSupplierReport([FromQuery] string supplierName)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                return BadRequest("Supplier name is required.");

            // 1. البحث عن المورد في جدول المستخدمين
            var users = await _userManager.Users
                .Where(u => u.FullName.Contains(supplierName) && u.Type == UserTypes.Supplier)
                .ToListAsync();

            if (!users.Any())
                return NotFound($"No supplier found with name containing '{supplierName}'.");

            if (users.Count > 1)
                return BadRequest($"Multiple suppliers found with name '{supplierName}'. Please be more specific.");

            var user = users.First();

            // 2. جلب كيان المورد (Supplier Entity) عشان الـ ID بتاعه مربوط بالفواتير
            var supplierSpec = new BaseSpecification<Supplier>(s => s.UserId == user.Id);
            var supplier = await _unitOfWork.Repository<Supplier>().GetEntityWithSpecAsync(supplierSpec);

            if (supplier == null)
                return NotFound("Supplier profile not found.");

            // 3. جلب فواتير المورد (توريد + مرتجع)
            // لاحظ: بنستخدم Repository<SupplierInvoice> مش Invoice العادية
            var invoiceSpec = new BaseSpecification<SupplierInvoice>(i =>
                i.SupplierId == supplier.Id);

            var allInvoices = await _unitOfWork.Repository<SupplierInvoice>().ListAsync(invoiceSpec);

            // 4. فصل الفواتير
            var supplyInvoices = allInvoices.Where(i => i.Type == InvoiceType.SupplierInvoice).ToList();
            var returnInvoices = allInvoices.Where(i => i.Type == InvoiceType.SupplierReturnInvoice).ToList();

            // 5. القوائم التفصيلية (للمبالغ المتبقية)
            var unpaidSupplyList = supplyInvoices
                .Where(i => i.RemainingAmount > 0)
                .OrderBy(i => i.DateOfCreation)
                .Select(i => new UnpaidInvoiceSummaryDto
                {
                    InvoiceCode = i.Code,
                    OriginalAmount = i.Amount,
                    RemainingAmount = i.RemainingAmount, // المبلغ اللي لسه علينا
                    InvoiceDate = i.DateOfCreation
                })
                .ToList();

            var unpaidReturnsList = returnInvoices
                .Where(i => i.RemainingAmount > 0)
                .OrderBy(i => i.DateOfCreation)
                .Select(i => new UnpaidInvoiceSummaryDto
                {
                    InvoiceCode = i.Code,
                    OriginalAmount = i.Amount,
                    RemainingAmount = i.RemainingAmount, // رصيدنا المتبقي عند المورد
                    InvoiceDate = i.DateOfCreation
                })
                .ToList();

            // 6. تجميع التقرير
            var report = new SupplierReportDto
            {
                UserNumber = user.UserNumber,
                SupplierName = supplier.Name,
                PhoneNumber = user.PhoneNumber ??"لا يوجد رقم", // بنحاول نجيب الرقم من أي مكان

                // إحصائيات التوريد
                TotalSupplyCount = supplyInvoices.Count,
                TotalSupplyAmount = supplyInvoices.Sum(i => i.Amount),

                // إحصائيات المرتجع
                TotalReturnsCount = returnInvoices.Count,
                TotalReturnsAmount = returnInvoices.Sum(i => i.Amount),

                // الموقف المالي
                TotalPaid = supplyInvoices.Sum(i => i.PaidAmount), // إجمالي اللي دفعناه للمورد
                TotalDebt = supplyInvoices.Sum(i => i.RemainingAmount), // إجمالي الديون الحالية

                // صافي المديونية = (اللي علينا) - (رصيد المرتجعات اللي لسه ماتخصمش)
                TotalNetDebt = supplyInvoices.Sum(i => i.RemainingAmount) - returnInvoices.Sum(i => i.RemainingAmount),

                // القوائم
                UnpaidInvoices = unpaidSupplyList,
                UnpaidReturns = unpaidReturnsList
            };

            return Ok(report);
        }
    }
}
