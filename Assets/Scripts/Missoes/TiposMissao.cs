namespace IF360.Missoes
{
    /// <summary>
    /// Estados possíveis de uma missão (Etapa 3.1 do plano).
    /// Uma única missão pode estar em InProgress/ReadyToTurnIn por vez.
    /// </summary>
    public enum EstadoMissao
    {
        Locked = 0,
        Available = 1,
        InProgress = 2,
        ReadyToTurnIn = 3,
        Completed = 4
    }

    /// <summary>Categoria hierárquica do NPC (Etapa 2 do plano).</summary>
    public enum TipoNPC
    {
        MissionInitiator = 0,
        MissionSubordinate = 1,
        InformationalNPC = 2
    }

    /// <summary>Como o objetivo de uma etapa é cumprido (Etapa 4/7).</summary>
    public enum TipoInteracaoEtapa
    {
        Dialogo = 0,
        Minijogo = 1
    }

    /// <summary>
    /// Resultado estruturado da validação central de interações (Etapa 4.1).
    /// Nunca usar booleano genérico: o motivo precisa ser identificável.
    /// </summary>
    public enum ResultadoInteracao
    {
        InteractionAllowed = 0,
        NoActiveMission = 1,
        AnotherMissionActive = 2,
        WrongMission = 3,
        NPCNotPartOfMission = 4,
        WrongNPCForCurrentStep = 5,
        PreviousStepIncomplete = 6,
        MissingRequirements = 7,
        ObjectiveAlreadyCompleted = 8,
        MissionAlreadyCompleted = 9,
        MissionReadyToTurnIn = 10,
        InformationalNpc = 11,
        MissionLocked = 12
    }
}
