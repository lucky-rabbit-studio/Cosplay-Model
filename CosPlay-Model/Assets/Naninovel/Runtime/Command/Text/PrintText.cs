// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Commands
{
    /// <summary>
    /// Resets the active printer, prints text message and waits for user input.
    /// </summary>
    /// <remarks>
    /// This command is used under the hood when processing generic text lines, eg generic line `Kohaku: Hello World!` will be 
    /// automatically tranformed into `@print "Hello World!" actor:Kohaku` when parsing the naninovel scripts.<br/>
    /// Will cancel the printing (reveal the text at once) on `Continue` and `Skip` inputs.<br/>
    /// Will attempt to play corresponding voice clip when [Auto Voicing](/guide/voicing.html#auto-voicing) feature is enabled.<br/>
    /// Will attempt to add the printed message to [printer backlog](/guide/printer-backlog.html).<br/>
    /// Will attempt to change tint color of the characters, based on [Speaker Highlight](/guide/characters.html#speaker-highlight) feature.
    /// </remarks>
    /// <example>
    /// ; Will print the infamous phrase
    /// @print "Lorem ipsum dolor sit amet."
    /// ; To include quotes in the text itself, escape them
    /// @print "Saying \"Stop the car\" was a mistake."
    /// </example>
    [CommandAlias("print")]
    public class PrintText : ModifyText, Command.IPreloadable, Command.ILocalizable
    {
        /// <summary>
        /// Text of the message to print.
        /// When the text contain spaces, wrap it in double quotes (`"`). 
        /// In case you wish to include the double quotes in the text itself, escape them.
        /// </summary>
        [CommandParameter(NamelessParameterAlias)]
        public string Text { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// ID of the printer actor to use.
        /// </summary>
        [CommandParameter("printer", true)]
        public string PrinterId { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// ID of the actor, which should be associated with the printed message.
        /// </summary>
        [CommandParameter("actor", true)]
        public string ActorId { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Whether to reset text of the printer before executing the printing task.
        /// </summary>
        [CommandParameter("reset", true)]
        public bool ResetPrinter { get => GetDynamicParameter(true); set => SetDynamicParameter(value); }
        /// <summary>
        /// Whether to wait for user input after finishing the printing task.
        /// </summary>
        [CommandParameter("waitInput", true)]
        public bool WaitForInput { get => GetDynamicParameter(true); set => SetDynamicParameter(value); }

        public bool AutoVoicingEnabled => IsFromScriptLine && AudioManager != null && AudioManager.AutoVoicingEnabled;
        public string AutoVoiceClipName => $"{ScriptName}/{LineNumber}.{InlineIndex}";

        protected AudioManager AudioManager => Engine.GetService<AudioManager>();

        public async Task HoldResourcesAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printerId = string.IsNullOrWhiteSpace(PrinterId) ? mngr.DefaultPrinterId : PrinterId;
            var printer = await mngr.GetOrAddActorAsync(printerId);
            await printer.HoldResourcesAsync(this, null);

            if (AutoVoicingEnabled) await AudioManager.HoldVoiceResourcesAsync(this, AutoVoiceClipName);
        }

        public void ReleaseResources ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printerId = string.IsNullOrWhiteSpace(PrinterId) ? mngr.DefaultPrinterId : PrinterId;
            if (mngr is null || !mngr.ActorExists(printerId)) return;
            mngr?.GetActor(printerId).ReleaseResources(this, null);

            if (AutoVoicingEnabled) AudioManager.ReleaseVoiceResources(this, AutoVoiceClipName);
        }

        public override async Task ExecuteAsync ()
        {
            var textMngr = Engine.GetService<TextPrinterManager>();
            var inputMngr = Engine.GetService<InputManager>();
            var printerId = string.IsNullOrWhiteSpace(PrinterId) ? textMngr.GetActivePrinter()?.Id ?? textMngr.DefaultPrinterId : PrinterId;
            var actorExisted = textMngr.ActorExists(printerId);
            var printer = await textMngr.GetOrAddActorAsync(printerId);

            UndoData.Executed = true;
            UndoData.AddedActor = !actorExisted;
            UndoData.InitialActivePrinterId = textMngr.GetActivePrinter()?.Id;
            UndoData.State = textMngr.GetActorState(printerId);

            { // Speaker highlight feature.
                var charMngr = Engine.GetService<CharacterManager>();
                foreach (var actor in charMngr.GetAllActors())
                {
                    var actorMeta = charMngr.GetActorMetadata<CharacterMetadata>(actor.Id);
                    if (!actorMeta.HighlightWhenSpeaking) continue;
                    var tintColor = actor.Id == ActorId ? actorMeta.SpeakingTint : actorMeta.NotSpeakingTint;
                    actor.ChangeTintColorAsync(tintColor, Duration).WrapAsync();
                }
            }

            if (AutoVoicingEnabled) AudioManager.PlayVoiceAsync(AutoVoiceClipName).WrapAsync();

            if (!printer.IsPrinterActive) textMngr.SetActivePrinter(printer.Id);
            if (ResetPrinter) printer.ResetText();
            await printer.PrintTextAsync(Text, ActorId, 
                inputMngr.Continue.GetInputStartCancellationToken(), 
                inputMngr.Skip.GetInputStartCancellationToken());
            if (WaitForInput) Engine.GetService<ScriptPlayer>()?.EnableWaitingForInput();

            if (ResetPrinter) Engine.GetService<UIManager>()?.GetUI<UI.IBacklogUI>()?.AddMessage(Text, ActorId, AutoVoicingEnabled ? AutoVoiceClipName : null);
            else Engine.GetService<UIManager>()?.GetUI<UI.IBacklogUI>()?.AppendMessage(Text, AutoVoicingEnabled ? AutoVoiceClipName : null);
        }

        public override Task UndoAsync ()
        {
            if (UndoData.Executed && WaitForInput) Engine.GetService<ScriptPlayer>()?.DisableWaitingForInput();
            return base.UndoAsync();
        }
    } 
}
