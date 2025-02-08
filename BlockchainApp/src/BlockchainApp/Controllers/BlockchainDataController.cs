using BlockchainApp.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace BlockchainApp.Controllers
{
    [ApiController]
    [Route("api/blockchain")]
    public class BlockchainDataController(
        IBlockchainService blockchainService,
        ILogger<BlockchainDataController> logger)
        : ControllerBase
    {
        [HttpGet("history")]
        public async Task<IActionResult> GetBlockchainHistory(
            [FromQuery] string? blockchainApi,
            [FromQuery] int skip = 1,
            [FromQuery] int pageSize = 10)
        {
            if (skip < 0 )
                return BadRequest("Skip must be non-negative.");

            if (pageSize is < 1 or > 100)
                return BadRequest("Page number must be greater than 0.");

            logger.LogInformation($"Fetching blockchain history (API: {blockchainApi}, Skip: {skip}, Size: {pageSize})");

            var (data, totalCount) = await blockchainService.GetBlockchainHistory( skip, pageSize, blockchainApi);

            var response = new
            {
                TotalCount = totalCount,
                PageNumber = skip,
                Skip = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Data = data
            };

            return Ok(response);
        }
    }
}
