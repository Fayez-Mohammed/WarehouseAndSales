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

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        /// Retrieves the official Customer Invoice for a specific order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
       // [Authorize(Roles = "Customer")]
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
                GeneratedDate = invoice.GeneratedDate,
                OrderId = invoice.OrderId
            };
            return Ok(dto);
        }
        /// <summary>
        /// Retrieves All Customer Invoices
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
                GeneratedDate = i.GeneratedDate,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }
        /// <summary>
        /// Retrieves the official SalesRep Invoice for a specific order
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
                GeneratedDate = invoice.GeneratedDate,
                OrderId = invoice.OrderId
            };
            return Ok(dto);
        }
        /// <summary>
        /// Retrieves All SalesRep Invoices
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
                GeneratedDate = i.GeneratedDate,
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
                GeneratedDate = i.GeneratedDate,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }

        /// <summary>
        /// Retrieves all invoices for a specific customer by their customer ID.
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
                GeneratedDate = i.GeneratedDate,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }
        /// <summary>
        /// Retrieves all invoices for a specific sales representative by their sales rep ID.
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
                GeneratedDate = i.GeneratedDate,
                OrderId = i.OrderId
            }).ToList();
            return Ok(invoiceDTOs);
        }
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
            var lastInvoice = invoices.OrderByDescending(i => i.GeneratedDate).FirstOrDefault();
            if (lastInvoice == null) return NotFound();
            var dto = new InvoiceDTO
            {
                Id = lastInvoice.Id,
                Type = lastInvoice.Type,
                RecipientName = lastInvoice.RecipientName,
                Amount = lastInvoice.Amount,
                GeneratedDate = lastInvoice.GeneratedDate,
                OrderId = lastInvoice.OrderId
            };
            return Ok(dto);
        }

        /// <summary>
        /// Retrieves the most recent invoice for a specific sales representative by their sales rep ID.
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
            var lastInvoice = invoices.OrderByDescending(i => i.GeneratedDate).FirstOrDefault();
            if (lastInvoice == null) return NotFound();
            var dto = new InvoiceDTO
            {
                Id = lastInvoice.Id,
                Type = lastInvoice.Type,
                RecipientName = lastInvoice.RecipientName,
                Amount = lastInvoice.Amount,
                GeneratedDate = lastInvoice.GeneratedDate,
                OrderId = lastInvoice.OrderId
            };
            return Ok(dto);
        }
        /// <summary>
        /// Retrieves a paginated list of invoices based on the specified search criteria.
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
           
           [FromQuery] DateTime? fromDate,
           [FromQuery] DateTime? toDate,


           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 20){
            var repo = unitOfWork.Repository<Invoice>();
            if (repo == null) return NotFound();
            var spec = new BaseSpecification<Invoice>(i =>
                (string.IsNullOrEmpty(search) || i.RecipientName.Contains(search) || i.OrderId.Contains(search)) &&
                (!invoiceType.HasValue || i.Type == invoiceType.Value) &&
                (string.IsNullOrEmpty(orderId) || i.OrderId == orderId) &&
                (string.IsNullOrEmpty(recipientName) || i.RecipientName.Contains(recipientName)) &&
                (!fromDate.HasValue || i.GeneratedDate >= fromDate.Value) &&
                (!toDate.HasValue || i.GeneratedDate <= toDate.Value)
            );
            var totalItems = await repo.CountAsync(spec);
            spec.ApplyPaging((page - 1) * pageSize, pageSize);
            var invoices = await repo.ListAsync(spec);
            var invoiceDTOs = invoices.Select(i => new InvoiceDTO
            {
                Id = i.Id,
                Type = i.Type,
                RecipientName = i.RecipientName,
                Amount = i.Amount,
                GeneratedDate = i.GeneratedDate,
                OrderId = i.OrderId
            }).ToList();
            var pagedResult = new PagedResult<InvoiceDTO>
            {
                Items = invoiceDTOs,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
            return Ok(pagedResult);
        }
        }

    internal class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
