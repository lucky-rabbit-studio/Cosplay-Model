﻿// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Holds script execution until user activates a `continue` input.
    /// Shortcut for `@wait input`.
    /// </summary>
    /// <example>
    /// ; User will have to activate a `continue` input after the first sentence 
    /// ; for the printer to contiue printing out the following text.
    /// Lorem ipsum dolor sit amet.[i] Consectetur adipiscing elit.
    /// </example>
    [ActionAlias("i")]
    public class WaitForInput : NovelAction
    {
        public override async Task ExecuteAsync ()
        {
            var waitAction = new Wait();
            waitAction.WaitMode = "input";
            await waitAction.ExecuteAsync();
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
