using UnityEngine;
using UnityEditor;
using AudioSystem;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor utility to batch create SoundData assets from audio clips.
/// Run from menu: Tools > Audio > Create Combat SoundData Assets
/// </summary>
public static class SoundDataBatchCreator
{
    // Script GUID for AudioSystem.SoundData
    private const string SOUND_DATA_SCRIPT_GUID = "16edac33fa8a6da49a81b313a27485f8";
    
    /// <summary>
    /// Data structure for audio clip references
    /// </summary>
    private struct AudioClipData
    {
        public string clipName;
        public string clipGuid;
        public string category;      // Bosses, Heroes, MiniBosses
        public string subcategory;   // Apolaki, Babaylan, etc.
        
        public AudioClipData(string name, string guid, string cat, string sub)
        {
            clipName = name;
            clipGuid = guid;
            category = cat;
            subcategory = sub;
        }
    }
    
    [MenuItem("Tools/Audio/Create Combat SoundData Assets")]
    public static void CreateAllCombatSoundDataAssets()
    {
        var audioClips = GetAllAudioClips();
        int created = 0;
        int skipped = 0;
        
        foreach (var clip in audioClips)
        {
            string outputFolder = $"Assets/Data/Audio/SFX/Combat/{clip.category}/{clip.subcategory}";
            string assetPath = $"{outputFolder}/{clip.clipName}_SFX.asset";
            
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                CreateFolderRecursive(outputFolder);
            }
            
            // Skip if asset already exists
            if (AssetDatabase.LoadAssetAtPath<SoundData>(assetPath) != null)
            {
                skipped++;
                continue;
            }
            
            // Create SoundData asset
            SoundData soundData = ScriptableObject.CreateInstance<SoundData>();
            
            // Load the audio clip by GUID
            string clipPath = AssetDatabase.GUIDToAssetPath(clip.clipGuid);
            if (!string.IsNullOrEmpty(clipPath))
            {
                soundData.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            }
            
            // Set default values for SFX
            soundData.volume = 1f;
            soundData.pitch = 1f;
            soundData.loop = false;
            soundData.playOnAwake = false;
            soundData.spatialBlend = 0f; // 2D sound for UI/combat feedback
            soundData.priority = 128;
            
            // Create the asset
            AssetDatabase.CreateAsset(soundData, assetPath);
            created++;
            
            Debug.Log($"[SoundDataBatchCreator] Created: {assetPath}");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[SoundDataBatchCreator] Batch creation complete! Created: {created}, Skipped (already exists): {skipped}");
        EditorUtility.DisplayDialog("SoundData Batch Creator", 
            $"Created: {created} SoundData assets\nSkipped (already exists): {skipped}", "OK");
    }
    
    private static void CreateFolderRecursive(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(parent))
        {
            CreateFolderRecursive(parent);
        }
        
