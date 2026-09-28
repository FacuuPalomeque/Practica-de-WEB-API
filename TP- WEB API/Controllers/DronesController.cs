using Microsoft.AspNetCore.Mvc;
namespace CentroDrones.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DronesController : ControllerBase
    {
        private static List<Drone> drones = new List<Drone>()
        {
            new Drone
            {
                Id = 1,
                Codigo = "Dron-001",
                Modelo = "DR Mini",
                Bateria = 20,
                Estado = "Disponible"
            },

            new Drone
            {
                Id = 2,
                Codigo = "Dron-002",
                Modelo = "DR Max",
                Bateria = 80,
                Estado = "Disponible"
            }
        };

        private static List<Mision> misiones = new List<Mision>();

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(drones);
        }

        [HttpGet("available")]
        public IActionResult GetAvailable()
        {
            List<Drone> resultado = drones
                .Where(d => d.Estado == "Disponible")
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("low-battery/{porcentaje}")]
        public IActionResult GetLowBattery(double porcentaje)
        {
            List<Drone> resultado = drones
                .Where(d => d.Bateria < porcentaje)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("model/{modelo}")]
        public IActionResult GetByModel(string modelo)
        {
            List<Drone> resultado = drones
                .Where(d => d.Modelo.ToLower() == modelo.ToLower())
                .ToList();

            return Ok(resultado);
        }

        [HttpPost("mission")]
        public IActionResult AssignMission(Mision mision)
        {
            Drone drone = drones.FirstOrDefault(d => d.Id == mision.DroneId);

            if (drone == null)
            {
                return NotFound("El drone no existe.");
            }

            if (drone.Estado != "Disponible")
            {
                return BadRequest("El drone no está disponible.");
            }

            if (drone.Bateria < 30)
            {
                return BadRequest("El drone no tiene suficiente batería.");
            }

            bool tieneMision = misiones.Any(m =>
                m.DroneId == mision.DroneId &&
                m.Completada == false);

            if (tieneMision)
            {
                return BadRequest("El drone ya tiene una misión activa.");
            }

            mision.Id = misiones.Count + 1;
            mision.Completada = false;

            misiones.Add(mision);

            drone.Estado = "EnMision";

            return Ok(mision);
        }

        [HttpPut("mission/{id}/finish")]
        public IActionResult FinishMission(int id)
        {
            Mision mision = misiones.FirstOrDefault(m => m.Id == id);

            if (mision == null)
            {
                return NotFound("La misión no existe.");
            }

            if (mision.Completada)
            {
                return BadRequest("La misión ya fue finalizada.");
            }

            mision.Completada = true;

            Drone drone = drones.FirstOrDefault(d => d.Id == mision.DroneId);

            if (drone != null)
            {
                drone.Estado = "Disponible";
            }

            return Ok(mision);
        }

        [HttpGet("{id}/missions")]
        public IActionResult GetMissionsByDrone(int id)
        {
            List<Mision> resultado = misiones
                .Where(m => m.DroneId == id)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("total-distance")]
        public IActionResult GetTotalDistance()
        {
            var resultado = drones.Select(d => new
            {
                Drone = d.Codigo,
                DistanciaTotal = misiones
                    .Where(m => m.DroneId == d.Id && m.Completada)
                    .Sum(m => m.DistanciaKm)
            }).ToList();

            return Ok(resultado);
        }

        [HttpGet("top-distance")]
        public IActionResult GetTopDistance()
        {
            var resultado = drones.Select(d => new
            {
                Drone = d,
                DistanciaTotal = misiones
                    .Where(m => m.DroneId == d.Id && m.Completada)
                    .Sum(m => m.DistanciaKm)
            })
            .OrderByDescending(d => d.DistanciaTotal)
            .FirstOrDefault();

            return Ok(resultado);
        }

        [HttpGet("available/average-battery")]
        public IActionResult GetAverageBattery()
        {
            List<Drone> disponibles = drones
                .Where(d => d.Estado == "Disponible")
                .ToList();

            if (disponibles.Count == 0)
            {
                return BadRequest("No hay drones disponibles.");
            }

            double promedio = disponibles.Average(d => d.Bateria);

            return Ok(promedio);
        }

        [HttpGet("pending-missions")]
        public IActionResult GetPendingMissions()
        {
            List<Mision> resultado = misiones
                .Where(m => m.Completada == false)
                .OrderBy(m => m.Fecha)
                .ToList();

            return Ok(resultado);
        }
    }
}