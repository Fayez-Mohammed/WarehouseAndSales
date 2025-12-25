using Base.API.DTOs;
using FluentValidation;

namespace BaseAPI.Validation.ProductValidation;

public class ProductDtoValidation : AbstractValidator<ProductDto>
{

  public ProductDtoValidation()
  {
    RuleFor(x => x.Quantity)
      .GreaterThan(0);
    RuleFor(x => x.SalePrice)
      .GreaterThan(0);
    RuleFor(x => x.ProductName)
      .NotEmpty();
    RuleFor(x => x.SKU)
      .NotEmpty();
  }
}
public class ProductWithCategoryNameDtoValidation : AbstractValidator<ProductWithCategoryNameDto>
{

  public ProductWithCategoryNameDtoValidation()
  {
    RuleFor(x => x.Quantity)
      .GreaterThan(0);
    RuleFor(x => x.SalePrice)
      .GreaterThan(0);
    RuleFor(x => x.ProductName)
      .NotEmpty();
    RuleFor(x => x.SKU)
      .NotEmpty();
  }
}