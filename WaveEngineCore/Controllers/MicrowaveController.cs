using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microwave.Application;
using Microwave.Domain;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using WaveEngineCore.Infrastructure;

namespace WaveEngineCore.Controllers;

/// <summary>
/// Controller principal da API do micro-ondas, responsável por todas as operações de aquecimento e gerenciamento de programas
/// </summary>
[Authorize]
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

    /// <summary>
    /// Inicia aquecimento manual com tempo e potência específicos
    /// </summary>
    /// <param name="request">Parâmetros de aquecimento (tempo em segundos, potência 1-10)</param>
    /// <returns>Confirmação de início do aquecimento</returns>
    [HttpPost("heating/start")]
    public IActionResult StartHeating([FromBody] StartHeatingRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var result = _microwaveService.StartHeating(request.TimeInSeconds, request.PowerLevel, stateStorage);

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

    /// <summary>
    /// Inicia aquecimento rápido (30 segundos na potência máxima)
    /// </summary>
    /// <returns>Confirmação de início do aquecimento rápido</returns>
    [HttpPost("heating/quick")]
    public IActionResult StartQuickHeating()
    {
        try
        {
            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var result = _microwaveService.StartQuickHeat(stateStorage);

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

    [HttpPost("heating/pause")]
    public IActionResult PauseHeating()
    {
        try
        {
            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var result = _microwaveService.PauseOrCancel(stateStorage);

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

    [HttpPost("heating/cancel")]
    public IActionResult CancelHeating()
    {
        try
        {
            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var result = _microwaveService.PauseOrCancel(stateStorage);

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

    /// <summary>
    /// Adiciona tempo extra ao aquecimento em andamento (apenas para aquecimento manual)
    /// </summary>
    /// <param name="request">Tempo adicional em segundos</param>
    /// <returns>Confirmação de tempo adicionado</returns>
    [HttpPost("heating/add-time")]
    public IActionResult AddTime([FromBody] AddTimeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var result = _microwaveService.IncreaseTime(request.AdditionalSeconds, stateStorage);

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

    /// <summary>
    /// Obtém o status atual do aquecimento em tempo real
    /// </summary>
    /// <returns>Informações detalhadas sobre o estado do micro-ondas</returns>
    [HttpGet("heating/status")]
    public IActionResult GetHeatingStatus()
    {
        try
        {
            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var status = _microwaveService.GetHeatingProgress(stateStorage);

            var response = new HeatingStatusResponse
            {
                IsRunning = status.IsRunning,
                RemainingTime = status.RemainingTime,
                PowerLevel = status.PowerLevel,
                Progress = status.Progress,
                CurrentState = status.IsRunning ? "HEATING" : status.RemainingTime > 0 ? "PAUSED" : "STOPPED",
                HeatingChar = stateStorage.GetString("HeatingChar"),
                CurrentProgram = stateStorage.GetString("CurrentProgram"),
                ProgressDisplay = status.StatusMessage,
                StartTime = !string.IsNullOrEmpty(stateStorage.GetString("StartTime"))
                    ? DateTime.Parse(stateStorage.GetString("StartTime")!)
                    : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpGet("programs/predefined")]
    public IActionResult GetPredefinedPrograms()
    {
        try
        {
            var programs = _microwaveService.GetPredefinedPrograms();
            return Ok(programs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    [HttpPost("programs/predefined/{name}/start")]
    public IActionResult StartPredefinedProgram(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Nome do programa é obrigatório" });
            }

            var stateStorage = new SessionStateStorage(HttpContext.Session);
            var result = _microwaveService.StartPredefinedProgram(name, stateStorage);

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