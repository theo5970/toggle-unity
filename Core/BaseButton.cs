
using Toggle.Core.Function;
using UnityEngine;
using Utils;

namespace Toggle.Core
{
    public class BaseButton
    {
        public Vector2Int coordinate;
        public ButtonGrid grid { get; private set; }
        public bool isOn;

        public BaseFunction function { get; private set; }

        public FunctionSubType functionSubType
        {
            get => _functionSubType;
            set
            {
                function = TypeUtils.GetFunctionBySubType(value);
                _functionSubType = value;
            }
        }
        private FunctionSubType _functionSubType;

        public BaseButton(ButtonGrid grid)
        {
            this.grid = grid;
        }
        public void SetGrid(ButtonGrid grid)
        {
            this.grid = grid;
        }

        public void Action()
        {
            function.OnClicked(this);
        }

        public void Toggle()
        {
            isOn = !isOn;
        }
    }
}