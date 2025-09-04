using Microwave.Domain;
using Microwave.Domain.DTOs;
using Microwave.Domain.Interfaces;
using System.Text.Json;

namespace Microwave.Application
{
    public class MicrowaveService : IMicrowaveService
    {
        private readonly IProgramDisplayService _programDisplayService;

        public MicrowaveService(IProgramDisplayService programDisplayService)
        {
            _programDisplayService = programDisplayService;
        }

        private const string SESSION_CURRENT_OVEN = "CurrentOven";
        private const string SESSION_IS_HEATING = "IsHeating";
        private const string SESSION_START_TIME = "StartTime";
        private const string SESSION_MICROWAVE_STATE = "MicrowaveState";
        private const string SESSION_PAUSED_REMAINING_TIME = "PausedRemainingTime";
        private const string SESSION_CURRENT_PROGRAM = "CurrentProgram";
        private const string SESSION_HEATING_CHAR = "HeatingChar";

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

                var oven = MicrowaveOven.CreateManual(timeInSeconds, powerLevel);

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
                var finalProgressString = GenerateProgressString(powerLevel, timeInSeconds, stateStorage) + " Aquecimento concluído";
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
            var progressString = GenerateProgressString(powerLevel, elapsedSeconds, stateStorage);

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
            var currentProgramStr = stateStorage.GetString(SESSION_CURRENT_PROGRAM);

            if (isHeatingStr != "true" || string.IsNullOrEmpty(ovenDataStr))
                return OperationResult.CreateError("Erro: Micro-ondas não está aquecendo.", "NOT_HEATING");

            if (!string.IsNullOrEmpty(currentProgramStr))
                return OperationResult.CreateError("Erro: Não é permitido aumentar tempo em programa pré-definido.", "PREDEFINED_PROGRAM");

