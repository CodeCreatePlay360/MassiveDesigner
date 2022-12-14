using System.Collections.Generic;
using UnityEngine;


namespace MassiveDesinger
{
    public abstract class Action
    {
        public abstract void Undo();
        public abstract void Redo();
    }

    [System.Serializable]
    public class ActionManager
    {
        [SerializeField]
        private static Stack<Action> undoQueue = new (30);

        [SerializeField]
        private static Stack<Action> redoQueue = new (30);

        public static void Add(Action actn)
        {
            undoQueue.Push(actn);
        }

        public static void Undo()
        {

            if (undoQueue.Count == 0)
            {
                Debug.Log("undo queue empty");
                return;
            }

            Action undoActn = undoQueue.Pop();
            undoActn.Undo();
            redoQueue.Push(undoActn);
        }

        public static void Redo()
        {
            if (redoQueue.Count == 0)
            {
                Debug.Log("redo queue empty");
                return;
            }

            Action redoActn = redoQueue.Pop();
            redoActn.Redo();
            undoQueue.Push(redoActn);
        }
    }
}
