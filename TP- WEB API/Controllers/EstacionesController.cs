using Microsoft.AspNetCore.Mvc;

namespace EstacionesMeteorologicas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstacionesController : ControllerBase
    {
        private static List<Estacion> estaciones = new List<Estacion>()
        {
            new Estacion
            {
                Id = 1,
                Nombre = "Estacion del Centro",
                Localidad = "Cordoba",
                Activa = true
            },

            new Estacion
            {
                Id = 2,
                Nombre = "Estacion del Norte",
                Localidad = "Villa Allende",
                Activa = true
            }
        };

        private static List<Medicion> mediciones = new List<Medicion>()
        {
            new Medicion
            {
                Id = 1,
                EstacionId = 1,
                Temperatura = 25,
                Humedad = 60,
                VelocidadViento = 10,
                FechaHora = new DateTime(2026, 3, 17, 10, 0, 0)
            },

            new Medicion
            {
                Id = 2,
                EstacionId = 1,
                Temperatura = 30,
                Humedad = 50,
                VelocidadViento = 15,
                FechaHora = new DateTime(2026, 6, 22, 14, 0, 0)
            },

            new Medicion
            {
                Id = 3,
                EstacionId = 2,
                Temperatura = 20,
                Humedad = 70,
                VelocidadViento = 8,
                FechaHora = new DateTime(2026, 9, 25, 12, 0, 0)
            }
        };

        [HttpPost("estaciones")]
        public IActionResult CreateStation(Estacion estacion)
        {
            estacion.Id = estaciones.Count + 1;

            estaciones.Add(estacion);

            return Ok(estacion);
        }

        [HttpPost("mediciones")]
        public IActionResult CreateMeasurement(Medicion medicion)
        {
            Estacion estacion = estaciones.FirstOrDefault(e => e.Id == medicion.EstacionId);

            if (estacion == null)
            {
                return NotFound("La estación no existe.");
            }

            if (estacion.Activa == false)
            {
                return BadRequest("La estación está inactiva.");
            }

            if (medicion.Humedad < 0 || medicion.Humedad > 100)
            {
                return BadRequest("La humedad debe estar entre 0 y 100.");
            }

            if (medicion.VelocidadViento < 0)
            {
                return BadRequest("La velocidad del viento no puede ser negativa.");
            }

            if (medicion.FechaHora > DateTime.Now)
            {
                return BadRequest("La fecha no puede ser futura.");
            }

            medicion.Id = mediciones.Count + 1;

            mediciones.Add(medicion);

            return Ok(medicion);
        }

        [HttpGet("mediciones")]
        public IActionResult GetAll()
        {
            return Ok(mediciones);
        }

        [HttpGet("mediciones/estacion/{id}")]
        public IActionResult GetByStation(int id)
        {
            List<Medicion> resultado = mediciones
                .Where(m => m.EstacionId == id)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("estaciones/localidad/{localidad}")]
        public IActionResult SearchByLocation(string localidad)
        {
            List<Estacion> resultado = estaciones
                .Where(e => e.Localidad.ToLower() == localidad.ToLower())
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("mediciones/temperatura/{temperatura}")]
        public IActionResult GetHigherTemperatures(double temperatura)
        {
            List<Medicion> resultado = mediciones
                .Where(m => m.Temperatura > temperatura)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("mediciones/ordenadas")]
        public IActionResult GetOrderedMeasurements()
        {
            List<Medicion> resultado = mediciones
                .OrderByDescending(m => m.Temperatura)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("mediciones/promedio")]
        public IActionResult GetAverageTemperature()
        {
            if (mediciones.Count == 0)
            {
                return BadRequest("No hay mediciones.");
            }

            double promedio = mediciones.Average(m => m.Temperatura);

            return Ok(promedio);
        }

        [HttpGet("mediciones/maxima")]
        public IActionResult GetMaximumTemperature()
        {
            if (mediciones.Count == 0)
            {
                return BadRequest("No hay mediciones.");
            }

            double maxima = mediciones.Max(m => m.Temperatura);

            return Ok(maxima);
        }

        [HttpGet("mediciones/minima")]
        public IActionResult GetMinimumTemperature()
        {
            if (mediciones.Count == 0)
            {
                return BadRequest("No hay mediciones.");
            }

            double minima = mediciones.Min(m => m.Temperatura);

            return Ok(minima);
        }

        [HttpGet("mediciones/cantidad")]
        public IActionResult GetMeasurementCount()
        {
            var resultado = estaciones.Select(e => new
            {
                Estacion = e.Nombre,
                Cantidad = mediciones.Count(m => m.EstacionId == e.Id)
            }).ToList();

            return Ok(resultado);
        }

        [HttpGet("estaciones/existe/{id}")]
        public IActionResult StationExists(int id)
        {
            bool existe = estaciones.Any(e => e.Id == id);

            return Ok(existe);
        }
    }
}