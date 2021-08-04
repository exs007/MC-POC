using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MS.API.Filters
{
    /// <summary>
    /// Execute fluent validators based on a model type and add errors in the model state
    /// </summary>
    public class FluentValidationFilterAttribute
        : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var factory = context.HttpContext.RequestServices.GetService<IValidatorFactory>();

            foreach (var item in context?.ActionArguments)
            {
                // ReSharper disable once PossibleNullReferenceException
                var validator = factory.GetValidator(item.Value.GetType());

                if (validator == null)
                    continue;

                var validationResult = await validator.ValidateAsync(item.Value);

                if (!validationResult.IsValid)
                    //Set all errors in the model state for ability to handle errors from attribute based validation
                    foreach (var validationError in validationResult.Errors)
                        context.ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
            }

            await next();
        }
    }
}