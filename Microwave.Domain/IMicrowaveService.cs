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
    }
}
