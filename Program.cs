using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        // Version (Used for display)
        string version = "v0.1.1";
        string overflowTag = "[Overflow]";

        static int waitSeconds = 300;
        static int waitTicksStatic = (waitSeconds * 60) / 100;
        int waitTicks = waitTicksStatic;

        List<IMyAssembler> assemblers = new List<IMyAssembler>();
        List<IMyCargoContainer> taggedCargos = new List<IMyCargoContainer>();
        
        IMyCargoContainer overflowCargo;
        
        List <AssemblerContainer> assemblerContainers = new List<AssemblerContainer>();
        
        class CargoContainer
        {
            public IMyCargoContainer Container;
            public IMyInventory Inventory;
            public bool OverflowFlag = false;
        }

        class AssemblerContainer
        {
            public IMyAssembler Assembler;
            public IMyInventory InventoryIn;
            public IMyInventory InventoryOut;
        }
        
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
            foreach (var assembler in assemblers)
            {
                var managedAssembler = new AssemblerContainer
                {
                    Assembler = assembler,
                    InventoryIn = assembler.InputInventory,
                    InventoryOut = assembler.OutputInventory,
                };
                
                assemblerContainers.Add(managedAssembler);
            }
            
            
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(taggedCargos, container => container.CustomName.Contains(overflowTag));
            foreach (var cargoContainer in taggedCargos)
            {
                var managedCargo = new CargoContainer
                {
                    Container = cargoContainer,
                    Inventory = cargoContainer.GetInventory(0)
                };

                if (cargoContainer.CustomName.Contains(overflowTag))
                {
                    managedCargo.OverflowFlag = true;
                    overflowCargo = managedCargo.Container;
                }
            }
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo($"Kippen Assembler Emptier (KAE) {version}...");
            Echo($"Waiting {waitTicks * 100 / 60} more seconds...\n");

            if (waitTicks > 0)
            {
                waitTicks--;
                return;
            }

            foreach (var managedAssembler in assemblerContainers)
            {
                var itemsIn = new List<MyInventoryItem>();
                var itemsOut = new List<MyInventoryItem>();

                managedAssembler.InventoryIn.GetItems(itemsIn);
                managedAssembler.InventoryOut.GetItems(itemsOut);

                for (var i = itemsIn.Count - 1; i >= 0; i--)
                {
                    managedAssembler.InventoryIn.TransferItemTo(overflowCargo.GetInventory(), itemsIn[i]);
                }

                for (var i = itemsOut.Count - 1; i >= 0; i--)
                {
                    managedAssembler.InventoryOut.TransferItemTo(overflowCargo.GetInventory(), itemsOut[i]);
                }
            }
            
            waitTicks = waitTicksStatic;

        }
    }
}