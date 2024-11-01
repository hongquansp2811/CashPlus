using LOYALTY.DataObjects.Response;
using LOYALTY.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pig.AspNetCore.Application.Exceptions;
using Pig.AspNetCore.Application.Wrappers;
using System.Linq;

namespace LOYALTY.Attribute
{
    public class CustomFluentValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.Where(v => v.Errors.Count > 0)
                        .SelectMany(v => v.Errors)
                        .Select(v => v.ErrorMessage)
                        .ToList();
                context.Result = new JsonResult(new APIResponse(string.Join(",", errors)));
            }
        }
    }
}
