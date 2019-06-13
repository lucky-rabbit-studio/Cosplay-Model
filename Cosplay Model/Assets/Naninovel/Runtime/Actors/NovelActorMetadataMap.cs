﻿// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable actor ID (string) to <see cref="NovelActorMetadata"/> map.
    /// </summary>
    /// <typeparam name="TMeta">Type of the novel actor metadata.</typeparam>
    [System.Serializable]
    public abstract class NovelActorMetadataMap<TMeta> where TMeta : NovelActorMetadata
    {
        public int Length => ids.Length;

        [SerializeField] private string[] ids = new string[0];
        [SerializeField] private TMeta[] metas = new TMeta[0];

        public NovelActorMetadataMap () { }

        public NovelActorMetadataMap (IDictionary<string, TMeta> dictionary)
        {
            ids = new string[dictionary.Count];
            metas = new TMeta[dictionary.Count];

            foreach (var kv in dictionary)
                AddRecord(kv.Key, kv.Value);
        }

        public TMeta this[string id] { get => GetMetaById(id); set => AddRecord(id, value); }

        public Dictionary<string, TMeta> ToDictionary ()
        {
            var dictionary = new Dictionary<string, TMeta>();
            for (int i = 0; i < ids.Length; i++)
                dictionary[ids[i]] = metas.ElementAtOrDefault(i);
            return dictionary;
        }

        public List<TMeta> GetAllMetas () => new List<TMeta>(metas);

        public bool ContainsId (string id) => ArrayUtils.Contains(ids, id);

        public TMeta GetMetaById (string id)
        {
            if (!ContainsId(id)) return null;

            var index = ArrayUtils.IndexOf(ids, id);
            return metas.ElementAtOrDefault(index);
        }

        public void AddRecord (string id, TMeta meta)
        {
            if (ContainsId(id))
            {
                var index = ArrayUtils.IndexOf(ids, id);
                metas[index] = meta;
            }
            else
            {
                ArrayUtils.Add(ref ids, id);
                ArrayUtils.Add(ref metas, meta);
            }
        }

        public void RemoveRecord (string id)
        {
            if (!ContainsId(id)) return;

            var index = ArrayUtils.IndexOf(ids, id);
            ArrayUtils.RemoveAt(ref ids, index);
            ArrayUtils.RemoveAt(ref metas, index);
        }

        public void RemoveAllRecords ()
        {
            ArrayUtils.ClearAndResize(ref ids);
            ArrayUtils.ClearAndResize(ref metas);
        }
    }
}