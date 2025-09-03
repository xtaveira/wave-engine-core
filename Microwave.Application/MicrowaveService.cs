using Microwave.Domain;
using Microwave.Domain.DTOs;
using System.Text.Json;

namespace Microwave.Application
{
    public class MicrowaveService : IMicrowaveService
    {
        private const string SESSION_CURRENT_OVEN = "CurrentOven";
        private const string SESSION_IS_HEATING = "IsHeating";
        private const string SESSION_START_TIME = "StartTime";
        private const string SESSION_MICROWAVE_STATE = "MicrowaveState";
        private const string SESSION_PAUSED_REMAINING_TIME = "PausedRemainingTime";

        private const string STATE_STOPPED = "STOPPED";
        private const string STATE_HEATING = "HEATING";
        private const string STATE_PAUSED = "PAUSED";

        public OperationResult StartHeating(int timeInSeconds, int powerLevel, IStateStorage stateStorage)
        {
            try
            {
                var currentState = stateStorage.GetString(SESSION_MICROWAVE_STATE) ?? STATE_STOPPED;

                if (currentState == STATE_PAUSED)
                {
                    return ResumeHeating(stateStorage);
                }

                var oven = new MicrowaveOven(timeInSeconds, powerLevel);

                stateStorage.SetString(SESSION_CURRENT_OVEN, JsonSerializer.Serialize(new { timeInSeconds, powerLevel }));
                stateStorage.SetString(SESSION_IS_HEATING, "true");
                stateStorage.SetString(SESSION_START_TIME, DateTime.Now.ToString("O"));
                stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_HEATING);

                oven.StartHeating();

                var timeDisplay = FormatTimeDisplay(timeInSeconds);
                return OperationResult.CreateSuccess($"Aquecimento iniciado: {timeDisplay} a potência {powerLevel}.");
            }
            catch (ArgumentException ex)
            {
                return OperationResult.CreateError(ex.Message, "INVALID_PARAMETERS");
            }
        }

        public MicrowaveStatus GetHeatingProgress(IStateStorage stateStorage)
        {
            var currentState = stateStorage.GetString(SESSION_MICROWAVE_STATE) ?? STATE_STOPPED;
            var isHeatingStr = stateStorage.GetString(SESSION_IS_HEATING);
            var ovenDataStr = stateStorage.GetString(SESSION_CURRENT_OVEN);

            if (currentState == STATE_PAUSED)
            {
                var pausedTimeStr = stateStorage.GetString(SESSION_PAUSED_REMAINING_TIME);
                if (!string.IsNullOrEmpty(pausedTimeStr) && !string.IsNullOrEmpty(ovenDataStr))
                {
                    var pausedRemainingTime = int.Parse(pausedTimeStr);
                    var pausedOvenData = JsonSerializer.Deserialize<JsonElement>(ovenDataStr);
                    var pausedPowerLevel = pausedOvenData.GetProperty("powerLevel").GetInt32();

                    return new MicrowaveStatus
                    {
                        IsRunning = false,
                        RemainingTime = pausedRemainingTime,
                        PowerLevel = pausedPowerLevel,
                        Progress = 0,
                        StatusMessage = $"PAUSADO - Restam {FormatTimeDisplay(pausedRemainingTime)}. Pressione 'Retomar Aquecimento' para continuar.",
                        FormattedRemainingTime = FormatTimeDisplay(pausedRemainingTime)
                    };
                }
            }

            if (isHeatingStr != "true" || string.IsNullOrEmpty(ovenDataStr))
            {
                return new MicrowaveStatus
                {
                    IsRunning = false,
                    RemainingTime = 0,
                    PowerLevel = 0,
                    Progress = 0,
                    StatusMessage = "Micro-ondas parado.",
                    FormattedRemainingTime = "0s"
                };
            }

            var ovenData = JsonSerializer.Deserialize<JsonElement>(ovenDataStr);
            var timeInSeconds = ovenData.GetProperty("timeInSeconds").GetInt32();
            var powerLevel = ovenData.GetProperty("powerLevel").GetInt32();

            var startTimeStr = stateStorage.GetString(SESSION_START_TIME);
            var startTime = DateTime.Parse(startTimeStr!);
            var elapsedSeconds = (int)(DateTime.Now - startTime).TotalSeconds;

            var remainingTime = timeInSeconds - elapsedSeconds;

            if (elapsedSeconds >= timeInSeconds)
            {
                stateStorage.SetString(SESSION_IS_HEATING, "false");
                stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_STOPPED);
                var finalProgressString = GenerateProgressString(powerLevel, timeInSeconds) + " Aquecimento concluído";
                return new MicrowaveStatus
                {
                    IsRunning = false,
                    RemainingTime = 0,
                    PowerLevel = powerLevel,
                    Progress = 100,
                    StatusMessage = finalProgressString,
                    FormattedRemainingTime = "0s"
                };
            }

            var progress = (elapsedSeconds * 100) / timeInSeconds;
            var progressString = GenerateProgressString(powerLevel, elapsedSeconds);

