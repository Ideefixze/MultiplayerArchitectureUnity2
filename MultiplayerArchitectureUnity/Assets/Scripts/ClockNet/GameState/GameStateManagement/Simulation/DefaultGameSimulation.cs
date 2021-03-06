﻿using ClockNet.GameState.CommandExecutor;
using ClockNet.GameState.Commands;
using UnityEngine;

namespace ClockNet.GameState.GameStateManagement.Simulation
{
    public class DefaultGameSimulation : IGameSimulation
    {
        private IGameExecutor executor; 
        private GameState gameState;

        /// <summary>
        /// DefaultGameSimulation constructor.
        /// </summary>
        /// <param name="executor">Executor that will execute commands on gameState</param>
        /// <param name="gameState">Must be a GameState used by the executor!</param>
        public DefaultGameSimulation(IGameExecutor executor, GameState gameState)
        {
            this.executor = executor;
            this.gameState = gameState;
        }
        public void WorldTick()
        {
            executor.Execute(new AddPointsCommand(1));
        }
    }
}
