using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UniversityEventsAPI.Models;

namespace UniversityEventsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        // Verilənlər bazası əvəzinə yaddaşda saxlanan siyahı
        private static readonly List<Event> _events = new()
        {
            new Event { Id = 1, Name = "AI Conference 2027", Description = "Artificial Intelligence Conference",
                        Location = "Baku", EventDate = new DateTime(2027, 5, 20, 10, 0, 0), MaxParticipants = 150 },
            new Event { Id = 2, Name = "Career Day", Description = "Meeting with employers",
                        Location = "Ganja", EventDate = new DateTime(2027, 3, 10, 11, 0, 0), MaxParticipants = 80 }
        };
        private static int _nextId = 3;

        // GET /api/events?search=ai&city=Baku&upcoming=true&moreThan100=true&sortByDate=true
        [HttpGet]
        public ActionResult<IEnumerable<Event>> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? city,
            [FromQuery] bool upcoming = false,
            [FromQuery] bool moreThan100 = false,
            [FromQuery] bool sortByDate = false)
        {
            IEnumerable<Event> query = _events;

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(e => e.Location.Equals(city, StringComparison.OrdinalIgnoreCase));

            if (upcoming)
                query = query.Where(e => e.EventDate > DateTime.Now);

            if (moreThan100)
                query = query.Where(e => e.MaxParticipants > 100);

            if (sortByDate)
                query = query.OrderBy(e => e.EventDate);

            return Ok(query.ToList());
        }

        // GET /api/events/export  -> JSON faylı kimi yükləyir
        [HttpGet("export")]
        public IActionResult Export()
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(_events,
                new JsonSerializerOptions { WriteIndented = true });
            return File(json, "application/json", "events.json");
        }

        // GET /api/events/{id}
        [HttpGet("{id:int}")]
        public ActionResult<Event> GetById(int id)
        {
            var ev = _events.FirstOrDefault(e => e.Id == id);
            if (ev is null) return NotFound($"ID={id} olan tədbir tapılmadı.");
            return Ok(ev);
        }

        // POST /api/events
        [HttpPost]
        public ActionResult<Event> Create([FromBody] Event newEvent)
        {
            var error = Validate(newEvent);
            if (error is not null) return BadRequest(error);

            newEvent.Id = _nextId++;
            _events.Add(newEvent);
            return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
        }

        // PUT /api/events/{id}
        [HttpPut("{id:int}")]
        public ActionResult<Event> Update(int id, [FromBody] Event updated)
        {
            var existing = _events.FirstOrDefault(e => e.Id == id);
            if (existing is null) return NotFound($"ID={id} olan tədbir tapılmadı.");

            var error = Validate(updated);
            if (error is not null) return BadRequest(error);

            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.Location = updated.Location;
            existing.EventDate = updated.EventDate;
            existing.MaxParticipants = updated.MaxParticipants;
            return Ok(existing);
        }

        // DELETE /api/events/{id}
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var ev = _events.FirstOrDefault(e => e.Id == id);
            if (ev is null) return NotFound($"ID={id} olan tədbir tapılmadı.");

            _events.Remove(ev);
            return NoContent();
        }

        // Biznes qaydaları
        private static string? Validate(Event e)
        {
            if (string.IsNullOrWhiteSpace(e.Name))
                return "Tədbirin adı boş ola bilməz.";
            if (e.EventDate < DateTime.Now)
                return "Tədbirin tarixi keçmiş tarix ola bilməz.";
            if (e.MaxParticipants < 1)
                return "İştirakçı sayı 1-dən az ola bilməz.";
            return null;
        }
    }
}
