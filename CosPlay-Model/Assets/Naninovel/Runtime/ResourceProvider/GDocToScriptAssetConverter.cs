// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Text;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    public class GDocToScriptAssetConverter : IGoogleDriveConverter<ScriptAsset>
    {
        public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
            new RawDataRepresentation(null, "application/vnd.google-apps.document")
        }; } }

        public string ExportMimeType { get { return "text/plain"; } }

        public ScriptAsset Convert (byte[] obj) => ScriptAsset.FromScriptText(Encoding.UTF8.GetString(obj));

        public Task<ScriptAsset> ConvertAsync (byte[] obj) => Task.FromResult(ScriptAsset.FromScriptText(Encoding.UTF8.GetString(obj)));

        public object Convert (object obj) => Convert(obj as byte[]);

        public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
    }
}
