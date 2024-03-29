using FluentValidation;

namespace Ozon.ProductService.Api.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(el => el.Name)
            .NotNull()
            .MinimumLength(3)
            .MaximumLength(128);

        RuleFor(el => el.WarehouseId)
            .NotNull()
            .GreaterThan(0)
            .LessThan(long.MaxValue).WithMessage("The value entered in '{PropertyName}' is too large.");

        RuleFor(el => el.Price)
            .NotNull()
            .GreaterThan(0d)
            .LessThan(double.MaxValue).WithMessage("The value entered in '{PropertyName}' is too large.");

        RuleFor(el => el.CategoryProduct)
            .NotNull()
            .Must(el => el != Goods.Types.CategoryGoods.None)
            .WithMessage("'{PropertyName}' cannot have the value 'None'.")
            .IsInEnum();

        RuleFor(el => el.Weight)
            .NotNull()
            .GreaterThan(0d)
            .LessThan(double.MaxValue).WithMessage("The value entered in '{PropertyName}' is too large.");
    }
}