using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace LauGardensApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagensController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        //Injeção de IWebHostEnvironment
        public ImagensController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // POST: api/imagens/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImagem(IFormFile ficheiro)
        {
            if (ficheiro == null || ficheiro.Length == 0)
            {
                return BadRequest("Nenhum ficheiro foi enviado.");
            }

            //Caminho Físico da Raiz C:\...\LauGardens\Frontend
            var webRootPath = _webHostEnvironment.WebRootPath;

            //Caminho para a pasta 'img' 
            var pastaDestino = Path.Combine(webRootPath, "img");

            //Garante que a pasta 'img' existe
            if (!Directory.Exists(pastaDestino))
            {
                Directory.CreateDirectory(pastaDestino);
            }

            //Cria nome de ficheiro único para evitar problemas
            var extensao = Path.GetExtension(ficheiro.FileName);
            var nomeFicheiroUnico = Guid.NewGuid().ToString() + extensao;
            
            //Caminho físico completo onde o ficheiro será gravado
            var caminhoGravarCompleto = Path.Combine(pastaDestino, nomeFicheiroUnico);

            //Grava o ficheiro
            using (var stream = new FileStream(caminhoGravarCompleto, FileMode.Create))
            {
                await ficheiro.CopyToAsync(stream);
            }

            //Devolve o caminho URL público para a base de dados Ex: /img/1234abcd.png
            var urlPublica = Path.Combine("/img", nomeFicheiroUnico).Replace('\\', '/');

            return Ok(new { UrlImagem = urlPublica });
        }
    }
}
