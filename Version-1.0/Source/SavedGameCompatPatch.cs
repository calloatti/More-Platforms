using HarmonyLib;
using Timberborn.SerializationSystem;
using Timberborn.WorldSerialization;

namespace Tobbert.MorePlatforms
{
    [HarmonyPatch(typeof(WorldSerializer), "DeserializeEntity")]
    public static class Patch_WorldSerializer_DeserializeEntity
    {
        public static void Prefix(SerializedObject serializedObject)
        {
            // Timberborn uses "Template" in newer versions but falls back to "TemplateName" for older saves
            string templateKey = serializedObject.Has("Template") ? "Template" : 
                                 serializedObject.Has("TemplateName") ? "TemplateName" : null;

            if (templateKey != null)
            {
                string templateName = serializedObject.Get<string>(templateKey);
                
                // Check if the old substring exists and replace it with the new one
                if (!string.IsNullOrEmpty(templateName) && templateName.Contains("HorizontalPlatformEnd"))
                {
                    string updatedTemplateName = templateName.Replace("HorizontalPlatformEnd", "SidePlatform");
                    
                    // Overwrite the serialized data before the original method deserializes it
                    serializedObject.Set(templateKey, updatedTemplateName);
                }
            }
        }
    }
}