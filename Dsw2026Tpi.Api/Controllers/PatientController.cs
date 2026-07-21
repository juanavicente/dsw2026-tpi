using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
    public class PatientController
    {
        [HttpGet]
        public IActionResult GetAll()

[HttpGet("{id}")]
        public IActionResult GetById(Guid id)

[HttpPost]
        public IActionResult Create(...)

[HttpPut("{id}")]
        public IActionResult Update(...)

[HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
    }
}
