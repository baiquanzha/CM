using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using XLua;
using DG.Tweening;
using MTool.Framework;

[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour {
    const string TAG = "LuaBehaviour ";

    [NonSerialized]
    public bool usingUpdate = true;
    
    //lua函数绑定到委托，
    Action<LuaTable> luaAwake;
    Action<LuaTable> luaStart;
    Action<LuaTable> luaUpdate;
    Action<LuaTable> luaOnDisable;
    Action<LuaTable> luaOnEnable;
    Action<LuaTable> luaOnApplicationQuit;
    Action<LuaTable, bool> luaOnApplicationPause;
    Action<LuaTable> luaOnDestroy;

    public LuaTable table;
    private Transform trans;

    //加载脚本文件
    public void DoFile(string fn) {
        //try {
            Debug.Log(TAG + "DoFile() 开始载入 " + fn);
            LuaTable tbl = GameLauncher.Lua.env.LoadString<Func<LuaTable>>("return require '" + fn + "'")();
            Debug.Log(TAG + "DoFile() 载入 " + fn + " ,tb=" + tbl);
            setBehaviour(tbl);
        //} catch (Exception e) {
        //    Debug.LogError(TAG + "DoFile() " + FormatException(e), gameObject);
        //}
    }

    //设置执行的table对象
    public void setBehaviour(LuaTable myTable) {
        table = myTable;
        
        table.Set("this", this);
        table.Set("transform", transform);
        table.Set("gameObject", gameObject);

        luaAwake = myTable.Get<Action<LuaTable>>("Awake");
        luaStart = myTable.Get<Action<LuaTable>>("Start");
        luaUpdate = myTable.Get<Action<LuaTable>>("Update");
        luaOnDisable = myTable.Get<Action<LuaTable>>("OnDisable");
        luaOnEnable = myTable.Get<Action<LuaTable>>("OnEnable");
        luaOnApplicationQuit = myTable.Get<Action<LuaTable>>("OnApplicationQuit");
        luaOnApplicationPause = myTable.Get<Action<LuaTable, bool>>("OnApplicationPause");
        luaOnDestroy = myTable.Get<Action<LuaTable>>("OnDestroy");

        if (luaAwake != null) {
            luaAwake(table);
        }
    }


    void OnEnable() {
        if (luaOnEnable != null) {
            luaOnEnable(table);
        }
    }

    private void Start() {
        trans = this.transform;
        if (luaStart != null) {
            luaStart(table);
        }
    }

    protected void Update() {
        if (usingUpdate) {
            if (luaUpdate != null) {
                luaUpdate(table);
            }
        }
    }

    void OnDisable() {
        if (luaOnDisable != null) {
            luaOnDisable(table);
        }
    }

    void OnApplicationQuit() {
        if (luaOnApplicationQuit != null) {
            luaOnApplicationQuit(table);
        }
    }

    void OnApplicationPause(bool pauseStatus) {
        if (luaOnApplicationPause != null) {
            luaOnApplicationPause(table, pauseStatus);
        }
    }

    protected void OnDestroy() {
        if (luaOnDestroy != null) {
            luaOnDestroy(table);
        }

        if (table != null) {
            table.Dispose();
        }
        //委托必须置null
        luaAwake = null;
        luaStart = null;
        luaUpdate = null;
        luaOnDisable = null;
        luaOnEnable = null;
        luaOnApplicationQuit = null;
        luaOnDestroy = null;
    }

    public string FormatException(Exception e) {
        string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
        return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
    }
}