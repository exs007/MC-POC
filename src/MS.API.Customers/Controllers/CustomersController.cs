using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MS.API.Controllers;
using MS.API.Customers.Entities.Customers;
using MS.API.Customers.Models.Customers;
using MS.API.Exceptions;
using MS.API.Extensions;
using MS.API.Models.Common;
using MS.DataAccess.Repositories;

namespace MS.API.Customers.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : APIBaseController
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IMapper _mapper;

        public CustomersController(IRepository<Customer> customerRepository,
            IMapper mapper)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a list of customers
        /// </summary>
        /// <returns>List of customers</returns>
        /// <response code="200">Returns the list of customers</response>
        /// <response code="500">Internal server error</response> 
        [HttpGet]
        [ProducesResponseType(typeof(ResponseInfo<IEnumerable<CustomerListModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseInfo<IEnumerable<CustomerListModel>>),
            StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ResponseInfo<IEnumerable<CustomerListModel>>> GetCustomers()
        {
            var customers = await _customerRepository.ToListAsync();
            return customers.Select(p => _mapper.Map<CustomerListModel>(p)).ToList();
        }

        /// <summary>
        /// Returns a customer
        /// </summary>
        /// <returns>Customer view record</returns>
        /// <response code="200">Returns a customer</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Not found</response>
        /// <response code="500">Internal server error</response> 
        [HttpGet("{id}", Name = nameof(GetCustomer))]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ResponseInfo<CustomerViewModel>> GetCustomer(string id)
        {
            var parsedId = ParseInputId(id);
            var customer = await _customerRepository.FirstOrDefaultAsync(p => p.CustomerId == parsedId);
            CheckOnNull(customer);
            return _mapper.Map<CustomerViewModel>(customer);
        }


        /// <summary>
        /// Create a customer
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/customers
        ///     {
        ///        "name": "Test988"
        ///     }
        ///
        /// </remarks>
        /// <param name="model">Input model</param>
        /// <returns>A new customer view record</returns>
        /// <response code="201">A customer was created</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="500">Internal server error</response> 
        [HttpPost]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ResponseInfo<CustomerViewModel>> AddCustomer([FromBody] CustomerEditModel model)
        {
            var customer = await _customerRepository.InsertAsync(new Customer
            {
                Name = model.Name
            });

            return Response.GetCreatedResource(_mapper.Map<CustomerViewModel>(customer),
                Url.Link(nameof(GetCustomer), new {id = customer.CustomerId}));
        }

        /// <summary>
        /// Update a customer
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/customers/1
        ///     {
        ///        "name": "Test988"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Customer id</param>
        /// <param name="model">Input model</param>
        /// <returns>Updated customer view record</returns>
        /// <response code="200">Customer was updated</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Not found</response>
        /// <response code="500">Internal server error</response> 
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseInfo<CustomerViewModel>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ResponseInfo<CustomerViewModel>> EditCustomer(string id, [FromBody] CustomerEditModel model)
        {
            var parsedId = ParseInputId(id);
            var customer = await _customerRepository.FirstOrDefaultAsync(p => p.CustomerId == parsedId);
            CheckOnNull(customer);
            customer.Name = model.Name;
            customer = await _customerRepository.UpdateAsync(customer);

            return _mapper.Map<CustomerViewModel>(customer);
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <returns>Delete operation status</returns>
        /// <response code="200">Customer was deleted</response>
        /// <response code="400">Invalid input parameters</response>
        /// <response code="404">Not found</response>
        /// <response code="500">Internal server error</response> 
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseInfo<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseInfo<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseInfo<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseInfo<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ResponseInfo<bool>> DeleteCustomer(string id)
        {
            var parsedId = ParseInputId(id);
            var customer = await _customerRepository.FirstOrDefaultAsync(p => p.CustomerId == parsedId);
            CheckOnNull(customer);
            return await _customerRepository.DeleteAsync(customer);
        }

        private int ParseInputId(string id)
        {
            if (!int.TryParse(id, out var parsedId)) throw new AppValidationException("Invalid id");
            return parsedId;
        }

        private void CheckOnNull(Customer customer)
        {
            if (customer == null) throw new AppValidationException("No Customer", HttpStatusCode.NotFound);
        }
    }
}