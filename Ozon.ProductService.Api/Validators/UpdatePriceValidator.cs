using FluentValidation;

namespace Ozon.ProductService.Api.Validators;

public class UpdatePriceValidator : AbstractValidator<UpdatePriceRequest>
{
    public UpdatePriceValidator()
    {
        RuleFor(el => el.Id)
            .NotNull()
            .GreaterThan(0)
            .LessThan(int.MaxValue).WithMessage("The value entered in '{PropertyName}' is too large.");

        RuleFor(el => el.Price)
            .NotNull()
            .GreaterThan(0d)
            .LessThan(double.MaxValue).WithMessage("The value entered in '{PropertyName}' is too large.");
    }
}