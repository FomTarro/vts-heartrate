using UnityEngine;
using System;

/// <summary>
/// Class for objects that exist on launch and need to be loaded in a specific order
/// </summary>
public abstract class OrderedInitializer : MonoBehaviour, IComparable
{   
    [Tooltip("The order in which the Initialize method will be called. Larger values go later.")]
    [SerializeField]
    private int _initializationOrder;

    /// <summary>
    /// Method to do ordered loading within
    /// 
    /// Will be called after Awake()
    /// </summary>
    public abstract void Initialize();

    private static bool _loadingStarted = false;

    private void Start()
    {   
        // the first object to run Start will Initialize all of the currently extant 
        if(!_loadingStarted)
        {
            _loadingStarted = true;
            OrderedInitializer[] objects = FindObjectsOfType<OrderedInitializer>();
            Array.Sort(objects);
            foreach(OrderedInitializer o in objects)
            {
                Debug.LogFormat("Initializing {0}", o.name);
                o.Initialize();
            }
        }
    }

    public int CompareTo(object obj)
    {
       try
       {
           OrderedInitializer o = (OrderedInitializer)obj;
           return _initializationOrder - o._initializationOrder;
       }
       catch
       {
           return 0;
       }
    }
}