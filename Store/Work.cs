using Beam.Terrain;
using Beam;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Beam.Rendering;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Beam.Utilities;
using Beam.Crafting;
using Beam.UI;
using BepInEx.Configuration;

namespace MyPlugins.Store
{
    // Work类是一个单例
    internal class Work
    {
        // 用于存储用于初始化的GameObject
        internal SortedDictionary<string, ZoneObject> SD = new SortedDictionary<string, ZoneObject>();
        internal SortedDictionary<string, int> SDP, sell;
        internal BindingFlags defaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        bool isInit = false;
        internal Transform playerTransform;
        SlotStorage inventory;

        internal Work()
        {
            isInit = true;
            Main.Logger.LogInfo("Init() Begun!");

            // 获取初始的GameObject，注意它们并没有实体化，所以无法通过Find方法取得
            foreach (BiomeCategory i in Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.
                    BiomeGeneration)
                foreach (ZoneObjects j in i.ProceduralObjects)
                    foreach (ZoneObject k in j.Objects)
                        SD[k.Obj.gameObject.name] = k;

            foreach (ObjectCategory i in Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.
                    ObjectGeneration)
                foreach (ZoneObjects j in i.ProceduralObjects)
                    foreach (ZoneObject k in j.Objects)
                        SD[k.Obj.gameObject.name] = k;

            foreach (ZoneObject i in Singleton<StrandedWorld>.Instance.ProceduralObjectsManager.
                    SheltersGeneration.Objects)
                SD[i.Obj.gameObject.name] = i;

            // 有些Object不允许购买
            SortedDictionary<string, ZoneObject> t = new SortedDictionary<string, ZoneObject>(SD);
            t.Remove("BOAR_SPAWNER"); t.Remove("SNAKE_SPAWNER"); t.Remove("ROCK");
            t.Remove("HOG_SPAWNER"); t.Remove("CLOTH"); t.Remove("SCRAP_CORRUGATED");
            t.Remove("SCRAP_PLANK"); t.Remove("STICK");
            foreach(string i in t.Keys)SD.Remove(i);
            // 设定物品的价格
            SDP = new SortedDictionary<string, int>();
            SDP.Add("BOAR_SPAWNER", 200); SDP.Add("SNAKE_SPAWNER", 100); SDP.Add("ROCK", 10);
            SDP.Add("HOG_SPAWNER", 200); SDP.Add("CLOTH", 20); SDP.Add("STICK", 16);
            SDP.Add("SCRAP_PLANK", 100); SDP.Add("SCRAP_CORRUGATED", 100);

            // 获取Player的位置
            GameObject player = GameObject.Find("Player(Clone)");
            if (player == null) throw new NullReferenceException("Unfind Player!");
            Transform position = player.transform;
            if (position == null) throw new NullReferenceException("Unfind Play Transform!");
            playerTransform = position;

            //获取Player的背包信息
            inventory = (SlotStorage)player.GetComponent<Inventory>().GetSlotStorage();

            Main.Logger.LogInfo("Init() Ended!");
        }

        // 在玩家所在的Zone生成ZoneObject x
        internal void Substantiate(ZoneObject x)
        {
            if (!isInit) throw new Exception("Uninitialize!");
            // 检测是否形参非空
            if (x == null) throw new NullReferenceException("Null ZoneObject!");
            // 获取玩家所在的Zone
            Zone y = WhichZone();
            // 调用OnObjectGenerated方法
            Vector3 temp0 = playerTransform.TransformPoint(Vector3.forward * 2); ;
            GeneratedObject temp = new GeneratedObject
            {
                Prefab = x.Obj.gameObject,
                Position = temp0,
                Rotation = playerTransform.rotation,
                ImpostorParentPrefab = x.Obj.gameObject.GetInterface<IImpostorParent>()
            };
            // Vector3是一个结构体，可以放心大胆地用等于号
            y.OnObjectGenerated(temp);
            // 反射爆破CreateGeneratedPrefab，激活GameObject，一定GameObject要激活才能使用
            Type type = typeof(ZoneLoader);
            MethodInfo method = type.GetMethod("CreateGeneratedPrefab", defaultFlags | BindingFlags.NonPublic);
            if (method == null) throw new NullReferenceException("Unfind method!");
            object[] parameters = new object[] { temp, y };
            method.Invoke(new ZoneLoader(), parameters);
        }

        // 假实体化对象，可能不能保存
        internal void FalseSubstantiate(string x)
        {
            if (!isInit) throw new Exception("Uninitialize!");
            GameObject y = UnityEngine.Object.Instantiate(SD[x].Obj.gameObject);
            y.SetActive(true);
            ((Transform)(y.GetComponent<Transform>())).position = playerTransform.position;
        }

        // 删除玩家背包中的物品x
        internal void Remove(IPickupable x)
        {
            if (!isInit) throw new Exception("Uninitialize!");
            inventory.Pop(x);
        }

        internal List<IPickupable> GetInventory()
        {
            return new List<IPickupable>(inventory.GetStored());
        }

        // 判断玩家所在的Zone，这个游戏是这样一个结构，最大的是world，world下分很多zone
        internal Zone WhichZone()
        {
            if (!isInit) throw new Exception("Uninitialize!");
            Type type = typeof(StrandedWorld);
            MethodInfo method = type.GetMethod("FindClosestZone",
                defaultFlags | BindingFlags.NonPublic, null,
                new Type[] { typeof(Vector3) }, null);
            if (method == null) throw new Exception("No FindClosestZone!");
            // 记住一句话，那就是分析源码，单例是很重要的，尤其找这个类Singleton，这个是一种静态Singleton
            Zone zone = (Zone)method.Invoke(Singleton<StrandedWorld>.Instance,
            new object[] { playerTransform.position });
            if (zone == null) throw new Exception("No zone!");
            return zone;
        }
    }
}