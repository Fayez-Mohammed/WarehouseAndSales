using Base.API.DTOs;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.SystemModels;
using Base.DAL.Models.SystemModels.Enums;
using Base.Repo.Interfaces;
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
        [Authorize(Roles = "Customer")]
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
        [Authorize(Roles = "Customer")]
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
        [Authorize(Roles = "SalesRep")]
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
        [Authorize(Roles = "SalesRep")]
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
    }
        
}
