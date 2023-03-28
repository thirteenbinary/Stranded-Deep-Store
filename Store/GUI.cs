using Beam;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MyPlugins.Store
{
    internal class MyGUI
    {
        // 渲染MyGUI
        internal Rect position;
        internal void Start()
        {
            position = GUILayout.Window(0, position, Window, "Store");
        }

        // 初始化MyGUI
        GUIStyle toolbar = new GUIStyle("Button");
        internal MyGUI()
        {
            // 初始化窗口大小
            // 要记得对Rect进行new的操作，否则里面的地址将会是null
            position = new Rect();
            position.width = Screen.width >> 1;
            position.x = (Screen.width >> 1) - (Screen.width >> 2);
            position.height = Screen.height >> 1;
            position.y = (Screen.height >> 1) - (Screen.height >> 2);
            // 处理Style
            toolbar.fontSize = 18;
        }

        // 渲染窗口
        int tab = 0;
        void Window(int id)
        {
            tab = GUILayout.Toolbar(tab, new string[] { "Buy", "Sell" }, toolbar);
            GUILayout.Label($"The money you have: {Main.money.Value}");
            switch (tab)
            {
                case 0: RenderBuy(); break;
                case 1: RenderSell(); break;
            }
            // DragWindow一定要放到所有的控件的最后，否则无法正常工作
            GUI.DragWindow();
        }

        // 渲染购买标签页
        // 决定horizontal scroll bar and vertical scroll bar的位置，这不是左上角的位置
        // ScrollView会自动根据上一个GUILayout排列
        Vector2 scrollPosition = Vector2.zero;
        internal void RenderBuy()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            foreach (KeyValuePair<string, ZoneObject> i in Main.work.SD)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i.Key);
                int price = Main.work.SDP[i.Key];
                GUILayout.Label($"{price}");
                if (GUILayout.Button("Buy") && Main.money.Value >= price)
                {
                    Main.work.Substantiate(i.Value);
                    Main.money.Value -= price;
                }GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        // 渲染贩卖标签页
        Vector2 scrollPos = Vector2.zero;
        void RenderSell()
        {
            List<IPickupable> list = Main.work.GetInventory();
            // ScrollView显示区域
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.BeginVertical();
            foreach (IPickupable i in Main.work.GetInventory())
            {
                GUILayout.BeginHorizontal();
                string name = i.gameObject.name;
                GUILayout.Label(name.Substring(0, name.Length-7));
                // 利用string的hashcode来初始化seed，我太懒了，所以就把物品价格定为随机了
                System.Random random = new System.Random(name.GetHashCode());
                int price = random.Next()%10;
                GUILayout.Label($"{price}");
                if (GUILayout.Button("Sell"))
                {
                    Main.work.Remove(i);
                    Main.money.Value += price;
                }GUILayout.EndHorizontal();
                // 插入一个高度位10的空白
                GUILayout.Space(10);
            }GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}
