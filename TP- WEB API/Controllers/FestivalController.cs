using Microsoft.AspNetCore.Mvc;
namespace FestivalTecnologico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FestivalController : ControllerBase
    {
        private static List<Actividad> actividades = new List<Actividad>();

        private static List<Participante> participantes = new List<Participante>();

        private static List<ReservaActividad> reservas = new List<ReservaActividad>();

        [HttpPost("activities")]
        public IActionResult CreateActivity(Actividad actividad)
        {
            actividad.Id = actividades.Count + 1;

            actividades.Add(actividad);

            return Ok(actividad);
        }

        [HttpPost("participants")]
        public IActionResult CreateParticipant(Participante participante)
        {
            bool emailExiste = participantes.Any(p =>
                p.Email.ToLower() == participante.Email.ToLower());

            if (emailExiste)
            {
                return BadRequest("El email ya está registrado.");
            }

            participante.Id = participantes.Count + 1;

            participantes.Add(participante);

            return Ok(participante);
        }

        [HttpPost("reservations")]
        public IActionResult CreateReservation(ReservaActividad reserva)
        {
            Actividad actividad = actividades
                .FirstOrDefault(a => a.Id == reserva.ActividadId);

            Participante participante = participantes
                .FirstOrDefault(p => p.Id == reserva.ParticipanteId);

            if (actividad == null)
            {
                return NotFound("La actividad no existe.");
            }

            if (participante == null)
            {
                return NotFound("El participante no existe.");
            }

            if (actividad.Activa == false)
            {
                return BadRequest("La actividad está inactiva.");
            }

            int cantidadInscriptos = reservas
                .Count(r => r.ActividadId == actividad.Id);

            if (cantidadInscriptos >= actividad.Capacidad)
            {
                return BadRequest("La actividad está completa.");
            }

            bool yaInscripto = reservas.Any(r =>
                r.ActividadId == actividad.Id &&
                r.ParticipanteId == participante.Id);

            if (yaInscripto)
            {
                return BadRequest("El participante ya está inscripto.");
            }

            List<ReservaActividad> reservasParticipante = reservas
                .Where(r => r.ParticipanteId == participante.Id)
                .ToList();

            foreach (ReservaActividad reservaAnterior in reservasParticipante)
            {
                Actividad actividadAnterior = actividades
                    .FirstOrDefault(a => a.Id == reservaAnterior.ActividadId);

                if (actividadAnterior != null)
                {
                    DateTime inicioNuevo = actividad.Horario;
                    DateTime finNuevo = actividad.Horario.AddMinutes(actividad.DuracionMinutos);

                    DateTime inicioAnterior = actividadAnterior.Horario;
                    DateTime finAnterior = actividadAnterior.Horario.AddMinutes(actividadAnterior.DuracionMinutos);

                    if (inicioNuevo < finAnterior && finNuevo > inicioAnterior)
                    {
                        return BadRequest("El participante ya tiene otra actividad en ese horario.");
                    }
                }
            }

            reserva.Id = reservas.Count + 1;
            reserva.FechaReserva = DateTime.Now;

            reservas.Add(reserva);

            return Ok(reserva);
        }

        [HttpDelete("reservations/{id}")]
        public IActionResult CancelReservation(int id)
        {
            ReservaActividad reserva = reservas
                .FirstOrDefault(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound("La inscripción no existe.");
            }

            reservas.Remove(reserva);

            return Ok("Inscripción cancelada correctamente.");
        }

        [HttpGet("activities/available")]
        public IActionResult GetAvailableActivities()
        {
            List<Actividad> resultado = actividades
                .Where(a => a.Activa == true)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("activities/{id}/participants")]
        public IActionResult GetActivityParticipants(int id)
        {
            List<int> ids = reservas
                .Where(r => r.ActividadId == id)
                .Select(r => r.ParticipanteId)
                .ToList();

            List<Participante> resultado = participantes
                .Where(p => ids.Contains(p.Id))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("participants/{id}/activities")]
        public IActionResult GetParticipantActivities(int id)
        {
            List<int> ids = reservas
                .Where(r => r.ParticipanteId == id)
                .Select(r => r.ActividadId)
                .ToList();

            List<Actividad> resultado = actividades
                .Where(a => ids.Contains(a.Id))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("activities/search")]
        public IActionResult SearchActivities(string nombre)
        {
            List<Actividad> resultado = actividades
                .Where(a => a.Nombre.ToLower().Contains(nombre.ToLower()))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("activities/type/{tipo}")]
        public IActionResult FilterByType(string tipo)
        {
            List<Actividad> resultado = actividades
                .Where(a => a.Tipo.ToLower() == tipo.ToLower())
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("activities/with-spaces")]
        public IActionResult GetActivitiesWithSpaces()
        {
            List<Actividad> resultado = actividades
                .Where(a => reservas.Count(r => r.ActividadId == a.Id) < a.Capacidad)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("activities/full")]
        public IActionResult GetFullActivities()
        {
            List<Actividad> resultado = actividades
                .Where(a => reservas.Count(r => r.ActividadId == a.Id) >= a.Capacidad)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("activities/ordered")]
        public IActionResult GetActivitiesOrdered()
        {
            var resultado = actividades
                .Select(a => new
                {
                    Actividad = a,
                    Inscriptos = reservas.Count(r => r.ActividadId == a.Id)
                })
                .OrderByDescending(a => a.Inscriptos)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("activities/top")]
        public IActionResult GetTopActivity()
        {
            var resultado = actividades
                .Select(a => new
                {
                    Actividad = a,
                    Inscriptos = reservas.Count(r => r.ActividadId == a.Id)
                })
                .OrderByDescending(a => a.Inscriptos)
                .FirstOrDefault();

            return Ok(resultado);
        }

        [HttpGet("activities/occupancy")]
        public IActionResult GetOccupancy()
        {
            var resultado = actividades
                .Select(a => new
                {
                    Actividad = a.Nombre,
                    Inscriptos = reservas.Count(r => r.ActividadId == a.Id),
                    Capacidad = a.Capacidad,
                    Porcentaje = (double)reservas.Count(r => r.ActividadId == a.Id) * 100 / a.Capacidad
                })
                .ToList();

            return Ok(resultado);
        }
    }
}