using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NuvemPessoalApp.Models;

namespace NuvemPessoalApp.Controllers
{
    public class HomeController : Controller
    {
        IWebHostEnvironment _appEnvironment;

        public HomeController(IWebHostEnvironment env)
        {
            _appEnvironment = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult ListFiles(string tipo)
        {
            string pasta = tipo switch
            {
                "I" => "image",
                "V" => "video",
                "M" => "music",
                "D" => "document",
                _ => "document",
            };

            string caminho_WebRoot = _appEnvironment.WebRootPath;
            string caminhoDestinoArquivo = caminho_WebRoot + "\\files\\" + pasta + "\\";

            List<string> arquivos = new List<string>();

            foreach (var file in Directory.GetFiles(caminhoDestinoArquivo))
            {
                arquivos.Add("/files/" + pasta + "/" + new FileInfo(file).Name);
            }

            return Json(arquivos.ToArray());
        }

        public JsonResult DeleteFile(string fileName)
        {
            string[] splited = fileName.Split('/');
            string name = splited[1];
            string pasta = splited[0];
            string caminho_WebRoot = _appEnvironment.WebRootPath;
            string caminhoDestinoArquivo = caminho_WebRoot + "\\files\\" + pasta + "\\" + name ;

            System.IO.File.Delete(caminhoDestinoArquivo); 

            return Json(caminhoDestinoArquivo);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            foreach (var file in files)
            {

                string nomeArquivo = file.FileName;
                string pasta = "";
                string ext = new FileInfo(file.FileName).Extension.ToLower();

                switch (ext)
                {
                    case ".png":
                    case ".jpeg":
                    case ".jpg":
                        pasta = "image";
                        break;
                    case ".mp3":
                        pasta = "music";
                        break;
                    case ".mp4":
                    case ".mov":
                    case ".3gp":
                    case ".avi":
                    case ".wmv":
                    case ".flv":
                        pasta = "video";
                        break;
                    default:
                        pasta = "document";
                        break;
                }

                string caminho_WebRoot = _appEnvironment.WebRootPath;
                string caminhoDestinoArquivo = caminho_WebRoot + "\\files\\" + pasta + "\\";
                string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                using var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                await file.CopyToAsync(stream);


                var client = new MongoClient("mongodb://localhost:27017/?readPreference=primary&ssl=false");
                var database = client.GetDatabase("nuvem");
                var _files = database.GetCollection<Files>("files");

                _files.InsertOne(new Files
                {
                    nome = nomeArquivo.Replace(ext, ""),
                    caminho_original = caminhoDestinoArquivoOriginal,
                    caminho_local = caminhoDestinoArquivo,
                    extensao = ext
                });
            }

            return RedirectToAction("Index");
        }
    }

    class Files
    {
        public string nome;
        public string caminho_original;
        public string caminho_local;
        public string extensao;
    }
}
