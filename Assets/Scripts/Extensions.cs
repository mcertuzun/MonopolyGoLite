using System.Collections.Generic;
using UnityEngine;

namespace MonopolyLite
{
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> GetAllChildren(this Transform root)
        {
            Stack<Transform> stack = new();
            stack.Push(root);
            while (stack.Count > 0)
            {
                Transform t = stack.Pop();
                for (int i = 0; i < t.childCount; i++)
                {
                    Transform c = t.GetChild(i);
                    yield return c;
                    stack.Push(c);
                }
            }
        }
    }
}