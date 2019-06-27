// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Provides managed text documents access and automatically replaces static string values, marked with <see cref="ManagedTextAttribute"/>.
    /// </summary>
    [InitializeAtRuntime]
    public class TextManager : IEngineService
    {
        private readonly ManagedTextConfiguration config;
        private readonly ResourceProviderManager providersManager;
        private readonly LocalizationManager localizationManager;
        private readonly HashSet<ManagedTextRecord> records = new HashSet<ManagedTextRecord>();
        private ScriptLoader documentLoader;

        public TextManager (ManagedTextConfiguration config, ResourceProviderManager providersManager, LocalizationManager localizationManager)
        {
            this.config = config;
            this.providersManager = providersManager;
            this.localizationManager = localizationManager;
        }

        public Task InitializeServiceAsync ()
        {
            localizationManager.AddChangeLocaleCallback(ApplyManagedTextAsync);
            documentLoader = new ScriptLoader(config.LoaderConfiguration, providersManager, localizationManager);
            return Task.CompletedTask;
        }

        public void ResetService () { }

        public void DestroyService ()
        {
            localizationManager?.RemoveChangeLocaleCallback(ApplyManagedTextAsync);
            documentLoader?.UnloadAll();
        }

        /// <summary>
        /// Attempts to retrieve a managed text record value with the provided key and category (document name).
        /// Will return null when no records found.
        /// </summary>
        public string GetRecordValue (string key, string category = ManagedTextRecord.DefaultCategoryName)
        {
            foreach (var record in records)
                if (record.Category.EqualsFast(category) && record.Key.EqualsFast(key))
                    return record.Value;
            return null;
        }

        public List<ManagedTextRecord> GetAllRecords (params string[] categoryFilter)
        {
            if (categoryFilter is null || categoryFilter.Length == 0)
                return records.ToList();

            var result = new List<ManagedTextRecord>();
            foreach (var record in records)
                if (categoryFilter.Contains(record.Category))
                    result.Add(record);
            return result;
        }

        public async Task ApplyManagedTextAsync ()
        {
            records.Clear();
            documentLoader.UnloadAll();
            var documents = await documentLoader.LoadAllAsync();
            foreach (var document in documents)
            {
                var managedTextSet = ManagedTextUtils.GetManagedTextFromScript(document);

                foreach (var text in managedTextSet)
                    records.Add(new ManagedTextRecord(text.FieldId, text.FieldValue, text.Category));

                ManagedTextUtils.SetManagedTextValues(managedTextSet);
            }
        }
    }
}
