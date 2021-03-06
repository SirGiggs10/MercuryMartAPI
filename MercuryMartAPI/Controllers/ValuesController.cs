﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercuryMartAPI.Interfaces.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public ValuesController()
        {
            
        }

        // GET: api/Values
        [HttpGet("")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value 1", "value 2", "Value 3", "value 4", "value 5", "Value 6", "Value 7", "Value 8", "Value 9" };
        }

        // GET: api/Values/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
