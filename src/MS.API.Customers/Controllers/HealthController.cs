using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MS.API.Models.Common;

namespace MS.API.Customers.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns status of the service
        /// </summary>
        /// <returns>Status of the service</returns>
        /// <response code="200">Success health probe call</response>
        /// <response code="500">Internal server error</response> 
        [Route("/")]
        [HttpHead]
        [HttpGet]
        [ProducesResponseType(typeof(ResponseInfo<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseInfo<bool>), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public ResponseInfo<bool> Health()
        {
            //TODO: health probe
            Thread.CurrentThread.Join(20);
            return true;
        }
    }
}