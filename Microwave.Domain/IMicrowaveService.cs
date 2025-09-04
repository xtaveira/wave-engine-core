using System;
using Microwave.Domain.DTOs;

namespace Microwave.Domain
{
    /// <summary>
    /// Interface principal do serviço de micro-ondas, define todas as operações de aquecimento e gerenciamento de programas
    /// </summary>
    public interface IMicrowaveService
    {
        /// <summary>
        /// Inicia aquecimento manual com parâmetros específicos
        /// </summary>
        /// <param name="timeInSeconds">Tempo de aquecimento em segundos (1-120)</param>
        /// <param name="powerLevel">Nível de potência (1-10)</param>
        /// <param name="stateStorage">Armazenamento de estado da sessão</param>
        /// <returns>Resultado da operação</returns>
        OperationResult StartHeating(int timeInSeconds, int powerLevel, IStateStorage stateStorage);

        /// <summary>
        /// Obtém o progresso atual do aquecimento
        /// </summary>
        MicrowaveStatus GetHeatingProgress(IStateStorage stateStorage);

        /// <summary>
        /// Inicia aquecimento rápido (30s na potência máxima)
        /// </summary>
        OperationResult StartQuickHeat(IStateStorage stateStorage);

        /// <summary>
        /// Adiciona tempo extra ao aquecimento em andamento (bloqueado para programas pré-definidos)
        /// </summary>
        OperationResult IncreaseTime(int additionalSeconds, IStateStorage stateStorage);

        /// <summary>
        /// Pausa ou cancela o aquecimento dependendo do estado atual
        /// </summary>
        OperationResult PauseOrCancel(IStateStorage stateStorage);

        /// <summary>
        /// Retorna a lista dos 5 programas pré-definidos do sistema
        /// </summary>
        IEnumerable<PredefinedProgram> GetPredefinedPrograms();

        /// <summary>
        /// Inicia um programa pré-definido pelo nome
        /// </summary>
        OperationResult StartPredefinedProgram(string programName, IStateStorage stateStorage);

        /// <summary>
        /// Inicia um programa personalizado criado pelo usuário
        /// </summary>
        Task<OperationResult> StartCustomProgramAsync(Guid customProgramId, IStateStorage stateStorage);

        Task<IEnumerable<ProgramDisplayInfo>> GetAllProgramsAsync();
        Task<CustomProgram?> GetCustomProgramAsync(Guid id);
    }
}
