using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SMSPrototype1.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check ModelState first (for backwards compatibility with Data Annotations)
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    ErrorMessage = string.Join(" | ", context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
                return;
            }

            // Validate each action parameter using FluentValidation
            foreach (var parameter in context.ActionDescriptor.Parameters)
            {
                var argument = context.ActionArguments[parameter.Name];
                if (argument == null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);
                var validator = _serviceProvider.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    var validationContext = new ValidationContext<object>(argument);
                    var validationResult = await validator.ValidateAsync(validationContext);

                    if (!validationResult.IsValid)
                    {
                        context.Result = new BadRequestObjectResult(new
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            ErrorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage))
                        });
                        return;
                    }
                }
            }

            await next();
        }
    }
}
