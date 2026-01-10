using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MamboDMA.Games.BA
{
    public class Il2CppSystem
    {
        // The dictionary to store Class Name -> Address (RVA)
        private Dictionary<string, ulong> _classOffsets = new Dictionary<string, ulong>();

        private ulong _gameAssemblyBase;

        // VERIFY THIS VALUE IN il2cpp.h! (Usually 0xB8 for x64)
        private const ulong OFFSET_STATIC_FIELDS = 0xB8;

        public Il2CppSystem(ulong gameAssemblyBase, string scriptJsonPath)
        {
            _gameAssemblyBase = gameAssemblyBase;
            LoadScriptJson(scriptJsonPath);
        }

        private void LoadScriptJson(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("script.json not found!");

            string json = File.ReadAllText(path);
            JObject root = JObject.Parse(json);

            // We specifically want 'ScriptMetadata' which contains Class TypeInfo addresses
            JArray scriptMetadata = (JArray)root["ScriptMetadata"];

            foreach (var item in scriptMetadata)
            {
                // Name example: "Assembly-CSharp.dll|GameManager"
                // You might want to strip the DLL name if you prefer cleaner lookups
                string name = (string)item["Name"];
                ulong address = (ulong)item["Address"];

                if (!_classOffsets.ContainsKey(name))
                {
                    _classOffsets.Add(name, address);
                }
            }
            Console.WriteLine($"[Il2Cpp] Loaded {_classOffsets.Count} class addresses.");
        }

        /// <summary>
        /// gets the address of a static instance (Singleton) for a class.
        /// </summary>
        /// <param name="className">Exact name from script.json (e.g. "Assembly-CSharp.dll|GameManager")</param>
        /// <param name="instanceOffset">The offset of the 'instance' field found in dump.cs</param>
        public ulong GetStaticInstance(string className, ulong instanceOffset)
        {
            if (!_classOffsets.TryGetValue(className, out ulong classRva))
            {
                Console.WriteLine($"[Error] Class '{className}' not found in JSON.");
                return 0;
            }

            // 1. Get the Class Structure Pointer (TypeInfo)
            ulong classInfoPtr = _gameAssemblyBase + classRva;

            // 2. Read the pointer to the Static Fields Block
            // Note: You must use your DMA ReadPtr method here
            ulong staticFieldsBlock = DmaMemory.Read<ulong>(classInfoPtr + OFFSET_STATIC_FIELDS);

            if (staticFieldsBlock == 0) return 0;

            // 3. Apply the specific variable offset to get the Instance Pointer
            return DmaMemory.Read<ulong>(staticFieldsBlock + instanceOffset);
        }
    }
}
