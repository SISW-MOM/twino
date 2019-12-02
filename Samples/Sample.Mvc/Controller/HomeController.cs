﻿using System.Linq;
using System.Threading.Tasks;
using Twino.Mvc.Controllers;
using Twino.Mvc.Filters.Route;

namespace Sample.Mvc.Controller
{
    [Route("")]
    public class HomeController : TwinoController
    {
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            return await StringAsync("Welcome!");
        }

        [HttpPost("")]
        public async Task<IActionResult> Post()
        {
            string data = "";
            foreach (var kv in Request.Form)
            {
                data += kv.Key + ": " + kv.Value + "\r\n";
            }

            data += Request.Files.Count() + " Files";
            return await StringAsync(data);
        }
    }
}