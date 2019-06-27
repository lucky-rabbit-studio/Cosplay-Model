// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Adds a choice option to an active (or default) choice handler actor.
    /// </summary>
    /// <remarks>
    /// When `goto` parameter is not specified, will continue script execution from the next script line.
    /// </remarks>
    /// <example>
    /// ; Print the text, then immediately show choices and stop script execution.
    /// Continue executing this script or load another?[skipInput]
    /// @choice "Continue" goto:.Continue
    /// @choice "Load another from start" goto:AnotherScript
    /// @choice "Load another from \"MyLabel\"" goto:AnotherScript.MyLabel
    /// @stop
    /// 
    /// ; Following example shows how to make an interactive map via `@choice` commands.
    /// ; For this example, we assume, that inside a `Resources/MapButtons` folder you've stored prefabs with `ChoiceHandlerButton` component attached to their root objects.
    /// ; Please note, that making a custom choice handler is a more appropriate solution for this, unless you can't (or don't want to) mess with C# scripting.
    /// # Map
    /// @back Map
    /// @hideText
    /// @choice handler:ButtonArea button:MapButtons/Home pos:-300,-300 goto:.HomeScene
    /// @choice handler:ButtonArea button:MapButtons/Shop pos:300,200 goto:.ShopScene
    /// @stop
    /// 
    /// # HomeScene
    /// @back Home
    /// Home, sweet home!
    /// @goto.Map
    /// 
    /// # ShopScene
    /// @back Shop
    /// Don't forget about cucumbers!
    /// @goto.Map
    /// </example>
    [CommandAlias("choice")]
    public class AddChoice : Command, Command.ILocalizable, Command.IPreloadable
    {
        public class UndoData { public string ChoiceId, HandlerId, InitialActiveHandlerId; public SetCustomVariable SetAction; }

        /// <summary>
        /// Text to show for the choice.
        /// When the text contain spaces, wrap it in double quotes (`"`). 
        /// In case you wish to include the double quotes in the text itself, escape them.
        /// </summary>
        [CommandParameter(NamelessParameterAlias, true)]
        public string ChoiceSummary { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Path (relative to a `Resources` folder) to a [button prefab](/guide/choices.md#choice-button) representing the choice. 
        /// The prefab should have a `ChoiceHandlerButton` component attached to the root object.
        /// Will use a default button when not provided.
        /// </summary>
        [CommandParameter("button", true)]
        public string ButtonPath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Local position of the choice button inside the choice handler (if supported by the handler implementation).
        /// </summary>
        [CommandParameter("pos", true)]
        public float?[] ButtonPosition { get => GetDynamicParameter<float?[]>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// ID of the choice handler to add choice for.
        /// </summary>
        [CommandParameter("handler", true)]
        public string HandlerId { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Path to go when the choice is selected by user;
        /// See [`@goto`](/api/#goto) command for the path format.
        /// </summary>
        [CommandParameter("goto", true)]
        public Named<string> GotoPath { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Set expression to execute when the choice is selected by user; 
        /// see [`@set`](/api/#set) command for syntax reference.
        /// </summary>
        [CommandParameter("set", true)]
        public string SetExpression { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData = new UndoData();

        public async Task HoldResourcesAsync ()
        {
            var mngr = Engine.GetService<ChoiceHandlerManager>();
            var handlerId = string.IsNullOrWhiteSpace(HandlerId) ? mngr.DefaultHandlerId : HandlerId;
            var handler = await mngr.GetOrAddActorAsync(handlerId);
            await handler.HoldResourcesAsync(this, null);
        }

        public void ReleaseResources ()
        {
            var mngr = Engine.GetService<ChoiceHandlerManager>();
            var handlerId = string.IsNullOrWhiteSpace(HandlerId) ? mngr.DefaultHandlerId : HandlerId;
            if (mngr.ActorExists(handlerId)) mngr.GetActor(handlerId).ReleaseResources(this, null);
        }

        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<ChoiceHandlerManager>();

            undoData.InitialActiveHandlerId = mngr.GetActiveHandler()?.Id;

            var choiceHandler = string.IsNullOrEmpty(HandlerId) ? await mngr.GetActiveHandlerOrDefaultAsync() : mngr.GetActor(HandlerId);
            if (!choiceHandler.IsHandlerActive)
                await mngr.SetActiveHandlerAsync(choiceHandler.Id);

            var buttonPos = ButtonPosition is null ? null : (Vector2?)ArrayUtils.ToVector2(ButtonPosition);
            var choice = new ChoiceState(ChoiceSummary, ButtonPath, undoData, buttonPos, GotoPath?.Item1, GotoPath?.Item2, SetExpression);
            choiceHandler.AddChoice(choice);

            undoData.HandlerId = choiceHandler.Id;
            undoData.ChoiceId = choice.Id;
        }

        public override async Task UndoAsync ()
        {
            if (undoData.SetAction != null)
                await undoData.SetAction.UndoAsync();

            if (string.IsNullOrEmpty(undoData.ChoiceId))
            {
                undoData = new UndoData();
                return;
            }

            var manager = Engine.GetService<ChoiceHandlerManager>();

            if (!string.IsNullOrWhiteSpace(undoData.InitialActiveHandlerId))
                await manager.SetActiveHandlerAsync(undoData.InitialActiveHandlerId);
            else manager.DeactivateAllHandlers();

            if (!manager.ActorExists(undoData.HandlerId))
            {
                undoData = new UndoData();
                return;
            }

            var choiceHandler = manager.GetActor(undoData.HandlerId);
            choiceHandler.RemoveChoice(undoData.ChoiceId);

            undoData = new UndoData();
        }
    }
}
