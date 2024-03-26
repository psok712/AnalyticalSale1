using FluentValidation;

namespace Ozon.ProductService.Api.Validators;

public class GoodsListValidator : AbstractValidator<GoodsListRequest>
{
    public GoodsListValidator()
    {
        RuleFor(p => p.PageSize)
            .GreaterThan(-1)
            .LessThan(int.MaxValue).WithMessage("The value entered in '{PropertyName}' is too large.");

        RuleFor(p => p.Page)
            .GreaterThan(-1)
            .LessThan(int.MaxValue).WithMessage("The value entered in '{PropertyName}' is too large.");
    }
}