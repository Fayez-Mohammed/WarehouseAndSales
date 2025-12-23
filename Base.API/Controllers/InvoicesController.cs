using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
using Base.Shared.DTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RepositoryProject.Specifications;
using System.Threading.Tasks;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin,Accountant,StoreManager")]

    public class InvoicesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public InvoicesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }
        /// <summary>
        /// جلب الفاتورة الرسمية للعميل لطلب معين
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("Retrieves the official Customer Invoice for a specific order")]
        public IActionResult RetriveCustomerInvoiceByOrderId([FromQuery] string orderId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.OrderId == orderId && i.Type == InvoiceType.CustomerInvoice);
            var invoice = repo.ListAsync(spec).Result.FirstOrDefault();
            if (invoice == null) return NotFound();
            var dto = new InvoiceDTO
            {
                Id = invoice.Id,
                Type = invoice.Type,
                RecipientName = invoice.RecipientName,
                Amount = invoice.Amount,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                GeneratedDate = invoice.DateOfCreation,
                OrderId = invoice.OrderId
            };
            return Ok(dto);
        }
        /// <summary>
        /// جلب جميع فواتير العملاء الرسمية
        /// </summary>
        /// <returns></returns>
      //  [Authorize(Roles = "Customer")]
        [HttpGet("Retrieves All official Customer Invoices")]
        public IActionResult RetriveAllCustomerInvoices()
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.Type == InvoiceType.CustomerInvoice);
            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var invoiceDTOs = invoices.Select(i => new InvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                RecipientName = i.RecipientName,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                GeneratedDate = i.DateOfCreation,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }
        /// <summary>
        /// جلب الفاتورة الرسمية لمندوب المبيعات لطلب معين
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
    //    [Authorize(Roles = "SalesRep")]
        [HttpGet("Retrieves the official SalesRep Invoice for a specific order")]
        public IActionResult RetriveSalesRepByOrderId([FromQuery] string orderId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.OrderId == orderId && i.Type == InvoiceType.CommissionInvoice);
            var invoice = repo.ListAsync(spec).Result.FirstOrDefault();
            if (invoice == null) return NotFound();
            var dto = new InvoiceDTO
            {
                Id = invoice.Id,
                Type = invoice.Type,
                RecipientName = invoice.RecipientName,
                Amount = invoice.Amount,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount= invoice.RemainingAmount,
                GeneratedDate = invoice.DateOfCreation,
                OrderId = invoice.OrderId
            };
            return Ok(dto);
        }
        /// <summary>
        /// جلب جميع فواتير مندوبي المبيعات الرسمية
        /// </summary>
        /// <returns></returns>
       // [Authorize(Roles = "SalesRep")]
        [HttpGet("Retrieves All official SalesRep Invoices")]
        public IActionResult RetriveAllSalesRepInvoices()
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.Type == InvoiceType.CommissionInvoice);
            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var invoiceDTOs = invoices.Select(i => new InvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                RecipientName = i.RecipientName,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                GeneratedDate = i.DateOfCreation,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }
        /// <summary>
        /// Retrieves All Invoices (SystemAdmin Only)
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "SystemAdmin")]
        [HttpGet("Retrieves All Invoices")]
        public IActionResult RetriveAllInvoices()
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var invoices = repo.ListAllAsync().Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var invoiceDTOs = invoices.Select(i => new InvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                RecipientName = i.RecipientName,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                GeneratedDate = i.DateOfCreation,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }

        /// <summary>
        /// جلب جميع فواتير العملاء الرسمية لعميل معين بواسطة معرف العميل
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("AllInvoicesForSpecificCustomerByCustomerId")]
        public IActionResult GetAllInvoicesForSpecificCustomerByCustomerId([FromQuery] string customerId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();

            var spec = new BaseSpecification<Invoice>(i=> i.Type == InvoiceType.CustomerInvoice);
            var repoOrder = unitOfWork.Repository<Order>();
            if (repoOrder == null) return NotFound();
            var specOrder = new BaseSpecification<Order>(o => o.CustomerId == customerId);
            var orders = repoOrder.ListAsync(specOrder).Result;
            var orderIds = orders.Select(o => o.Id).ToList();
            spec.Criteria = i => orderIds.Contains(i.OrderId) && i.Type == InvoiceType.CustomerInvoice;

            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var invoiceDTOs = invoices.Select(i => new InvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                RecipientName = i.RecipientName,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                GeneratedDate = i.DateOfCreation,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }
        /// <summary>
        /// جلب جميع فواتير مندوبي المبيعات الرسمية لمندوب مبيعات معين بواسطة معرف مندوب المبيعات
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <returns></returns>
        [HttpGet("AllInvoicesForSpecificSalesRepBySalesRepId")]
        public IActionResult GetAllInvoicesForSpecificSalesRepBySalesRepId([FromQuery] string salesRepId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.Type == InvoiceType.CommissionInvoice);
            var repoOrder = unitOfWork.Repository<Order>();
            if (repoOrder == null) return NotFound();
            var specOrder = new BaseSpecification<Order>(o => o.SalesRepId == salesRepId);
            var orders = repoOrder.ListAsync(specOrder).Result;
            var orderIds = orders.Select(o => o.Id).ToList();
            spec.Criteria = i => orderIds.Contains(i.OrderId) && i.Type == InvoiceType.CommissionInvoice;
            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var invoiceDTOs = invoices.Select(i => new InvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                RecipientName = i.RecipientName,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                GeneratedDate = i.DateOfCreation,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }
        /// <summary>
        /// جلب الفاتورة الرسمية الأخيرة للعميل لعميل معين بواسطة معرف العميل
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("LastInvoiceForSpecificCustomerByCustomerId")]
        public IActionResult GetLastInvoiceForSpecificCustomerByCustomerId([FromQuery] string customerId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.Type == InvoiceType.CustomerInvoice);
            var repoOrder = unitOfWork.Repository<Order>();
            if (repoOrder == null) return NotFound();
            var specOrder = new BaseSpecification<Order>(o => o.CustomerId == customerId);
            var orders = repoOrder.ListAsync(specOrder).Result;
            var orderIds = orders.Select(o => o.Id).ToList();
            spec.Criteria = i => orderIds.Contains(i.OrderId) && i.Type == InvoiceType.CustomerInvoice;
            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var lastInvoice = invoices.OrderByDescending(i => i.DateOfCreation).FirstOrDefault();
            if (lastInvoice == null) return NotFound();
            var dto = new InvoiceDTO
            {
                Id = lastInvoice.Id,
                Type = lastInvoice.Type,
                RecipientName = lastInvoice.RecipientName,
                Amount = lastInvoice.Amount,
                PaidAmount = lastInvoice.PaidAmount,
                RemainingAmount = lastInvoice.RemainingAmount,
                GeneratedDate = lastInvoice.DateOfCreation,
                OrderId = lastInvoice.OrderId
            };
            return Ok(dto);
        }

        /// <summary>
        /// جلب الفاتورة الرسمية الأخيرة لمندوب المبيعات لمندوب مبيعات معين بواسطة معرف مندوب المبيعات
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <returns></returns>
        [HttpGet("LastInvoiceForSpecificSalesRepBySalesRepId")]
        public IActionResult GetLastInvoiceForSpecificSalesRepBySalesRepId([FromQuery] string salesRepId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.Type == InvoiceType.CommissionInvoice);
            var repoOrder = unitOfWork.Repository<Order>();
            if (repoOrder == null) return NotFound();
            var specOrder = new BaseSpecification<Order>(o => o.SalesRepId == salesRepId);
            var orders = repoOrder.ListAsync(specOrder).Result;
            var orderIds = orders.Select(o => o.Id).ToList();
            spec.Criteria = i => orderIds.Contains(i.OrderId) && i.Type == InvoiceType.CommissionInvoice;
            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var lastInvoice = invoices.OrderByDescending(i => i.DateOfCreation).FirstOrDefault();
            if (lastInvoice == null) return NotFound();
            var dto = new InvoiceDTO
            {
                Id = lastInvoice.Id,
                Type = lastInvoice.Type,
                RecipientName = lastInvoice.RecipientName,
                Amount = lastInvoice.Amount,
                PaidAmount = lastInvoice.PaidAmount,
                RemainingAmount = lastInvoice.RemainingAmount,
                GeneratedDate = lastInvoice.DateOfCreation,
                OrderId = lastInvoice.OrderId
            };
            return Ok(dto);
        }
        /// <summary>
        /// جلب جميع الفواتير مع خيارات البحث والفرز والترقيم الصفحي
        /// </summary>
        /// <param name="search"></param>
        /// <param name="invoiceType"></param>
        /// <param name="orderId"></param>
        /// <param name="recipientName"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<ActionResult> GetAll(
           [FromQuery] string? search,
           [FromQuery] InvoiceType? invoiceType,
           [FromQuery] string? orderId,
           [FromQuery] string? recipientName,
           [FromQuery] string? recipientId,
           [FromQuery] bool? lastInvoice,
           [FromQuery] DateTime? fromDate,
           [FromQuery] DateTime? toDate,


           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 20){
            if (invoiceType.HasValue && InvoiceType.SupplierInvoice == invoiceType.Value)
            {
                var repoS = unitOfWork.Repository<SupplierInvoice>();
                var specS = new BaseSpecification<SupplierInvoice>(i =>
                    (string.IsNullOrEmpty(search) || i.SupplierName.Contains(search)) &&
                    (!invoiceType.HasValue || i.Type == InvoiceType.SupplierInvoice) &&
                    (string.IsNullOrEmpty(recipientName) || i.SupplierName.Contains(recipientName)) &&
                    (!fromDate.HasValue || i.DateOfCreation >= fromDate.Value) &&
                    (!toDate.HasValue || i.DateOfCreation <= toDate.Value)
                );
                var totalItemsS = await repoS.CountAsync(specS);
                specS.ApplyPaging((page - 1) * pageSize, pageSize);
                var invoicesS = await repoS.ListAsync(specS);
                var invoiceDTOsS = invoicesS.Select(i => new InvoiceDTO
                {
                    Id = i.Id,
                    Type = i.Type,
                    RecipientName = i.SupplierName,
                    Amount = i.Amount,
                    PaidAmount = i.PaidAmount,
                    RemainingAmount = i.RemainingAmount,
                    GeneratedDate = i.DateOfCreation,

                }).ToList();
                if (lastInvoice.HasValue && lastInvoice.Value)
                {
                    invoiceDTOsS = invoiceDTOsS
                        .GroupBy(i => i.RecipientName)
                        .Select(g => g.OrderByDescending(i => i.GeneratedDate).First())
                        .ToList();
                }
                var pagedResultS = new PagedResult<InvoiceDTO>
                {
                    Items = invoiceDTOsS,
                    TotalItems = totalItemsS,
                    Page = page,
                    PageSize = pageSize
                };
                return Ok(pagedResultS);
            }
           
                var repo = unitOfWork.Repository<Invoice>();
                if (repo == null) return NotFound();


                var spec = new BaseSpecification<Invoice>(i =>
                    (string.IsNullOrEmpty(search) || i.RecipientName.Contains(search) || i.OrderId.Contains(search)) &&
                    (!invoiceType.HasValue || i.Type == invoiceType.Value) &&
                    (string.IsNullOrEmpty(orderId) || i.OrderId == orderId) &&

                    (string.IsNullOrEmpty(recipientName) || i.RecipientName.Contains(recipientName)) &&
                     (
                         string.IsNullOrEmpty(recipientId) ||

                             (
                                 (i.Type == InvoiceType.CustomerInvoice || i.Type == InvoiceType.ReturnInvoice) &&
                                 i.Order.CustomerId == recipientId
                                                  )
                             ||
                              (
                                 i.Type == InvoiceType.CommissionInvoice &&
                                 i.Order.SalesRepId == recipientId
                             )
                         )
                                          &&
                    (!fromDate.HasValue || i.DateOfCreation >= fromDate.Value) &&
                    (!toDate.HasValue || i.DateOfCreation <= toDate.Value)
                );
                spec.Includes.Add(i => i.Order);
            
            
            var totalItems = await repo.CountAsync(spec);
            spec.ApplyPaging((page - 1) * pageSize, pageSize);
            var invoices = await repo.ListAsync(spec);



            var invoiceDTOs = invoices.Select(i => new InvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                RecipientName = i.RecipientName,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                GeneratedDate = i.DateOfCreation,
                OrderId = i.OrderId
            }).ToList();
            if (lastInvoice.HasValue && lastInvoice.Value)
            {
                invoiceDTOs = invoiceDTOs
                    .GroupBy(i => i.RecipientName)
                    .Select(g => g.OrderByDescending(i => i.GeneratedDate).First())
                    .ToList();
            }
            var pagedResult = new PagedResult<InvoiceDTO>
            {
                Items = invoiceDTOs,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
            return Ok(pagedResult);
        }
        /// <summary>
        /// استلام مبلغ من العميل لطلب معين وتحديث المبلغ المدفوع والمبلغ المتبقي في الفاتورة
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="PayiedAmount"></param>
        /// <returns></returns>
        [HttpPut("PayedAmountFromCustomerByOrderId")]
        public async Task<IActionResult> TakeOrderPriceFromCustomer([FromQuery] CustomerOrSalesRep customerOrSalesRep, [FromQuery] string orderId, [FromQuery] decimal PayiedAmount)
        {
            if(customerOrSalesRep == CustomerOrSalesRep.SalesRep)
            {
                return await PayCommissionToSalesRep(customerOrSalesRep, orderId, PayiedAmount);
            }
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.OrderId == orderId && i.Type == InvoiceType.CustomerInvoice);
            var invoice = repo.ListAsync(spec).Result.FirstOrDefault();
            if (invoice == null) return NotFound();
        
            invoice.PaidAmount += PayiedAmount;
            invoice.RemainingAmount = invoice.RemainingAmount - PayiedAmount;
            if(invoice.RemainingAmount < 0)
            {
                return BadRequest("Paid amount exceeds remaining amount.");
            }
            await repo.UpdateAsync(invoice);
            await unitOfWork.CompleteAsync();
            spec.Includes.Add(i => i.Order);
            var customer = await userManager.FindByIdAsync(invoice.Order.CustomerId);
            if (customer == null) return NotFound();
            
            return Ok(new { invoice.PaidAmount, invoice.RemainingAmount, CustomerName = customer.FullName });
        }
        /// <summary>
        /// جلب المبلغ المتبقي والمبلغ المدفوع لطلب عميل معين بواسطة معرف الطلب
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("RemainingAndPaidAmountForCustomerOrderByOrderId")]
        public async Task<IActionResult> GetRemainingAmountForOrder([FromQuery] string orderId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.OrderId == orderId && i.Type == InvoiceType.CustomerInvoice);
            var invoice = repo.ListAsync(spec).Result.FirstOrDefault();
            if (invoice == null) return NotFound();
            var total = invoice.Amount;
            var remainingAmount = invoice.RemainingAmount;
       
            var payedAmount = invoice.PaidAmount;
            spec.Includes.Add(i => i.Order);
            var customer= await userManager.FindByIdAsync(invoice.Order.CustomerId);
            if (customer == null) return NotFound();
            

            return Ok(new {Total=total, PaidAmount = payedAmount, RemainingAmount = remainingAmount, CustomerName = customer.FullName });
        }
        /// <summary>
        /// دفع عمولة لمندوب المبيعات لطلب معين وتحديث المبلغ المدفوع والمبلغ المتبقي في الفاتورة
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="PayiedAmount"></param>
        /// <returns></returns>
        [HttpPut("PayCommissionToSalesRepByOrderId")]
        public async Task<IActionResult> PayCommissionToSalesRep([FromQuery] CustomerOrSalesRep customerOrSalesRep,[FromQuery] string orderId, [FromQuery] decimal PayiedAmount)
        {
            if(customerOrSalesRep == CustomerOrSalesRep.Customer)
            {
                return await TakeOrderPriceFromCustomer(customerOrSalesRep, orderId, PayiedAmount);
            }
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.OrderId == orderId && i.Type == InvoiceType.CommissionInvoice);
            var invoice = repo.ListAsync(spec).Result.FirstOrDefault();
            if (invoice == null) return NotFound();
            invoice.PaidAmount += PayiedAmount;
            invoice.RemainingAmount = invoice.RemainingAmount - PayiedAmount;

            if (invoice.RemainingAmount < 0)
            {
                return BadRequest("Paid amount exceeds remaining amount.");
            }
            await repo.UpdateAsync(invoice);
            await unitOfWork.CompleteAsync();
            spec.Includes.Add(i => i.Order);
            var salesRep = await userManager.FindByIdAsync(invoice.Order.SalesRepId);
            if (salesRep == null) return NotFound();
            return Ok(new { invoice.Amount,invoice.PaidAmount, invoice.RemainingAmount, SalesRepName = salesRep.FullName });
        }


        /// <summary>
        /// جلب المبلغ المتبقي والمبلغ المدفوع لطلب مندوب مبيعات معين بواسطة معرف الطلب
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("RemainingAndPaidAmountForSalesRepOrderByOrderId")]
        public async Task<IActionResult> GetRemainingAmountForSalesRepOrder([FromQuery] string orderId)
        {
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i => i.OrderId == orderId && i.Type == InvoiceType.CommissionInvoice);
            var invoice = repo.ListAsync(spec).Result.FirstOrDefault();
            if (invoice == null) return NotFound();
            var total = invoice.Amount;
            var remainingAmount = invoice.RemainingAmount;
            var payedAmount = invoice.PaidAmount;
            spec.Includes.Add(i => i.Order);
            var salesRep = await userManager.FindByIdAsync(invoice.Order.SalesRepId);
            if (salesRep == null) return NotFound();
            return Ok(new { Total=total, PaidAmount = payedAmount, RemainingAmount = remainingAmount, SalesRepName = salesRep.FullName });
        }
        /// <summary>
        /// الدفع ل المورد
        /// </summary>
        /// <param name="SupplierInvoiceId"></param>
        /// <param name="PayiedAmount"></param>
        /// <returns></returns>
        [HttpPut("PayPartOfMoneyToSupplierBySupplierInvoiceId")]
        public async Task<IActionResult> PayToSupplier([FromQuery] string SupplierInvoiceId, [FromQuery] decimal PayiedAmount)
        {
            var repo = unitOfWork.Repository<SupplierInvoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<SupplierInvoice>(i => i.Id == SupplierInvoiceId && i.Type == InvoiceType.SupplierInvoice);
            var invoice = repo.ListAsync(spec).Result.FirstOrDefault();
            if (invoice == null) return NotFound();
            invoice.PaidAmount += PayiedAmount;
            invoice.RemainingAmount = invoice.RemainingAmount - PayiedAmount;

            if (invoice.RemainingAmount < 0)
            {
                return BadRequest("Paid amount exceeds remaining amount.");
            }
            await repo.UpdateAsync(invoice);
            await unitOfWork.CompleteAsync();
            spec.Includes.Add(i => i.Supplier);
            var Supplier = await userManager.FindByIdAsync(invoice.Supplier.UserId);
            if (Supplier == null) return NotFound();
            return Ok(new { invoice.Amount, invoice.PaidAmount, invoice.RemainingAmount, Supplier = Supplier.FullName });
        }
       /// <summary>
       /// كل فواتير المورد
       /// </summary>
       /// <param name="supplierId"></param>
       /// <returns></returns>
        
        [HttpGet("AllInvoicesForSpecificSupplierBySupplierId")]
        public async Task<IActionResult> AllInvoicesForSpecificSupplierBySupplierId([FromQuery] string supplierId)
        {
            var reposup = unitOfWork.Repository<Supplier>();
            var specsup=new BaseSpecification<Supplier>(s=>s.UserId==supplierId);
            var supplier = await reposup.GetEntityWithSpecAsync(specsup);
            var repo = unitOfWork.Repository<SupplierInvoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<SupplierInvoice>(i =>i.SupplierId==supplier.Id && i.Type==InvoiceType.SupplierInvoice);
          
         
            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var invoiceDTOs = invoices.Select(i => new SupplierInvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                SupplierName = i.SupplierName,
                Amount = i.Amount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                GeneratedDate = i.DateOfCreation,
                SupplierId=supplierId
                 
            }).ToList();
            return Ok(invoiceDTOs);
        }
        
        /// <summary>
        /// اخر فاتورة مورد
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        [HttpGet("LastInvoiceForSpecificSupplierBySupplierId")]
        public async Task<IActionResult> LastInvoiceForSpecificSupplierBySupplierIdAsync([FromQuery] string supplierId)
        {
            var reposup = unitOfWork.Repository<Supplier>();
            var specsup = new BaseSpecification<Supplier>(s => s.UserId == supplierId);
            var supplier = await reposup.GetEntityWithSpecAsync(specsup);
            var repo = unitOfWork.Repository<SupplierInvoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<SupplierInvoice>(i => i.SupplierId==supplier.Id &&i.Type == InvoiceType.SupplierInvoice);
            var invoices = repo.ListAsync(spec).Result;
            if (invoices == null || !invoices.Any()) return NotFound();
            var lastInvoice = invoices.OrderByDescending(i => i.DateOfCreation).FirstOrDefault();
            if (lastInvoice == null) return NotFound();
            var dto = new SupplierInvoiceDTO
            {
                Id = lastInvoice.Id,
                Type = lastInvoice.Type,
                SupplierName = lastInvoice.SupplierName,
                Amount = lastInvoice.Amount,
                PaidAmount = lastInvoice.PaidAmount,
                RemainingAmount = lastInvoice.RemainingAmount,
                GeneratedDate = lastInvoice.DateOfCreation,
              
            };
            return Ok(dto);
        }
    }
    public enum CustomerOrSalesRep
    {
        Customer,
        SalesRep
    }
    internal class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
