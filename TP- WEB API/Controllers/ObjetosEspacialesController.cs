using Microsoft.AspNetCore.Mvc;
namespace MonitoreoEspacial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjetosEspacialesController : ControllerBase
    {
        private static List<ObjetoEspacial> objetos = new List<ObjetoEspacial>();

        private static List<Observacion> observaciones = new List<Observacion>();

        [HttpPost]
        public IActionResult Create(ObjetoEspacial objeto)
        {
            objeto.Id = objetos.Count + 1;

            objetos.Add(objeto);

            return Ok(objeto);
        }

        [HttpPost("observations")]
        public IActionResult CreateObservation(Observacion observacion)
        {
            ObjetoEspacial objeto = objetos
                .FirstOrDefault(o => o.Id == observacion.ObjetoEspacialId);

            if (objeto == null)
            {
                return NotFound("El objeto espacial no existe.");
            }

            observacion.Id = observaciones.Count + 1;

            observaciones.Add(observacion);

            return Ok(observacion);
        }

        [HttpGet("{id}/observations")]
        public IActionResult GetObservations(int id)
        {
            List<Observacion> resultado = observaciones
                .Where(o => o.ObjetoEspacialId == id)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("search")]
        public IActionResult SearchByName(string nombre)
        {
            List<ObjetoEspacial> resultado = objetos
                .Where(o => o.Nombre.ToLower().Contains(nombre.ToLower()))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("type/{tipo}")]
        public IActionResult FilterByType(string tipo)
        {
            List<ObjetoEspacial> resultado = objetos
                .Where(o => o.Tipo.ToLower() == tipo.ToLower())
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("risk/{nivel}")]
        public IActionResult FilterByRisk(int nivel)
        {
            List<ObjetoEspacial> resultado = objetos
                .Where(o => o.NivelRiesgo >= nivel)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("distance/{distancia}")]
        public IActionResult GetByDistance(double distancia)
        {
            List<ObjetoEspacial> resultado = objetos
                .Where(o => o.Distancia < distancia)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("observations/average-speed")]
        public IActionResult GetAverageSpeed()
        {
            if (observaciones.Count == 0)
            {
                return BadRequest("No hay observaciones.");
            }

            double promedio = observaciones.Average(o => o.Velocidad);

            return Ok(promedio);
        }

        [HttpGet("most-observed")]
        public IActionResult GetMostObserved()
        {
            var resultado = objetos
                .Select(o => new
                {
                    Objeto = o,
                    Cantidad = observaciones.Count(x => x.ObjetoEspacialId == o.Id)
                })
                .OrderByDescending(o => o.Cantidad)
                .FirstOrDefault();

            return Ok(resultado);
        }

        [HttpGet("without-observations")]
        public IActionResult GetWithoutObservations()
        {
            List<ObjetoEspacial> resultado = objetos
                .Where(o => !observaciones.Any(x => x.ObjetoEspacialId == o.Id))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("ordered-by-risk")]
        public IActionResult GetOrderedByRisk()
        {
            List<ObjetoEspacial> resultado = objetos
                .OrderByDescending(o => o.NivelRiesgo)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("alertas")]
        public IActionResult GetAlerts(int distancia = 1000, int riesgo = 7)
        {
            List<ObjetoEspacial> resultado = objetos
                .Where(o =>
                    o.NivelRiesgo >= riesgo &&
                    o.Distancia < distancia &&
                    o.Activo == true)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var resultado = new
            {
                TotalObjetos = objetos.Count,
                TotalObservaciones = observaciones.Count,
                ObjetosActivos = objetos.Count(o => o.Activo),
                RiesgoPromedio = objetos.Count > 0
                    ? objetos.Average(o => o.NivelRiesgo)
                    : 0,
                VelocidadPromedio = observaciones.Count > 0
                    ? observaciones.Average(o => o.Velocidad)
                    : 0
            };

            return Ok(resultado);
        }
    }
}