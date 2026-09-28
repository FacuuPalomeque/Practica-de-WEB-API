using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class DesafiosController : ControllerBase
    {
        private static List<Participante> participantes = new List<Participante>();

        private static List<Desafio> desafios = new List<Desafio>();

        private static List<Resolucion> resoluciones = new List<Resolucion>();

        [HttpPost("participantes")]
        public IActionResult CreateParticipant(Participante participante)
        {
            bool emailExiste = participantes.Any(p => p.Email.ToLower() == participante.Email.ToLower());

            if (emailExiste)
            {
                return BadRequest("El email ya está registrado.");
            }

            participante.Id = participantes.Count + 1;

            participantes.Add(participante);

            return Ok(participante);
        }

        [HttpPost("desafios")]
        public IActionResult CreateChallenge(Desafio desafio)
        {
            desafio.Id = desafios.Count + 1;

            desafios.Add(desafio);

            return Ok(desafio);
        }

        [HttpPost("resoluciones")]
        public IActionResult CreateResolution(Resolucion resolucion)
        {
            Participante participante = participantes
                .FirstOrDefault(p => p.Id == resolucion.ParticipanteId);

            Desafio desafio = desafios
                .FirstOrDefault(d => d.Id == resolucion.DesafioId);

            if (participante == null)
            {
                return NotFound("El participante no existe.");
            }

            if (desafio == null)
            {
                return NotFound("El desafío no existe.");
            }

            if (desafio.Activo == false)
            {
                return BadRequest("El desafío está inactivo.");
            }

            bool yaResuelto = resoluciones.Any(r =>
                r.ParticipanteId == resolucion.ParticipanteId &&
                r.DesafioId == resolucion.DesafioId);

            if (yaResuelto)
            {
                return BadRequest("El participante ya resolvió este desafío.");
            }

            if (resolucion.PuntajeObtenido > desafio.PuntajeMaximo)
            {
                return BadRequest("El puntaje supera el máximo permitido.");
            }

            resolucion.Id = resoluciones.Count + 1;

            resoluciones.Add(resolucion);

            return Ok(resolucion);
        }

        [HttpGet("desafios/activos")]
        public IActionResult GetActiveChallenges()
        {
            List<Desafio> resultado = desafios
                .Where(d => d.Activo == true)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("desafios/dificultad/{dificultad}")]
        public IActionResult GetByDifficulty(string dificultad)
        {
            List<Desafio> resultado = desafios
                .Where(d => d.Dificultad.ToLower() == dificultad.ToLower())
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("resoluciones/participante/{id}")]
        public IActionResult GetResolutionsByParticipant(int id)
        {
            List<Resolucion> resultado = resoluciones
                .Where(r => r.ParticipanteId == id)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("participantes/puntaje/{puntaje}")]
        public IActionResult GetParticipantsAboveScore(double puntaje)
        {
            List<int> ids = resoluciones
                .Where(r => r.PuntajeObtenido > puntaje)
                .Select(r => r.ParticipanteId)
                .ToList();

            List<Participante> resultado = participantes
                .Where(p => ids.Contains(p.Id))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("desafios/{id}/promedio")]
        public IActionResult GetChallengeAverage(int id)
        {
            List<Resolucion> resultado = resoluciones
                .Where(r => r.DesafioId == id)
                .ToList();

            if (resultado.Count == 0)
            {
                return BadRequest("No hay resoluciones para este desafío.");
            }

            double promedio = resultado.Average(r => r.PuntajeObtenido);

            return Ok(promedio);
        }

        [HttpGet("participantes/mayor-puntaje")]
        public IActionResult GetTopParticipant()
        {
            if (resoluciones.Count == 0)
            {
                return BadRequest("No hay resoluciones.");
            }

            var resultado = participantes
                .Select(p => new
                {
                    Participante = p,
                    PuntajeTotal = resoluciones
                        .Where(r => r.ParticipanteId == p.Id)
                        .Sum(r => r.PuntajeObtenido)
                })
                .OrderByDescending(p => p.PuntajeTotal)
                .FirstOrDefault();

            return Ok(resultado);
        }

        [HttpGet("participantes/ordenados")]
        public IActionResult GetParticipantsOrdered()
        {
            var resultado = participantes
                .Select(p => new
                {
                    Participante = p,
                    PuntajeTotal = resoluciones
                        .Where(r => r.ParticipanteId == p.Id)
                        .Sum(r => r.PuntajeObtenido)
                })
                .OrderByDescending(p => p.PuntajeTotal)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("participantes/{id}/pendientes")]
        public IActionResult GetPendingChallenges(int id)
        {
            List<int> desafiosResueltos = resoluciones
                .Where(r => r.ParticipanteId == id)
                .Select(r => r.DesafioId)
                .ToList();

            List<Desafio> resultado = desafios
                .Where(d => !desafiosResueltos.Contains(d.Id))
                .ToList();

            return Ok(resultado);
        }
    }

    