using Microsoft.AspNetCore.Mvc;

namespace ObjetosPerdidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjetosPerdidosController : ControllerBase
    {
        private static List<ObjetoPerdido> objetos = new List<ObjetoPerdido>()
        {
            new ObjetoPerdido
            {
                Id = 1,
                Descripcion = "Mochila negra",
                Categoria = "Mochilas",
                LugarEncontrado = "Nuevo Centro Shopping",
                FechaEncontrado = new DateTime(2026, 5, 12),
                Reclamado = false,
                NombrePersonaQueRetiro = ""
            },

            new ObjetoPerdido
            {
                Id = 2,
                Descripcion = "Celular Samsung",
                Categoria = "Electronica",
                LugarEncontrado = "Colectivo",
                FechaEncontrado = new DateTime(2026, 6, 21),
                Reclamado = true,
                NombrePersonaQueRetiro = "Mati Perez"
            }
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(objetos);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            ObjetoPerdido objeto = objetos.FirstOrDefault(o => o.Id == id);

            if (objeto == null)
            {
                return NotFound("No se encontró el objeto.");
            }

            return Ok(objeto);
        }

        [HttpPost]
        public IActionResult Create(ObjetoPerdido objeto)
        {
            if (objeto.Descripcion == "")
            {
                return BadRequest("La descripción no puede estar vacía.");
            }

            if (objeto.FechaEncontrado > DateTime.Now)
            {
                return BadRequest("La fecha no puede ser futura.");
            }

            if (objeto.Reclamado && objeto.NombrePersonaQueRetiro == "")
            {
                return BadRequest("Debe indicar quién retiró el objeto.");
            }

            objeto.Id = objetos.Count + 1;

            objetos.Add(objeto);

            return Ok(objeto);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, ObjetoPerdido objetoModificado)
        {
            ObjetoPerdido objeto = objetos.FirstOrDefault(o => o.Id == id);

            if (objeto == null)
            {
                return NotFound("No se encontró el objeto.");
            }

            if (objetoModificado.Descripcion == "")
            {
                return BadRequest("La descripción no puede estar vacía.");
            }

            if (objetoModificado.FechaEncontrado > DateTime.Now)
            {
                return BadRequest("La fecha no puede ser futura.");
            }

            objeto.Descripcion = objetoModificado.Descripcion;
            objeto.Categoria = objetoModificado.Categoria;
            objeto.LugarEncontrado = objetoModificado.LugarEncontrado;
            objeto.FechaEncontrado = objetoModificado.FechaEncontrado;
            objeto.Reclamado = objetoModificado.Reclamado;
            objeto.NombrePersonaQueRetiro = objetoModificado.NombrePersonaQueRetiro;

            return Ok(objeto);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            ObjetoPerdido objeto = objetos.FirstOrDefault(o => o.Id == id);

            if (objeto == null)
            {
                return NotFound("No se encontró el objeto.");
            }

            objetos.Remove(objeto);

            return Ok("Objeto eliminado correctamente.");
        }

        [HttpGet("search")]
        public IActionResult SearchByDescription(string descripcion)
        {
            List<ObjetoPerdido> resultado = objetos
                .Where(o => o.Descripcion.ToLower().Contains(descripcion.ToLower()))
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("category/{categoria}")]
        public IActionResult FilterByCategory(string categoria)
        {
            List<ObjetoPerdido> resultado = objetos
                .Where(o => o.Categoria.ToLower() == categoria.ToLower())
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("unclaimed")]
        public IActionResult GetUnclaimed()
        {
            List<ObjetoPerdido> resultado = objetos
                .Where(o => o.Reclamado == false)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("after-date")]
        public IActionResult GetAfterDate(DateTime fecha)
        {
            List<ObjetoPerdido> resultado = objetos
                .Where(o => o.FechaEncontrado > fecha)
                .ToList();

            return Ok(resultado);
        }

        [HttpGet("sorted")]
        public IActionResult GetSorted()
        {
            List<ObjetoPerdido> resultado = objetos
                .OrderByDescending(o => o.FechaEncontrado)
                .ToList();

            return Ok(resultado);
        }

        [HttpPut("claim/{id}")]
        public IActionResult ClaimObject(int id, string nombrePersona)
        {
            ObjetoPerdido objeto = objetos.FirstOrDefault(o => o.Id == id);

            if (objeto == null)
            {
                return NotFound("No se encontró el objeto.");
            }

            if (objeto.Reclamado)
            {
                return BadRequest("El objeto ya fue reclamado.");
            }

            if (nombrePersona == "")
            {
                return BadRequest("Debe indicar quién retiró el objeto.");
            }

            objeto.Reclamado = true;
            objeto.NombrePersonaQueRetiro = nombrePersona;

            return Ok(objeto);
        }

        [HttpGet("exists-category/{categoria}")]
        public IActionResult ExistsCategory(string categoria)
        {
            bool exists = objetos.Any(o => o.Categoria.ToLower() == categoria.ToLower());

            return Ok(exists);
        }

        [HttpGet("count")]
        public IActionResult GetCount()
        {
            int count = objetos.Count();

            return Ok(count);
        }
    }
}