            return new MicrowaveStatus
            {
                IsRunning = true,
                RemainingTime = remainingTime,
                PowerLevel = powerLevel,
                Progress = progress,
                StatusMessage = progressString,
                FormattedRemainingTime = FormatTimeDisplay(remainingTime)
            };
        }

        public OperationResult StartQuickHeat(IStateStorage stateStorage)
        {
            return StartHeating(30, 10, stateStorage);
        }

        public OperationResult IncreaseTime(int additionalSeconds, IStateStorage stateStorage)
        {
            var isHeatingStr = stateStorage.GetString(SESSION_IS_HEATING);
            var ovenDataStr = stateStorage.GetString(SESSION_CURRENT_OVEN);

            if (isHeatingStr != "true" || string.IsNullOrEmpty(ovenDataStr))
                return OperationResult.CreateError("Erro: Micro-ondas não está aquecendo.", "NOT_HEATING");

            try
            {
                var ovenData = JsonSerializer.Deserialize<JsonElement>(ovenDataStr);
                var currentTime = ovenData.GetProperty("timeInSeconds").GetInt32();
                var powerLevel = ovenData.GetProperty("powerLevel").GetInt32();
                var newTime = currentTime + additionalSeconds;

                new MicrowaveOven(newTime, powerLevel);

                stateStorage.SetString(SESSION_CURRENT_OVEN, JsonSerializer.Serialize(new { timeInSeconds = newTime, powerLevel }));

                var timeDisplay = FormatTimeDisplay(newTime);
                return OperationResult.CreateSuccess($"Tempo aumentado para {timeDisplay}.");
            }
            catch (ArgumentException ex)
            {
                return OperationResult.CreateError(ex.Message, "INVALID_TIME");
            }
        }

        public OperationResult PauseOrCancel(IStateStorage stateStorage)
        {
            var currentState = stateStorage.GetString(SESSION_MICROWAVE_STATE) ?? STATE_STOPPED;
            var ovenDataStr = stateStorage.GetString(SESSION_CURRENT_OVEN);

            switch (currentState)
            {
                case STATE_HEATING:
                    return PauseHeating(stateStorage);

                case STATE_PAUSED:
                    return CancelHeating(stateStorage);

                case STATE_STOPPED:
                    if (!string.IsNullOrEmpty(ovenDataStr))
                    {
                        ClearAllSettings(stateStorage);
                        return OperationResult.CreateSuccess("Configurações limpas.");
                    }
                    return OperationResult.CreateError("Micro-ondas já está parado.", "NOT_RUNNING");

                default:
                    return OperationResult.CreateError("Estado desconhecido.", "UNKNOWN_STATE");
            }
        }

        private string GenerateProgressString(int powerLevel, int elapsedSeconds)
        {
            if (elapsedSeconds <= 0)
                return "";

            var dotPattern = new string('.', powerLevel);
            var segments = new List<string>();

            for (int i = 0; i < elapsedSeconds; i++)
            {
                segments.Add(dotPattern);
            }

            return string.Join(" ", segments);
        }

        private string FormatTimeDisplay(int timeInSeconds)
        {
            if (timeInSeconds > 60 && timeInSeconds < 100)
            {
                var minutes = timeInSeconds / 60;
                var seconds = timeInSeconds % 60;
                return $"{minutes}:{seconds:00}";
            }

            return $"{timeInSeconds}s";
        }

        /// <summary>
        /// Retoma o aquecimento a partir do ponto onde foi pausado
        /// </summary>
        private OperationResult ResumeHeating(IStateStorage stateStorage)
        {
            var remainingTimeStr = stateStorage.GetString(SESSION_PAUSED_REMAINING_TIME);
            var ovenDataStr = stateStorage.GetString(SESSION_CURRENT_OVEN);

            if (string.IsNullOrEmpty(remainingTimeStr) || string.IsNullOrEmpty(ovenDataStr))
                return OperationResult.CreateError("Erro: Dados de pausa não encontrados.", "NO_PAUSE_DATA");

            var remainingTime = int.Parse(remainingTimeStr);
            var ovenData = JsonSerializer.Deserialize<JsonElement>(ovenDataStr);
            var powerLevel = ovenData.GetProperty("powerLevel").GetInt32();

            stateStorage.SetString(SESSION_CURRENT_OVEN, JsonSerializer.Serialize(new { timeInSeconds = remainingTime, powerLevel }));
            stateStorage.SetString(SESSION_IS_HEATING, "true");
            stateStorage.SetString(SESSION_START_TIME, DateTime.Now.ToString("O"));
            stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_HEATING);
            stateStorage.Remove(SESSION_PAUSED_REMAINING_TIME);

            var timeDisplay = FormatTimeDisplay(remainingTime);
            return OperationResult.CreateSuccess($"Aquecimento retomado: {timeDisplay} restantes a potência {powerLevel}.");
        }

        /// <summary>
        /// Pausa o aquecimento salvando o tempo restante
        /// </summary>
        private OperationResult PauseHeating(IStateStorage stateStorage)
        {
            var status = GetHeatingProgress(stateStorage);

            if (!status.IsRunning)
                return OperationResult.CreateError("Micro-ondas não está aquecendo.", "NOT_HEATING");

            stateStorage.SetString(SESSION_PAUSED_REMAINING_TIME, status.RemainingTime.ToString());
            stateStorage.SetString(SESSION_IS_HEATING, "false");
            stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_PAUSED);

            var timeDisplay = FormatTimeDisplay(status.RemainingTime);
            return OperationResult.CreateSuccess($"Aquecimento pausado. Restam {timeDisplay}. Pressione 'Retomar Aquecimento' para continuar.");
        }

        /// <summary>
        /// Cancela totalmente o aquecimento
        /// </summary>
        private OperationResult CancelHeating(IStateStorage stateStorage)
        {
            ClearAllSettings(stateStorage);
            return OperationResult.CreateSuccess("Aquecimento cancelado. Todas as configurações foram limpas.");
        }

        /// <summary>
        /// Limpa todas as configurações e volta ao estado inicial
        /// </summary>
        private void ClearAllSettings(IStateStorage stateStorage)
        {
            stateStorage.Remove(SESSION_CURRENT_OVEN);
            stateStorage.Remove(SESSION_IS_HEATING);
            stateStorage.Remove(SESSION_START_TIME);
            stateStorage.Remove(SESSION_PAUSED_REMAINING_TIME);
            stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_STOPPED);
        }
    }
}