        string folderName = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folderName);
    }
    
    private static List<AudioClipData> GetAllAudioClips()
    {
        return new List<AudioClipData>
        {
            // BOSSES - Apolaki
            new AudioClipData("DaybreakFury", "212f8ffc04fdb0e48899ae674efe31da", "Bosses", "Apolaki"),
            new AudioClipData("RadiantCharge", "8d2bffe7b5256b047a3d1b8bbebe57bf", "Bosses", "Apolaki"),
            new AudioClipData("SolarFlareSlash", "e23794f8c1d9d5f40a57ea515965b942", "Bosses", "Apolaki"),
            new AudioClipData("SunburstNova", "92cf58f6dc45c77428c673c859a3ff88", "Bosses", "Apolaki"),
            
            // BOSSES - Bakunawa
            new AudioClipData("EatTheSunAndMoon", "154da12aad87ebb4a9882ef69ac60b9b", "Bosses", "Bakunawa"),
            new AudioClipData("EclipseFang", "7918d4ef05b14434195c55c4692fb91a", "Bosses", "Bakunawa"),
            new AudioClipData("LunarDevour", "24553413f1b1f4b41a7f15565323068d", "Bosses", "Bakunawa"),
            new AudioClipData("SerpentsCoil", "99f8e76fc888e9748b69438ea714f503", "Bosses", "Bakunawa"),
            new AudioClipData("ShadowDive", "06234d10747bc5c45ac83863d1698d3b", "Bosses", "Bakunawa"),
            
            // BOSSES - Bathala
            new AudioClipData("CelestialJudgement", "a24f682964b37c8458a70cac16f36f9c", "Bosses", "Bathala"),
            new AudioClipData("HeavensMandate", "996192dee4212ba4b96a92a63554b897", "Bosses", "Bathala"),
            new AudioClipData("Skyhammer", "0e93252ef86b8b442bbd3f235e04bf39", "Bosses", "Bathala"),
            new AudioClipData("ThunderousDecree", "4a1323f2588c3db4e9173ac0e500cff7", "Bosses", "Bathala"),
            
            // BOSSES - Mayari
            new AudioClipData("LunarStrike", "1c8058ca835be9f47bc03fea301a21ec", "Bosses", "Mayari"),
            new AudioClipData("MoonfallSpear", "5f6e99cb503e5d74abee8268bcecb2ff", "Bosses", "Mayari"),
            new AudioClipData("MoonlightGrace", "d4ec88a32a43ebb46be16b24ccd9e92d", "Bosses", "Mayari"),
            new AudioClipData("TideOfNight", "6eaf1feb8c6bdbe4db4fc1e3ccf62c6f", "Bosses", "Mayari"),
            
            // HEROES - Babaylan
            new AudioClipData("Blessing", "edb15f73c0f3fcb4baa5e520559f009f", "Heroes", "Babaylan"),
            new AudioClipData("Heal", "f5994f9f31e28d3459dba4b7b889bb24", "Heroes", "Babaylan"),
            new AudioClipData("ManaSurge", "8279d5087e140c948932c86fdda0da48", "Heroes", "Babaylan"),
            new AudioClipData("Purify", "bf1f661b5359de14397c6b48b3880532", "Heroes", "Babaylan"),
            new AudioClipData("Sacrifice", "b2d4bbc049fe7b04c99c94c791eb1d04", "Heroes", "Babaylan"),
            
            // HEROES - Bagani
            new AudioClipData("BaganiOath", "bb85cd4d9cd96f04a838efc2e2f7bb03", "Heroes", "Bagani"),
            new AudioClipData("Fortify", "27fd2769024e7804d8bce33cf8e82df1", "Heroes", "Bagani"),
            new AudioClipData("LastStand", "0f251b6698256a940a731c0d33a4995f", "Heroes", "Bagani"),
            new AudioClipData("ShieldBash", "1befa22c1a8eac74c99c4e4cd5175169", "Heroes", "Bagani"),
            new AudioClipData("Taunt", "96da7830bc6c4414e9aef9d0d794f9a3", "Heroes", "Bagani"),
            
            // HEROES - Mandirigma
            new AudioClipData("AllIn", "5ed1e37853a068243b17c9cce17667da", "Heroes", "Mandirigma"),
            new AudioClipData("Attack", "c0bace206282acf4999ae09713fa7075", "Heroes", "Mandirigma"),
            new AudioClipData("BerserkMonster", "63c9cb2695b7b3247bb96405e9e4b9f9", "Heroes", "Mandirigma"),
            new AudioClipData("HeavyAttack", "9c4e8a0958863bc4eb42e6c630e62054", "Heroes", "Mandirigma"),
            new AudioClipData("ManBerserk", "30b84359963c15542961f57fec88ca9d", "Heroes", "Mandirigma"),
            new AudioClipData("Rest", "9faf5dca6e86fd2428b99ea652069c3f", "Heroes", "Mandirigma"),
            
            // HEROES - Mangangayaw
            new AudioClipData("ExplosiveArrow", "0b54131e07f3fed4b9fa634cd5af1dec", "Heroes", "Mangangayaw"),
            new AudioClipData("FocusAim", "991e90e1257f9ea40aaf4625bc0bb697", "Heroes", "Mangangayaw"),
            new AudioClipData("PiercingArrow", "be3718150a4327641966a5097c497989", "Heroes", "Mangangayaw"),
            new AudioClipData("QuickShot", "09f488169272d9a489e85be17471106c", "Heroes", "Mangangayaw"),
            new AudioClipData("VolleyShot", "23d2561f5de5e2042b31b4837405839a", "Heroes", "Mangangayaw"),
            
            // MINI BOSSES - Kapre
            new AudioClipData("ForestWrath", "6cbce4f777064354ebfb8dcb41a00771", "MiniBosses", "Kapre"),
            new AudioClipData("TreeSmash", "d4e1f20e98cfaf443a84d194b994ddc5", "MiniBosses", "Kapre"),
            new AudioClipData("UprootSmash", "58d99c1e4913700448bf38e5686fd540", "MiniBosses", "Kapre"),
            
            // MINI BOSSES - Manananggal
            new AudioClipData("BatwingSplash", "f0c6d8f279598834aa2408ca041f4a66", "MiniBosses", "Manananggal"),
            new AudioClipData("BloodSplash", "71055569777979a46847bb66b960406a", "MiniBosses", "Manananggal"),
            new AudioClipData("SplitBody", "333feec285624904396603673abfe68f", "MiniBosses", "Manananggal"),
            
            // MINI BOSSES - Sirena
            new AudioClipData("DrowningCurrent", "f016f7238ad11a849845a05287fa1425", "MiniBosses", "Sirena"),
            new AudioClipData("MoonlightHymn", "13f8ce5f2267ef64a8b04d1beff76643", "MiniBosses", "Sirena"),
            new AudioClipData("TidalSurge", "a3562c2ab997b4c4b8af252915d5578e", "MiniBosses", "Sirena"),
            
            // MINI BOSSES - Tiyanak
            new AudioClipData("BloodHex", "b86eb89ef2da6cd49a199cbd2bee79e5", "MiniBosses", "Tiyanak"),
            new AudioClipData("ClawLatch", "717aa92b6c4aec144a01d5fdfb0a69a6", "MiniBosses", "Tiyanak"),
            new AudioClipData("DemonicWail", "eb5f2136319d938468e9d44e6e9d1d66", "MiniBosses", "Tiyanak"),
        };
    }
}
