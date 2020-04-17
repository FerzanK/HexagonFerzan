using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "HexfallFerzan/Variables/FloatVariable")]
public class FloatVariable : ScriptableObject, ISerializationCallbackReceiver
{
    public float InitialValue;

    [NonSerialized]
    public float RuntimeValue;

    public void OnAfterDeserialize()
    {
        RuntimeValue = InitialValue;
    }

    public void OnBeforeSerialize() { }
}
