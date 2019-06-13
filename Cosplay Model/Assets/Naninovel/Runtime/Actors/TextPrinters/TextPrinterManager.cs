// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Manages text printer actors.
    /// </summary>
    [InitializeAtRuntime]
    public class TextPrinterManager : NovelActorManager<ITextPrinterActor, TextPrinterState>, IStatefulService<SettingsStateMap>
    {
        [System.Serializable]
        private class Settings
        {
            public float PrintSpeed = .5f;
        }

        public string DefaultPrinterId => config.DefaulPrinterId;
        public float MaxPrintDelay => config.MaxPrintDelay;
        public float PrintSpeed { get; private set; }
        public float PrintDelay => Mathf.Lerp(MaxPrintDelay, 0, PrintSpeed);

        private readonly TextPrintersConfiguration config;

        public TextPrinterManager (TextPrintersConfiguration config) 
            : base(config)
        {
            this.config = config;
        }

        public Task SaveServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                PrintSpeed = PrintSpeed
            };
            stateMap.SerializeObject(settings);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.DeserializeObject<Settings>() ?? new Settings();
            SetPrintSpeed(settings.PrintSpeed);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns currently active printer.
        /// </summary>
        public ITextPrinterActor GetActivePrinter ()
        {
            return ManagedActors.Values.FirstOrDefault(p => p.IsPrinterActive);
        }

        /// <summary>
        /// Selects printer with provided ID as active, wile de-activating all the other managed printers.
        /// </summary>
        public void SetActivePrinter (string id)
        {
            if (!ActorExists(id)) return;

            foreach (var prntr in ManagedActors.Values)
                prntr.IsPrinterActive = prntr.Id == id;
        }

        /// <summary>
        /// De-activates all the managed printers.
        /// </summary>
        public void DeactivateAllPrinters ()
        {
            foreach (var prntr in ManagedActors.Values)
                prntr.IsPrinterActive = false;
        }

        /// <summary>
        /// Sets <see cref="ITextPrinterActor.PrintDelay"/> for all the managed printers.
        /// </summary>
        public void SetPrintSpeed (float value)
        {
            PrintSpeed = value;
            foreach (var printer in ManagedActors)
                printer.Value.PrintDelay = PrintDelay;
        }

        public override TMetadata GetActorMetadata<TMetadata> (string actorId) =>
            (config.Metadata.GetMetaById(actorId) ?? config.DefaultMetadata) as TMetadata;

        protected override async Task<ITextPrinterActor> ConstructActorAsync (string actorId)
        {
            var actor = await base.ConstructActorAsync(actorId);

            // Set currently used delay when creating new printer.
            actor.PrintDelay = PrintDelay;

            return actor;
        }
    }
}
