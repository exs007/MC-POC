using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using MS.API.Filters;
using MS.Core.Constants;
using Xunit;

namespace MS.API.Tests.Filters
{
    public class ModelStateValidationFilterAttributeTests
    {
        private readonly ModelStateValidationFilterAttribute _attribute;
        private readonly ActionExecutingContext _actionExecutingContext;
        private readonly Mock<HttpResponse> _httpResponseMock;

        public ModelStateValidationFilterAttributeTests()
        {
            _attribute = new ModelStateValidationFilterAttribute();
            var httpContextMock = new Mock<HttpContext>();
            _httpResponseMock = new Mock<HttpResponse>();
            httpContextMock.Setup(p => p.Response).Returns(_httpResponseMock.Object);
            var actionContext = new ActionContext();
            actionContext.HttpContext = httpContextMock.Object;
            actionContext.RouteData = new RouteData();
            actionContext.ActionDescriptor = new ActionDescriptor();
            _actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
                new Dictionary<string, object>(), null);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenModelStateValid_OnActionExecuting_NoResult()
        {
            _attribute.OnActionExecuting(_actionExecutingContext);

            Assert.Null(_actionExecutingContext.Result);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenErrorExistInModelState_OnActionExecuting_JsonResultAndStatusCodeSetToBadRequest()
        {
            _actionExecutingContext.ModelState.AddModelError("field1", "error1 field1");

            _attribute.OnActionExecuting(_actionExecutingContext);

            Assert.NotNull(_actionExecutingContext.Result);
            Assert.IsType<JsonResult>(_actionExecutingContext.Result);

            _httpResponseMock.VerifySet(p => p.StatusCode = (int) HttpStatusCode.BadRequest, Times.Once);
        }
    }
}