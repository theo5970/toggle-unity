using System.Collections;
using Toggle.Core;
using Toggle.Core.Function;

namespace Toggle.Game.Common
{
    [System.Serializable]
    public abstract class Command
    {
        public abstract void Execute(); // 실행
        public abstract void Undo();    // 되돌리기
        public abstract void Redo();    // 다시하기
    }

    [System.Serializable]
    public class SetFunctionCommand : Command
    {
        private GameMap gameMap;
        private BaseButtonView buttonView;

        private FunctionSubType oldSubType;
        private FunctionSubType newSubType;

        public SetFunctionCommand(GameMap map, BaseButtonView btn, FunctionSubType targetSubType)
        {
            gameMap = map;
            buttonView = btn;

            newSubType = targetSubType;
            oldSubType = FunctionSubType.BH;
        }

        public override void Execute()
        {
            oldSubType = buttonView.button.functionSubType;
            buttonView.button.functionSubType = newSubType;
            gameMap.RefreshButtonsOutside();
        }

        public override void Redo()
        {
            buttonView.button.functionSubType = newSubType;
            gameMap.RefreshButtonsOutside();
        }

        public override void Undo()
        {
            buttonView.button.functionSubType = oldSubType;
            gameMap.RefreshButtonsOutside();
        }

        public override string ToString()
        {
            return $"[SetFunctionCommand] Target: {buttonView.name}, OldType: {oldSubType}, NewType: {newSubType}";
        }
    }

    [System.Serializable]
    public class ButtonClickCommand : Command
    {
        public BaseButton button;
        private BitArray lastStates;
        private ButtonGrid grid;

        public ButtonClickCommand(BaseButton button)
        {
            this.button = button;

            grid = button.grid;
            lastStates = new BitArray(grid.width * grid.height);
        }

        public override void Execute()
        {
            grid.BackupStates(lastStates);
            button.Action();
        }

        public override void Redo()
        {
            button.Action();
        }

        public override void Undo()
        {
            grid.RestoreStates(lastStates);
        }
    }
}

// (에디터) 기능설정 명령

// (에디터/인게임) 버튼 클릭 명령