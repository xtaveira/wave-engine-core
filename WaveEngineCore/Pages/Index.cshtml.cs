using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microwave.Domain;
using WaveEngineCore.Infrastructure;

namespace WaveEngineCore.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMicrowaveService _microwaveService;

    public IndexModel(ILogger<IndexModel> logger, IMicrowaveService microwaveService)
    {
        _logger = logger;
        _microwaveService = microwaveService;
    }

    public void OnGet()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;
    }

    public IActionResult OnPostStartHeating(int timeInSeconds, int powerLevel)
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.StartHeating(timeInSeconds, powerLevel, stateStorage);
        ViewData["Message"] = result.Message;

        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        return Page();
    }

    public IActionResult OnPostStartQuickHeat()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.StartQuickHeat(stateStorage);
        ViewData["Message"] = result.Message;
        return Page();
    }

    public IActionResult OnPostIncreaseTime()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.IncreaseTime(30, stateStorage);
        ViewData["Message"] = result.Message;
        return Page();
    }

    public IActionResult OnPostPauseOrCancel()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.PauseOrCancel(stateStorage);
        ViewData["Message"] = result.Message;

        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        return Page();
    }

    public IActionResult OnPostResumeHeating()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var result = _microwaveService.StartHeating(0, 0, stateStorage);
        ViewData["Message"] = result.Message;

        var status = _microwaveService.GetHeatingProgress(stateStorage);
        ViewData["CurrentStatus"] = status;

        return Page();
    }

    public IActionResult OnPostGetProgress()
    {
        var stateStorage = new SessionStateStorage(HttpContext.Session);
        var status = _microwaveService.GetHeatingProgress(stateStorage);
        return new JsonResult(new
        {
            progress = status.StatusMessage,
            isRunning = status.IsRunning,
            remainingTime = status.RemainingTime,
            formattedRemainingTime = status.FormattedRemainingTime,
            powerLevel = status.PowerLevel,
            progressPercent = status.Progress
        });
    }
}
