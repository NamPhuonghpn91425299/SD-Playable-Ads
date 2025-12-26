// #if UNITY_EDITOR
// using UnityEngine;
// using UnityEditor;
//
// public class BotManagerWindow : EditorWindow
// {
//     private Vector2 scrollPosition;
//     private bool showBotList = true;
//
//     [MenuItem("Tools/Bot Killer")]
//     public static void ShowWindow()
//     {
//         BotManagerWindow window = GetWindow<BotManagerWindow>("Bot Killer");
//         window.minSize = new Vector2(800, 800);
//         window.maxSize = new Vector2(1080, 1920);
//     }
//
//     private void OnGUI()
//     {
//         GUILayout.Space(10);
//         
//         // Title
//         EditorGUILayout.LabelField("🤖 Bot Manager", EditorStyles.boldLabel);
//         GUILayout.Space(10);
//
//         scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
//
//         // Bot Info section
//         DrawBotInfoSection();
//         
//         GUILayout.Space(10);
//         
//         // Bot List section
//         DrawBotListSection();
//         
//         GUILayout.Space(10);
//         
//         // Action buttons
//         DrawActionButtons();
//
//         EditorGUILayout.EndScrollView();
//     }
//
//     private void DrawBotInfoSection()
//     {
//         EditorGUILayout.BeginVertical("box");
//         EditorGUILayout.LabelField("📊 Bot Statistics:", EditorStyles.boldLabel);
//         
//         BotNetwork[] allBots = FindObjectsOfType<BotNetwork>();
//         // int activeBots = 0;
//         // int deadBots = 0;
//         // int inactiveBots = 0;
//         //
//         // foreach (var bot in allBots)
//         // {
//         //     if (bot.gameObject.activeInHierarchy)
//         //     {
//         //         if (!bot.IsDead)
//         //             activeBots++;
//         //         else
//         //             deadBots++;
//         //     }
//         //     else
//         //     {
//         //         inactiveBots++;
//         //     }
//         // }
//
//         //EditorGUILayout.LabelField($"🟢 Active Bots: {activeBots}");
//         //EditorGUILayout.LabelField($"💀 Dead Bots: {deadBots}");
//         //EditorGUILayout.LabelField($"⚫ Inactive Bots: {inactiveBots}");
//         EditorGUILayout.LabelField($"📈 Total Bots: {allBots.Length}");
//         
//         EditorGUILayout.EndVertical();
//     }
//
//     private void DrawBotListSection()
//     {
//         EditorGUILayout.BeginVertical("box");
//         
//         showBotList = EditorGUILayout.Foldout(showBotList, "📋 Bot List", true);
//         
//         if (showBotList)
//         {
//             BotNetwork[] allBots = FindObjectsOfType<BotNetwork>();
//             
//             if (allBots.Length == 0)
//             {
//                 EditorGUILayout.HelpBox("No bots found in scene", MessageType.Info);
//             }
//             else
//             {
//                 for (int i = 0; i < allBots.Length; i++)
//                 {
//                     var bot = allBots[i];
//                     if (bot == null) continue;
//
//                     EditorGUILayout.BeginHorizontal();
//                     
//                     // Bot status icon and name
//                     string statusIcon = bot.gameObject.activeInHierarchy ? 
//                         (bot.IsDead ? "💀" : "🟢") : "⚫";
//                     
//                     EditorGUILayout.LabelField($"{statusIcon} {bot.name}", GUILayout.Width(200));
//                     
//                     // Health info
//                     if (bot.gameObject.activeInHierarchy && !bot.IsDead)
//                     {
//                         EditorGUILayout.LabelField($"HP: {bot.currentHealth}", GUILayout.Width(80));
//                     }
//                     
//                     // Individual kill button
//                     GUI.backgroundColor = Color.red;
//                     if (GUILayout.Button("Kill", GUILayout.Width(50)) && 
//                         bot.gameObject.activeInHierarchy && !bot.IsDead)
//                     {
//                         KillBot(bot);
//                     }
//                     GUI.backgroundColor = Color.white;
//                     
//                     // Select button
//                     if (GUILayout.Button("Select", GUILayout.Width(60)))
//                     {
//                         Selection.activeGameObject = bot.gameObject;
//                         EditorGUIUtility.PingObject(bot.gameObject);
//                     }
//                     
//                     EditorGUILayout.EndHorizontal();
//                 }
//             }
//         }
//         
//         EditorGUILayout.EndVertical();
//     }
//
//     private void DrawActionButtons()
//     {
//         EditorGUILayout.BeginVertical("box");
//         
//         // Main Kill Button
//         GUI.backgroundColor = Color.red;
//         if (GUILayout.Button("💀 KILL ALL BOTS", GUILayout.Height(35)))
//         {
//             KillAllBotsOnScene();
//         }
//         GUI.backgroundColor = Color.white;
//         
//         GUILayout.Space(5);
//         
//         // Additional action buttons
//         // EditorGUILayout.BeginHorizontal();
//         //
//         // if (GUILayout.Button("🔄 Refresh"))
//         // {
//         //     Repaint();
//         // }
//         //
//         // if (GUILayout.Button("🎯 Find Player"))
//         // {
//         //     var player = FindObjectOfType<LocalPlayer>();
//         //     if (player != null)
//         //     {
//         //         Selection.activeGameObject = player.gameObject;
//         //         EditorGUIUtility.PingObject(player.gameObject);
//         //         SceneView.FrameLastActiveSceneView();
//         //     }
//         // }
//         //
//         // EditorGUILayout.EndHorizontal();
//         
//         EditorGUILayout.EndVertical();
//     }
//
//     private void KillBot(BotNetwork bot)
//     {
//         if (bot == null || bot.IsDead) return;
//
//         try
//         {
//             bot.CacularHealth(new DamageInfo
//             {
//                 damage = bot.currentHealth,
//                 damageType = DamageType.Normal,
//                 name = "Editor Individual Kill"
//             });
//             
//             Debug.Log($"🔥 [Bot Manager] Killed bot: {bot.name}");
//             //ShowNotification(new GUIContent($"Killed {bot.name}!"));
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogWarning($"Failed to kill bot {bot.name}: {e.Message}");
//         }
//         
//         Repaint();
//     }
//
//     private void KillAllBotsOnScene()
//     {
//         BotNetwork[] allBots = Resources.FindObjectsOfTypeAll<BotNetwork>();
//         int killedCount = 0;
//
//         foreach (BotNetwork bot in allBots)
//         {
//             if (bot != null && 
//                 bot.gameObject.activeInHierarchy && 
//                 !bot.IsDead &&
//                 bot.gameObject.scene.isLoaded)
//             {
//                 try
//                 {
//                     bot.CacularHealth(new DamageInfo
//                     {
//                         damage = bot.currentHealth,
//                         damageType = DamageType.Normal,
//                         name = "Editor Kill All"
//                     });
//                     killedCount++;
//                 }
//                 catch (System.Exception e)
//                 {
//                     Debug.LogWarning($"Failed to kill bot {bot.name}: {e.Message}");
//                 }
//             }
//         }
//
//         Debug.Log($"🔥 [Bot Manager] Killed {killedCount} bots on scene");
//         //ShowNotification(new GUIContent($"Killed {killedCount} bots!"));
//         
//         Repaint();
//     }
//     
//     // Auto refresh mỗi giây
//     private void OnInspectorUpdate()
//     {
//         Repaint();
//     }
// }
// #endif