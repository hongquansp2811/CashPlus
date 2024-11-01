using System.Collections.Generic;
using System;
using AutoMapper;
using Pig.AspNetCore.Application.Mapping;
using LOYALTY.OrderByHQ.Models;
using FluentValidation;

namespace LOYALTY.OrderByHQ.DTO
{
    public class InvoiceDetailDTO : IMapFrom<Invoice>
{
        public Guid Id { get; set; }
        public string TableName { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime date_created { get; set; }
        public string TimeOrder { get; set; }
        public List<OrderItemDetailDTO> OrderItems { get; set; } = new List<OrderItemDetailDTO>();
        public decimal total_gross_amount { get; set; }
        public decimal discount_amount { get; set; }
        public decimal extra_charge { get; set; }
        public decimal subtotal_gross_amount { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Invoice, InvoiceDetailDTO>()
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore()); // Ignoring OrderItems for manual mapping
        }
    }

    public class InvoiceDetailDTOValidator : AbstractValidator<InvoiceDetailDTO>
    {
        public InvoiceDetailDTOValidator()
        {
            RuleFor(x => x.TableName).NotNull().NotEmpty().WithMessage("errỏr ");
        }
    }

    public class OrderItemDetailDTO
    {
        public string product_name { get; set; }
        public int quantity { get; set; }
        public decimal base_price { get; set; }
        public decimal price_override { get; set; }
    }
}
