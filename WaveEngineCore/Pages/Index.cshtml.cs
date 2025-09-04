using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Domain;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using WaveEngineCore.Infrastructure;

namespace WaveEngineCore.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMicrowaveService _microwaveService;
    private readonly IProgramDisplayService _programDisplayService;

    public IEnumerable<PredefinedProgram> PredefinedPrograms { get; set; } = new List<PredefinedProgram>();
    public IEnumerable<ProgramDisplayInfo> AllPrograms { get; set; } = new List<ProgramDisplayInfo>();

    public IndexModel(ILogger<IndexModel> logger, IMicrowaveService microwaveService, IProgramDisplayService programDisplayService)
    {
        _logger = logger;
        _microwaveService = microwaveService;
        _programDisplayService = programDisplayService;
    }

    public async Task OnGetAsync()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        PredefinedPrograms = _microwaveService.GetPredefinedPrograms();
        AllPrograms = await _microwaveService.GetAllProgramsAsync();
    }

    public IActionResult OnPostStartHeating(int timeInSeconds, int powerLevel)
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.StartHeating(timeInSeconds, powerLevel, stateStorage);
        ViewData["Message"] = result.Message;

        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        PredefinedPrograms = _microwaveService.GetPredefinedPrograms();
        AllPrograms = _microwaveService.GetAllProgramsAsync().Result;

        return Page();
    }

    public IActionResult OnPostStartQuickHeat()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.StartQuickHeat(stateStorage);
        ViewData["Message"] = result.Message;

        PredefinedPrograms = _microwaveService.GetPredefinedPrograms();
        AllPrograms = _microwaveService.GetAllProgramsAsync().Result;

        return Page();
    }

    public IActionResult OnPostIncreaseTime()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.IncreaseTime(30, stateStorage);
        ViewData["Message"] = result.Message;

        PredefinedPrograms = _microwaveService.GetPredefinedPrograms();
        AllPrograms = _microwaveService.GetAllProgramsAsync().Result;

        return Page();
    }

    public IActionResult OnPostPauseOrCancel()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.PauseOrCancel(stateStorage);
        ViewData["Message"] = result.Message;

        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        PredefinedPrograms = _microwaveService.GetPredefinedPrograms();
        AllPrograms = _microwaveService.GetAllProgramsAsync().Result;

        return Page();
    }

    public IActionResult OnPostResumeHeating()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.StartHeating(0, 0, stateStorage);
        ViewData["Message"] = result.Message;

        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        PredefinedPrograms = _microwaveService.GetPredefinedPrograms();
        AllPrograms = _microwaveService.GetAllProgramsAsync().Result;

        return Page();
    }

    public IActionResult OnPostStartPredefinedProgram(string programName)
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.StartPredefinedProgram(programName, stateStorage);
        ViewData["Message"] = result.Message;

        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        PredefinedPrograms = _microwaveService.GetPredefinedPrograms();
        AllPrograms = _microwaveService.GetAllProgramsAsync().Result;

        return Page();
    }

    public IActionResult OnPostGetProgress()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var status = _microwaveService.GetHeatingProgress(stateStorage);
        var currentProgram = stateStorage.GetString("CurrentProgram");

        return new JsonResult(new
        {
            progress = status.StatusMessage,
            isRunning = status.IsRunning,
            remainingTime = status.RemainingTime,
            formattedRemainingTime = status.FormattedRemainingTime,
            powerLevel = status.PowerLevel,
            progressPercent = status.Progress,
        });
    }
}
