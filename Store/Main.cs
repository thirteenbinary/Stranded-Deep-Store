using Beam;
using Beam.Terrain;
using BepInEx;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
using System.IO;
using System.Text;
using BepInEx.Logging;
using System.Collections.Generic;
using UnityEngine.UIElements;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;

namespace MyPlugins.Store
{
    [BepInPlugin("Org.Store", "Store", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        // 全局喊话器
        internal new static ManualLogSource Logger;
        // 你到底有多少钱
        internal static ConfigEntry<int> money;
        internal static Work work;
        internal static MyGUI myGUI;
        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("\"Store\" loading successfully!");
            money = Config.Bind("General", "Money", 1000,
                "The money you have in the store, it will be set to 1000 by default.");
        }

        Scene loadGame;
        bool pd = true;
        void Update()
        {
            // 这种设计是考虑到字符串get函数一般很慢，所以在游戏中不要重复调用，在Title处倒无所谓了
            if (loadGame.IsValid())
            {
                if (Input.GetKeyDown(KeyCode.B))
                {
                    // 如果一个对象没有被任何变量引用，那么它将会被回收(garbage collect)，但是你不知道
                    // 什么时候会被回收，但是最终一定会被回收
                    if (pd)
                    {
                        work = new Work();
                        myGUI = new MyGUI();
                        pd = false;
                    }trigger ^= true;
                }
            }
            else
            {
                loadGame = SceneManager.GetSceneByName("Scenes - Load Game");
                pd = true;
            }
        }

        // 决定GUI是否渲染
        bool trigger = false;
        void OnGUI()
        {
            if (trigger)myGUI.Start();
        }
    }
}