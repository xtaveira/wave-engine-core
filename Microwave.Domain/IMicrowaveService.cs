using System;
using Microwave.Domain.DTOs;

namespace Microwave.Domain
{
    public interface IMicrowaveService
    {
        OperationResult StartHeating(int timeInSeconds, int powerLevel, IStateStorage stateStorage);
        MicrowaveStatus GetHeatingProgress(IStateStorage stateStorage);
        OperationResult StartQuickHeat(IStateStorage stateStorage);
        OperationResult IncreaseTime(int additionalSeconds, IStateStorage stateStorage);
        OperationResult PauseOrCancel(IStateStorage stateStorage);

        IEnumerable<PredefinedProgram> GetPredefinedPrograms();
        OperationResult StartPredefinedProgram(string programName, IStateStorage stateStorage);

        // Level 3 - Custom Programs
        Task<OperationResult> StartCustomProgramAsync(Guid customProgramId, IStateStorage stateStorage);
        Task<IEnumerable<ProgramDisplayInfo>> GetAllProgramsAsync();
        Task<CustomProgram?> GetCustomProgramAsync(Guid id);
    }
}
