using FluentValidation;
using MS.API.Constants;
using MS.API.Customers.Models.Customers;

namespace MS.API.Customers.Validators.Customers
{
    public class CustomerEditModelValidator : AbstractValidator<CustomerEditModel>
    {
        public CustomerEditModelValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .WithMessage(ValidationConstants.FIELD_REQUIRED_FORMAT);
        }
    }
}