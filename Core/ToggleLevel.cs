using System.Collections;
using Toggle.Core.Function;

namespace Toggle.Core
{
    public class ToggleLevel
    {
        public const int CurrentVersion = 1;

        public int width;
        public int height;
        public FunctionSubType[] buttons;
        public BitArray states;

        public ToggleLevel()
        {
        }

        public ToggleLevel(ToggleLevelReader.Result readerResult)
        {
            width = readerResult.width;
            height = readerResult.height;
            buttons = readerResult.buttons;
            states = readerResult.states;
        }
    
        public static ToggleLevel CreateEmpty(int width, int height)
        {
            ToggleLevel result = new ToggleLevel();
            result.width = width;
            result.height = height;

            int count = width * height;
            result.buttons = new FunctionSubType[count];
            for (int i = 0; i < count; i++)
            {
                result.buttons[i] = FunctionSubType.NOP;
            }

            result.states = new BitArray(count);
            return result;
        }

        public void CopyToGrid(ButtonGrid grid)
        {
            grid.Resize(width, height);

            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    int charIndex = y * grid.width + x;
                    BaseButton button = grid[x, y];
                    button.functionSubType = buttons[charIndex];
                }
            }

            grid.RestoreStates(states);
        }
    }
}