            try
            {
                var ovenData = JsonSerializer.Deserialize<JsonElement>(ovenDataStr);
                var currentTime = ovenData.GetProperty("timeInSeconds").GetInt32();
                var powerLevel = ovenData.GetProperty("powerLevel").GetInt32();
                var newTime = currentTime + additionalSeconds;

                MicrowaveOven.CreateManual(newTime, powerLevel);

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

        /// <summary>
        /// Gera string visual de progresso baseada na potência e tempo decorrido.
        /// Algoritmo: caractere de aquecimento se repete conforme a potência, 
        /// intercalado com pontos para mostrar progresso temporal.
        /// </summary>
        /// <param name="powerLevel">Potência (1-10) - determina frequência do caractere</param>
        /// <param name="elapsedSeconds">Tempo decorrido para calcular progresso</param>
        /// <param name="stateStorage">Storage para obter caractere de aquecimento</param>
        /// <returns>String visual representando o aquecimento em andamento</returns>
        private string GenerateProgressString(int powerLevel, int elapsedSeconds, IStateStorage stateStorage)
        {
            if (elapsedSeconds <= 0)
                return "";

            var customChar = stateStorage.GetString(SESSION_HEATING_CHAR);
            var heatingChar = !string.IsNullOrEmpty(customChar) ? customChar : ".";

            var pattern = new string(heatingChar[0], powerLevel);
            var segments = new List<string>();

            for (int i = 0; i < elapsedSeconds; i++)
            {
                segments.Add(pattern);
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

        private OperationResult CancelHeating(IStateStorage stateStorage)
        {
            ClearAllSettings(stateStorage);
            return OperationResult.CreateSuccess("Aquecimento cancelado. Todas as configurações foram limpas.");
        }

        private void ClearAllSettings(IStateStorage stateStorage)
        {
            stateStorage.Remove(SESSION_CURRENT_OVEN);
            stateStorage.Remove(SESSION_IS_HEATING);
            stateStorage.Remove(SESSION_START_TIME);
            stateStorage.Remove(SESSION_PAUSED_REMAINING_TIME);
            stateStorage.Remove(SESSION_CURRENT_PROGRAM);
            stateStorage.Remove(SESSION_HEATING_CHAR);
            stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_STOPPED);
        }


        public IEnumerable<PredefinedProgram> GetPredefinedPrograms()
        {
            return new List<PredefinedProgram>
            {
                new PredefinedProgram
                {
                    Name = "Pipoca",
                    Food = "Pipoca (de micro-ondas)",
                    TimeInSeconds = 180,
                    PowerLevel = 7,
                    HeatingChar = "∩",
                    Instructions = "Observar o barulho de estouros do milho, caso houver um intervalo de mais de 10 segundos entre um estouro e outro, interrompa o aquecimento."
                },
                new PredefinedProgram
                {
                    Name = "Leite",
                    Food = "Leite",
                    TimeInSeconds = 300,
                    PowerLevel = 5,
                    HeatingChar = "∿",
                    Instructions = "Cuidado com aquecimento de líquidos, o choque térmico aliado ao movimento do recipiente pode causar fervura imediata causando risco de queimaduras."
                },
                new PredefinedProgram
                {
                    Name = "Carnes de boi",
                    Food = "Carne em pedaço ou fatias",
                    TimeInSeconds = 840,
                    PowerLevel = 4,
                    HeatingChar = "≡",
                    Instructions = "Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme."
                },
                new PredefinedProgram
                {
                    Name = "Frango",
                    Food = "Frango (qualquer corte)",
                    TimeInSeconds = 480,
                    PowerLevel = 7,
                    HeatingChar = "∴",
                    Instructions = "Interrompa o processo na metade e vire o conteúdo com a parte de baixo para cima para o descongelamento uniforme."
                },
                new PredefinedProgram
                {
                    Name = "Feijão",
                    Food = "Feijão congelado",
                    TimeInSeconds = 480,
                    PowerLevel = 9,
                    HeatingChar = "◊",
                    Instructions = "Deixe o recipiente destampado e em casos de plástico, cuidado ao retirar o recipiente pois o mesmo pode perder resistência em altas temperaturas."
                }
            };
        }

        public OperationResult StartPredefinedProgram(string programName, IStateStorage stateStorage)
        {
            try
            {
                var currentState = stateStorage.GetString(SESSION_MICROWAVE_STATE) ?? STATE_STOPPED;

                if (currentState == STATE_PAUSED)
                {
                    return ResumeHeating(stateStorage);
                }

                var programs = GetPredefinedPrograms();
                var selectedProgram = programs.FirstOrDefault(p => p.Name == programName);

                if (selectedProgram == null)
                {
                    return OperationResult.CreateError($"Erro: Programa '{programName}' não encontrado.", "PROGRAM_NOT_FOUND");
                }

                var oven = MicrowaveOven.CreatePredefined(selectedProgram.TimeInSeconds, selectedProgram.PowerLevel);

                stateStorage.SetString(SESSION_CURRENT_OVEN, JsonSerializer.Serialize(new
                {
                    timeInSeconds = selectedProgram.TimeInSeconds,
                    powerLevel = selectedProgram.PowerLevel
                }));
                MicrowaveOven.CreatePredefined(selectedProgram.TimeInSeconds, selectedProgram.PowerLevel);
                stateStorage.SetString(SESSION_CURRENT_OVEN, JsonSerializer.Serialize(new
                {
                    timeInSeconds = selectedProgram.TimeInSeconds,
                    powerLevel = selectedProgram.PowerLevel
                }));
                stateStorage.SetString(SESSION_CURRENT_PROGRAM, programName);
                stateStorage.SetString(SESSION_HEATING_CHAR, selectedProgram.HeatingChar);
                stateStorage.SetString(SESSION_IS_HEATING, "true");
                stateStorage.SetString(SESSION_START_TIME, DateTime.Now.ToString("O"));
                stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_HEATING);

                oven.StartHeating();

                var timeDisplay = FormatTimeDisplay(selectedProgram.TimeInSeconds);
                return OperationResult.CreateSuccess($"Programa '{programName}' iniciado: {timeDisplay} a potência {selectedProgram.PowerLevel}.");
            }
            catch (ArgumentException ex)
            {
                return OperationResult.CreateError(ex.Message, "INVALID_PARAMETERS");
            }
        }

        public async Task<OperationResult> StartCustomProgramAsync(Guid customProgramId, IStateStorage stateStorage)
        {
            try
            {
                var currentState = stateStorage.GetString(SESSION_MICROWAVE_STATE) ?? STATE_STOPPED;

                if (currentState == STATE_PAUSED)
                {
                    return ResumeHeating(stateStorage);
                }

                var customProgram = await GetCustomProgramAsync(customProgramId);
                if (customProgram == null)
                {
                    return OperationResult.CreateError($"Erro: Programa customizado não encontrado.", "CUSTOM_PROGRAM_NOT_FOUND");
                }

                var oven = MicrowaveOven.CreateCustom(customProgram.TimeInSeconds, customProgram.PowerLevel);

                stateStorage.SetString(SESSION_CURRENT_OVEN, JsonSerializer.Serialize(new
                {
                    timeInSeconds = customProgram.TimeInSeconds,
                    powerLevel = customProgram.PowerLevel
                }));
                stateStorage.SetString(SESSION_CURRENT_PROGRAM, $"custom-{customProgramId}");
                stateStorage.SetString(SESSION_HEATING_CHAR, customProgram.Character.ToString());
                stateStorage.SetString(SESSION_IS_HEATING, "true");
                stateStorage.SetString(SESSION_START_TIME, DateTime.Now.ToString("O"));
                stateStorage.SetString(SESSION_MICROWAVE_STATE, STATE_HEATING);

                oven.StartHeating();

                var timeDisplay = FormatTimeDisplay(customProgram.TimeInSeconds);
                return OperationResult.CreateSuccess($"Programa '{customProgram.Name}' iniciado: {timeDisplay} a potência {customProgram.PowerLevel}.");
            }
            catch (ArgumentException ex)
            {
                return OperationResult.CreateError(ex.Message, "INVALID_PARAMETERS");
            }
        }

        public async Task<IEnumerable<ProgramDisplayInfo>> GetAllProgramsAsync()
        {
            return await _programDisplayService.GetAllProgramsAsync();
        }

        public async Task<CustomProgram?> GetCustomProgramAsync(Guid id)
        {
            var programInfo = await _programDisplayService.GetProgramByIdAsync(id.ToString());
            if (programInfo == null || !programInfo.IsCustom)
                return null;

            return new CustomProgram(
                programInfo.Name,
                programInfo.Food,
                programInfo.PowerLevel,
                programInfo.TimeInSeconds,
                programInfo.Character,
                programInfo.Instructions
            )
            {
                Id = Guid.Parse(programInfo.Id),
                CreatedAt = programInfo.CreatedAt ?? DateTime.Now
            };
        }
    }
}
