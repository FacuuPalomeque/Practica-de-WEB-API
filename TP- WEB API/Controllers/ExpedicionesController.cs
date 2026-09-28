using Microsoft.AspNetCore.Mvc;
namespace Expediciones.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpedicionesController : ControllerBase
    {
        private static List<Expedicion> expediciones = new List<Expedicion>();

        private static List<Explorador> exploradores = new List<Explorador>();

        private static List<Participacion> participaciones = new List<Participacion>();

        [HttpPost]
        public IActionResult CreateExpedition(Expedicion expedicion)
        {
            if (expedicion.FechaFin < expedicion.FechaInicio)
            {
                return BadRequest("La fecha de fin no puede ser anterior a la fecha de inicio.");
            }

            expedicion.Id = expediciones.Count + 1;

            expediciones.Add(expedicion);

            return Ok(expedicion);
        }

        [HttpPost("explorers")]
        public IActionResult CreateExplorer(Explorador explorador)
        {
            explorador.Id = exploradores.Count + 1;

            exploradores.Add(explorador);

            return Ok(explorador);
        }

        [HttpPost("participations")]
        public IActionResult AssignExplorer(Participacion participacion)
        {
            Expedicion expedicion = expediciones
                .FirstOrDefault(e => e.Id == participacion.ExpedicionId);

            Explorador explorador = exploradores
                .FirstOrDefault(e => e.Id == participacion.ExploradorId);

            if (expedicion == null)
            {
                return NotFound("La expedición no existe.");
            }

            if (explorador == null)
            {
                return NotFound("El explorador no existe.");
            }

            if (explorador.Disponible == false)
            {
                return BadRequest("El explorador no está disponible.");
            }

            int cantidadParticipantes = participaciones
                .Count(p => p.ExpedicionId == expedicion.Id);

            if (cantidadParticipantes >= expedicion.Capacidad)
            {
                return BadRequest("La expedición está completa.");
            }

            bool yaAsignado = participaciones.Any(p =>
                p.ExpedicionId == expedicion.Id &&
                p.ExploradorId == explorador.Id);

            if (yaAsignado)
            {
                return BadRequest("El explorador ya está asignado a esta expedición.");
            }

            List<Participacion> participacionesAnteriores = participaciones
                .Where(p => p.ExploradorId == explorador.Id)
                .ToList();

            foreach (Participacion anterior in participacionesAnteriores)
            {
                Expedicion expedicionAnterior = expediciones
                    .FirstOrDefault(e => e.Id == anterior.ExpedicionId);

                if (expedicionAnterior != null)
                {
                    if (expedicion.FechaInicio < expedicionAnterior.FechaFin &&
                        expedicion.FechaFin > expedicionAnterior.FechaInicio)
                    {
                        return BadRequest("El explorador ya tiene otra expedición en esas fechas.");
                    }
                }
            }

            participacion.Id = participaciones.Count + 1;

            participaciones.Add(participacion);

            return Ok(participacion);
        }

        [HttpGet("{id}/explorers")]
        public IActionResult GetExpeditionExplorers(int id)
        {
            List<int> ids = participaciones
                .Where(p => p.ExpedicionId == id)
                .Select(p => p.ExploradorId)
                .ToList();

            List<Explorador> resultado = exploradores
                .Where(e => ids.Contains(e.Id))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("explorers/{id}/expeditions")]
        public IActionResult GetExplorerExpeditions(int id)
        {
            List<int> ids = participaciones
                .Where(p => p.ExploradorId == id)
                .Select(p => p.ExpedicionId)
                .ToList();

            List<Expedicion> resultado = expediciones
                .Where(e => ids.Contains(e.Id))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("explorers/specialty/{especialidad}")]
        public IActionResult GetBySpecialty(string especialidad)
        {
            List<Explorador> resultado = exploradores
                .Where(e => e.Especialidad.ToLower() == especialidad.ToLower())
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("explorers/available")]
        public IActionResult GetAvailableExplorers()
        {
            List<Explorador> resultado = exploradores
                .Where(e => e.Disponible == true)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("active")]
        public IActionResult GetActiveExpeditions()
        {
            List<Expedicion> resultado = expediciones
                .Where(e => e.Estado.ToLower() == "activa")
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("future")]
        public IActionResult GetFutureExpeditions()
        {
            List<Expedicion> resultado = expediciones
                .Where(e => e.FechaInicio > DateTime.Now)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("full")]
        public IActionResult GetFullExpeditions()
        {
            List<Expedicion> resultado = expediciones
                .Where(e =>
                    participaciones.Count(p => p.ExpedicionId == e.Id) >= e.Capacidad)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("{id}/count")]
        public IActionResult GetParticipantCount(int id)
        {
            int cantidad = participaciones
                .Count(p => p.ExpedicionId == id);

            return Ok(cantidad);
        }

        [HttpGet("occupancy")]
        public IActionResult GetOccupancy()
        {
            var resultado = expediciones
                .Select(e => new
                {
                    Expedicion = e.Nombre,
                    Participantes = participaciones.Count(p => p.ExpedicionId == e.Id),
                    Capacidad = e.Capacidad,
                    Porcentaje = (double)participaciones.Count(p => p.ExpedicionId == e.Id)
                        * 100 / e.Capacidad
                })
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("ordered")]
        public IActionResult GetOrderedExpeditions()
        {
            List<Expedicion> resultado = expediciones
                .OrderBy(e => e.FechaInicio)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("explorers/top")]
        public IActionResult GetTopExplorers()
        {
            var resultado = exploradores
                .Select(e => new
                {
                    Explorador = e,
                    CantidadExpediciones = participaciones
                        .Count(p => p.ExploradorId == e.Id)
                })
                .OrderByDescending(e => e.CantidadExpediciones)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("specialties")]
        public IActionResult GetMostUsedSpecialties()
        {
            var resultado = exploradores
                .GroupBy(e => e.Especialidad)
                .Select(e => new
                {
                    Especialidad = e.Key,
                    Cantidad = e.Count()
                })
                .OrderByDescending(e => e.Cantidad)
                .ToList();

            return Ok(resultado);
        }
    }
}