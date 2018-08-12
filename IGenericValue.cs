
using UnityEngine.Events;

/// <summary>
/// This interface is to be used when you want to retrieve a value, but do not want to change it.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="T1"></typeparam>
public interface IGenericValue<out T>
{
    T Value { get; }
}

