using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
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
    public class FluentValidationFilterAttributeTests
    {
        private readonly FluentValidationFilterAttribute _attribute;
        private readonly Mock<IValidatorFactory> _validationFactoryMock;
        private readonly ActionExecutingContext _actionExecutingContext;

        public FluentValidationFilterAttributeTests()
        {
            _attribute = new FluentValidationFilterAttribute();
            _validationFactoryMock = new Mock<IValidatorFactory>();
            var httpContextMock = new Mock<HttpContext>();
            var actionContext = new ActionContext();
            actionContext.HttpContext = httpContextMock.Object;
            actionContext.RouteData = new RouteData();
            actionContext.ActionDescriptor = new ActionDescriptor();
            _actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
                new Dictionary<string, object>(), null);
            var serviceProviderMock = new Mock<IServiceProvider>();
            _actionExecutingContext.HttpContext = httpContextMock.Object;
            httpContextMock.SetupGet(p => p.RequestServices).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IValidatorFactory)))
                .Returns(_validationFactoryMock.Object);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task
            WhenValidatorExistsForModelAndValidationErrors_OnActionExecutionAsync_ErrorsAreAddedIntoModelState()
        {
            _actionExecutingContext.ActionArguments.Add("model", new ValidationTestModel());
            var validator = new Mock<IValidator>();
            _validationFactoryMock.Setup(p => p.GetValidator(typeof(ValidationTestModel))).Returns(validator.Object);
            var validationResult = new ValidationResult(new List<ValidationFailure> {new("field1", "field1 error1")});
            validator.Setup(p => p.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            var actionExecutionDelegateMock = new Mock<ActionExecutionDelegate>();

            await _attribute.OnActionExecutionAsync(_actionExecutingContext,
                actionExecutionDelegateMock.Object);

            Assert.False(_actionExecutingContext.ModelState.IsValid);
            var key = _actionExecutingContext.ModelState.Keys.First();
            var value = _actionExecutingContext.ModelState.Values.First();
            Assert.Equal("field1", key);
            Assert.Equal("field1 error1", value.Errors[0].ErrorMessage);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenValidatorNotExistsForModel_OnActionExecutionAsync_ValidModelState()
        {
            _actionExecutingContext.ActionArguments.Add("model", new ValidationTestModel());
            _validationFactoryMock.Setup(p => p.GetValidator(typeof(ValidationTestModel))).Returns((IValidator) null);
            var actionExecutionDelegateMock = new Mock<ActionExecutionDelegate>();

            await _attribute.OnActionExecutionAsync(_actionExecutingContext,
                actionExecutionDelegateMock.Object);

            Assert.True(_actionExecutingContext.ModelState.IsValid);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenModelNotProvidedInActionArguments_OnActionExecutionAsync_ValidModelState()
        {
            var validator = new Mock<IValidator>();
            _validationFactoryMock.Setup(p => p.GetValidator(typeof(ValidationTestModel))).Returns(validator.Object);
            var validationResult = new ValidationResult(new List<ValidationFailure> {new("field1", "field1 error1")});
            validator.Setup(p => p.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            var actionExecutionDelegateMock = new Mock<ActionExecutionDelegate>();

            await _attribute.OnActionExecutionAsync(_actionExecutingContext,
                actionExecutionDelegateMock.Object);

            Assert.True(_actionExecutingContext.ModelState.IsValid);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenValidatorExistsForModelAndNoValidationErrors_OnActionExecutionAsync_ValidModelState()
        {
            _actionExecutingContext.ActionArguments.Add("model", new ValidationTestModel());
            var validator = new Mock<IValidator>();
            _validationFactoryMock.Setup(p => p.GetValidator(typeof(ValidationTestModel))).Returns(validator.Object);
            var validationResult = new ValidationResult(new List<ValidationFailure>());
            validator.Setup(p => p.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            var actionExecutionDelegateMock = new Mock<ActionExecutionDelegate>();

            await _attribute.OnActionExecutionAsync(_actionExecutingContext,
                actionExecutionDelegateMock.Object);

            Assert.True(_actionExecutingContext.ModelState.IsValid);
        }
    }
}