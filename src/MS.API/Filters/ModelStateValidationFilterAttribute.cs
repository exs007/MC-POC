using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MS.API.Models.Common;

namespace MS.API.Filters
{
    /// <summary>
    /// Generate common response structure for the model state errors
    /// </summary>
    public class ModelStateValidationFilterAttribute
        : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var messages = new List<MessageInfo>();
                foreach (var keyValue in context.ModelState)
                    if (keyValue.Value.Errors.Count > 0)
                        messages.AddRange(keyValue.Value.Errors.Where(p => !string.IsNullOrEmpty(p.ErrorMessage))
                            .Select(p => new MessageInfo(keyValue.Key, p.ErrorMessage)));
                var result = new ResponseInfo<object>(messages);
                context.Result = new JsonResult(result);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
        }
    }
}