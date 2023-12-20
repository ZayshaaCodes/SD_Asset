using System.Collections;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public static class CoroutineUtilities
{
    public static IEnumerator AsCoroutine(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
    }

    public static IEnumerator AsCoroutine<T>(this Task<T> task, System.Action<T> resultCallback)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
        else
        {
            resultCallback?.Invoke(task.Result);
        }
    }

    public static Coroutine StartAsync(Task task, MonoBehaviour owner)
    {
        return owner.StartCoroutine(task.AsCoroutine());
    }
    public static Coroutine StartAsync<T>(Task<T> task, MonoBehaviour owner, System.Action<T> resultCallback)
    {
        return owner.StartCoroutine(task.AsCoroutine(resultCallback));
    }
    public static EditorCoroutine StartAsync(Task task, Object owner)
    {
        return EditorCoroutineUtility.StartCoroutine(task.AsCoroutine(), owner);
    }
    public static EditorCoroutine StartAsync<T>(Task<T> task, Object owner, System.Action<T> resultCallback)
    {
        return EditorCoroutineUtility.StartCoroutine(task.AsCoroutine(resultCallback), owner);
    }

    public static IEnumerator RunAsync(Task task, Object owner)
    {
        yield return StartAsync(task, owner);
    }
    public static IEnumerator RunAsync(Task task, MonoBehaviour owner)
    {
        yield return StartAsync(task, owner);
    }

    public static IEnumerator RunAsync<T>(Task<T> task, Object owner, System.Action<T> resultCallback)
    {
        yield return StartAsync(task, owner, resultCallback);
    }

    public static IEnumerator RunAsync<T>(Task<T> task, MonoBehaviour owner, System.Action<T> resultCallback)
    {
        yield return StartAsync(task, owner, resultCallback);
    }

}