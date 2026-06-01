using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autobarn.Website.Api {
	[Route("api/[controller]")]
	[ApiController]
	public class VehiclesController(AutobarnDbContext db) : ControllerBase {

		[HttpGet]
		public IEnumerable<Vehicle> Get() => db.Vehicles.ToList();

		// GET api/<VehiclesController>/5
		[HttpGet("{registration}")]
		public Vehicle Get(string registration) => db.Vehicles.Find(registration);

		//// POST api/<VehiclesController>
		//[HttpPost]
		//[ProducesResponseType(StatusCodes.Status201Created)]
		//[HttpPost("{modelCode}/vehicles/{registration}")]
		//public void Post([FromBody] VehicleDto dto) {
		//}

		//// PUT api/<VehiclesController>/5
		//[HttpPut("{id}")]
		//public void Put(int id, [FromBody] string value) {
		//}

		//// DELETE api/<VehiclesController>/5
		//[HttpDelete("{id}")]
		//public void Delete(int id) {
		//}
	}
}
