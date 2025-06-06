﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace VCC_Projekt.Controllers
{
    [EnableCors("AllowSpecificOrigin")]
    [Route("api/files")]
    [ApiController]
    [Authorize(Roles = "Admin,Editor,Benutzer")]
    public class FileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public FileController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private bool IsRequestFromAllowedWebsite()
        {
            var referer = Request.Headers["Referer"].ToString();

            // Liste unerlaubter Begriffe
            string[] blockedKeywords = { "account", "returnurl" };

            return !string.IsNullOrWhiteSpace(referer) &&
                   !blockedKeywords.Any(keyword => referer.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        [HttpGet("{levelId}")]
        public async Task<IActionResult> DownloadFile(int levelId)
        {
            if (!IsRequestFromAllowedWebsite())
            {
                return Forbid();
            }
            // Get the file from the database
            var file = await _dbContext.Levels
                .Where(l => l.LevelID == levelId)
                .Select(l => l.Angabe_PDF)
                .FirstOrDefaultAsync();
            if (file == null) return NotFound();

            return File(file, "application/pdf");
        }

        [HttpGet("{levelId}/{aufgabeId}/input")]
        public async Task<IActionResult> DownloadInputFile(int levelId, int aufgabeId)
        {
            if (!IsRequestFromAllowedWebsite())
            {
                return Forbid();
            }
            // Get the file from the database
            var file = await _dbContext.Aufgabe
                .Where(l => l.Level_LevelID == levelId && l.AufgabenID == aufgabeId)
                .Select(l => l.Input_TXT)
                .FirstOrDefaultAsync();
            if (file == null) return NotFound();

            return File(file, "plain/text");
        }

        [HttpGet("{levelId}/{aufgabeId}/result")]
        public async Task<IActionResult> DownloadErgebnisFile(int levelId, int aufgabeId)
        {
            if (!IsRequestFromAllowedWebsite())
            {
                return Forbid();
            }
            var file = await _dbContext.Aufgabe
                .Where(l => l.Level_LevelID == levelId && l.AufgabenID == aufgabeId)
                .Select(l => l.Ergebnis_TXT)
                .FirstOrDefaultAsync();
            if (file == null) return NotFound();

            return File(file, "plain/text");
        }

        [HttpGet("{levelId}/input/zip")]
        public async Task<IActionResult> DownloadFiles(int levelId)
        {
            if (!IsRequestFromAllowedWebsite())
            {
                return Forbid();
            }
            var files = await _dbContext.Aufgabe
                                        .Where(l => l.Level_LevelID == levelId)
                                        .Select(l => new { l.Input_TXT, l.Level.Levelnr, l.Aufgabennr })
                                        .OrderBy(a => a.Aufgabennr)
                                        .ToListAsync();

            if (files == null || files.Count == 0) return NotFound();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true, System.Text.Encoding.UTF8))
                {
                    foreach (var file in files)
                    {
                        var zipArchiveEntry = archive.CreateEntry($"level{file.Levelnr}_{file.Aufgabennr}.txt");
                        using (var zipStream = zipArchiveEntry.Open())
                        {
                            await zipStream.WriteAsync(file.Input_TXT, 0, file.Input_TXT.Length);
                        }
                    }
                }
                return File(memoryStream.ToArray(), "application/zip");
            }
        }
    }
}
