﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VCC_Projekt.Controllers
{
    [Route("api/v1/files")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public FileController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{levelId}")]
        public async Task<IActionResult> DownloadFile(int levelId)
        {
            // Get the file from the database
            var file = await _dbContext.Levels
                .Where(l => l.LevelID == levelId)
                .Select(l => l.Angabe_PDF)
                .FirstOrDefaultAsync();
            if (file == null)
            {
                return NotFound();
            }
            return File(file, "application/pdf");
        }

        [HttpGet("{levelId}/{aufgabeId}/input")]
        public async Task<IActionResult> DownloadFile(int levelId, int aufgabeId)
        {
            // Get the file from the database
            var file = await _dbContext.Aufgabe
                .Where(l => l.Level_LevelID == levelId && l.AufgabenID==aufgabeId)
                .Select(l => l.Input_TXT)
                .FirstOrDefaultAsync();
            if (file == null)
            {
                return NotFound();
            }
            return File(file, "plain/text");
        }
    }
}
