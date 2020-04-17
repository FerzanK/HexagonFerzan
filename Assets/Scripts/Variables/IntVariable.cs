using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HexfallFerzan/Variables/IntVariable")]
public class IntVariable : ScriptableObject, ISerializationCallbackReceiver
{
    public int initialValue = 0;

    [NonSerialized]
    public int value = 0;

    public void OnAfterDeserialize()
    {
        value = initialValue;
    }

    public void OnBeforeSerialize() { }
}