using System.Collections.Generic;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Game.Common
{
    public class CommandManager : Singleton<CommandManager>
    {
        public int maxCommandCount = 1000;

        private LinkedList<Command> undoList;
        private LinkedList<Command> redoList;

        public event System.Action<Command> onCommandRegister;
        public event System.Action<Command> onUndo;
        public event System.Action<Command> onRedo;

        private void Awake()
        {
            undoList = new LinkedList<Command>();
            redoList = new LinkedList<Command>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            {
                Undo();
            }

            if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.R))
            {
                Redo();
            }
        }

        public void Undo()
        {
            if (undoList.Count > 0)
            {
                Command command = undoList.Last.Value;
                command.Undo();

                undoList.RemoveLast();
                redoList.AddLast(command);

                SolveMaxCommands();
                onUndo?.Invoke(command);
            }
        }

        public void Redo()
        {
            if (redoList.Count > 0)
            {
                Command command = redoList.Last.Value;
                command.Redo();

                redoList.RemoveLast();
                undoList.AddLast(command);

                SolveMaxCommands();
                onRedo?.Invoke(command);
            }
        }

        public void SolveMaxCommands()
        {
            if (undoList.Count > maxCommandCount)
            {
                undoList.RemoveFirst();
            }

            if (redoList.Count > maxCommandCount)
            {
                redoList.RemoveFirst();
            }
        }

        public void Register(Command command)
        {
            undoList.AddLast(command);
            if (undoList.Count > maxCommandCount)
            {
                undoList.RemoveFirst();
            }

            onCommandRegister?.Invoke(command);
        }

        public void ClearHistory()
        {
            undoList.Clear();
            redoList.Clear();
        }

        public bool IsUndoEmpty => (undoList.Count == 0);
        public bool IsRedoEmpty => (redoList.Count == 0);
    }
}