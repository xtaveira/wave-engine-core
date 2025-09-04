using Microsoft.AspNetCore.Mvc;
using Microwave.Application;
using Microwave.Domain;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using WaveEngineCore.Infrastructure;

namespace WaveEngineCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MicrowaveController : ControllerBase
{
    private readonly IMicrowaveService _microwaveService;
    private readonly CustomProgramService _customProgramService;
    private readonly IProgramDisplayService _programDisplayService;

    public MicrowaveController(
        IMicrowaveService microwaveService,
        CustomProgramService customProgramService,
        IProgramDisplayService programDisplayService)
    {
        _microwaveService = microwaveService;
        _customProgramService = customProgramService;
        _programDisplayService = programDisplayService;
    }

    [HttpGet("programs")]
    public async Task<IActionResult> GetAllPrograms()
    {
        try
        {
            var programs = await _microwaveService.GetAllProgramsAsync();
            return Ok(programs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpGet("programs/custom")]
    public async Task<IActionResult> GetCustomPrograms()
    {
        try
        {
            var programs = await _programDisplayService.GetCustomProgramsAsync();
            return Ok(programs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpGet("programs/custom/{id:guid}")]
    public async Task<IActionResult> GetCustomProgram(Guid id)
    {
        try
        {
            var program = await _customProgramService.GetByIdAsync(id);
            if (program == null)
            {
                return NotFound(new { message = "Programa não encontrado" });
            }
            return Ok(program);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpPost("programs/custom")]
    public async Task<IActionResult> CreateCustomProgram([FromBody] CreateCustomProgramRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var program = new CustomProgram(
                request.Name,
                request.Food,
                request.PowerLevel,
                request.TimeInSeconds,
                request.Character,
                request.Instructions ?? string.Empty
            );

            var result = await _customProgramService.CreateAsync(program);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return CreatedAtAction(nameof(GetCustomProgram), new { id = result.Data!.Id }, result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpPut("programs/custom/{id:guid}")]
    public async Task<IActionResult> UpdateCustomProgram(Guid id, [FromBody] UpdateCustomProgramRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProgram = await _customProgramService.GetByIdAsync(id);
            if (existingProgram == null)
            {
                return NotFound(new { message = "Programa não encontrado" });
            }

            var updatedProgram = new CustomProgram(
                request.Name,
                request.Food,
                request.PowerLevel,
                request.TimeInSeconds,
                request.Character,
                request.Instructions ?? string.Empty
            )
            {
                Id = id,
                CreatedAt = existingProgram.CreatedAt
            };

            var result = await _customProgramService.UpdateAsync(updatedProgram);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpDelete("programs/custom/{id:guid}")]
    public async Task<IActionResult> DeleteCustomProgram(Guid id)
    {
        try
        {
            var result = await _customProgramService.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "PROGRAM_NOT_FOUND")
                {
                    return NotFound(new { message = result.Message });
                }
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpPost("programs/custom/{id:guid}/start")]
    public async Task<IActionResult> StartCustomProgram(Guid id)
    {
        try
        {
            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var result = await _microwaveService.StartCustomProgramAsync(id, stateStorage);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpGet("characters/used")]
    public async Task<IActionResult> GetUsedCharacters()
    {
        try
        {
            var usedCharacters = await _programDisplayService.GetUsedCharactersAsync();
            return Ok(new { usedCharacters = usedCharacters.ToArray() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpGet("characters/{character}/unique")]
    public async Task<IActionResult> IsCharacterUnique(char character, [FromQuery] string? excludeId = null)
    {
        try
        {
            bool isUnique;
            if (!string.IsNullOrEmpty(excludeId))
            {
                isUnique = await _programDisplayService.IsCharacterUniqueAsync(character, excludeId);
            }
            else
            {
                isUnique = await _programDisplayService.IsCharacterUniqueAsync(character);
            }

            return Ok(new { character, isUnique });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}

public class CreateCustomProgramRequest
{
    public string Name { get; set; } = string.Empty;
    public string Food { get; set; } = string.Empty;
    public int PowerLevel { get; set; }
    public int TimeInSeconds { get; set; }
    public char Character { get; set; }
    public string? Instructions { get; set; }
}

public class UpdateCustomProgramRequest
{
    public string Name { get; set; } = string.Empty;
    public string Food { get; set; } = string.Empty;
    public int PowerLevel { get; set; }
    public int TimeInSeconds { get; set; }
    public char Character { get; set; }
    public string? Instructions { get; set; }
}