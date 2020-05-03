using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OddsFromHypergeometric
{
    public static class OddsCalculator
    {
        [FunctionName("OddsCalculator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string populationSizeStr = req.Query["CardsRemaining"];
            string successesStr = req.Query["Outs"];
            string drawsStr = req.Query["Draws"];
            string neededStr = req.Query["Needed"];

            if (string.IsNullOrWhiteSpace(populationSizeStr) || string.IsNullOrWhiteSpace(successesStr) ||
                string.IsNullOrWhiteSpace(drawsStr) || string.IsNullOrWhiteSpace(neededStr)) return new BadRequestObjectResult("Missing Parameters");

            var popSuccess = int.TryParse(populationSizeStr, out var population);
            var successesSuccess = int.TryParse(successesStr, out var successes);
            var drawsSuccess = int.TryParse(drawsStr, out var draws);
            var neededSuccess = int.TryParse(neededStr, out var needed);

            if (!popSuccess || !successesSuccess || !drawsSuccess || !neededSuccess) return new BadRequestObjectResult("Numbers have to be an int");

            var dist = new Hypergeometric(population, successes, draws);

            var result = 1 - dist.CumulativeDistribution(needed - 1);

            return new OkObjectResult(result.ToString(CultureInfo.InvariantCulture));
        }
    }
}
