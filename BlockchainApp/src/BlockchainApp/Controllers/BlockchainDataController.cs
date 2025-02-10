using BlockchainApp.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;



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
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 10)
        {
            if (page < 0 )
                return BadRequest("Page must be non-negative.");

            if (pageSize is < 1 or > 100)
                return BadRequest("Page size must be greater than 0.");

            logger.LogInformation($"Fetching blockchain history (API: {blockchainApi}, Skip: {page}, Size: {pageSize})");

            var (data, totalCount) = await blockchainService.GetBlockchainHistory( page, pageSize, blockchainApi);

           var response = new
               {
                   TotalCount = totalCount,
                   Page = page,
                   PageSize = pageSize,
                   Data = JsonConvert.SerializeObject(data, Formatting.Indented)

           };
            return Ok(response);
        }
    }
}