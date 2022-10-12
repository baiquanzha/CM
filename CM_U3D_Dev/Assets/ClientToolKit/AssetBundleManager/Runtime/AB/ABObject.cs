using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


public sealed class ABObject
{
//    public ABObject(ABRequest _req)
//    {
//        Request = _req;
//    }
//
//    public void Delete()
//    {
//        GameObject.DestroyImmediate(Obj);
//        Request.Delete();
//        Obj = null;
//        Request = null;
//        pool.Enqueue(this);
//    }
//
//    public GameObject Obj = null;
//    public ABRequest Request = null;
//    public Vector3 Position, Rotation;
//    public bool UseDefaultPos = true;
//    public static ABObject Get(ABRequest r)
//    {
//        ABObject abObj = null;
//        if (pool.Count > 0)
//        {
//            abObj = pool.Dequeue();
//            abObj.Request = r;
//        }
//        else
//        {
//            abObj = new ABObject(r);
//        }
//        return abObj;
//    }
//    private static Queue<ABObject> pool = new Queue<ABObject>();

}

public delegate void ABObjectCallBack(GameObject obj);
