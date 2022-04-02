using System.Collections.Generic;
using Toggle.Core.Function;
using UnityEngine;

namespace Toggle.Core.Solver
{
    public class SequenceReverse
    {
        private List<BaseButton> input = new List<BaseButton>();
        private List<BaseButton> output;

        public void Clear()
        {
            input.Clear();
        }

        public void Add(BaseButton button)
        {
            input.Add(button);
        }

        public void AddRange(IEnumerable<BaseButton> buttons)
        {
            input.AddRange(buttons);
        }
    
        /// <summary>
        /// 주어진 클릭 순서를 역방향으로 변환
        /// </summary>
        /// <returns>성공 여부(?)</returns>
        public void Solve(List<BaseButton> outputList)
        {
            output = outputList;
            output.Clear();

            input.Reverse();
            for (int i = 0; i < input.Count; i++)
            {
                BaseButton button = input[i];

                switch (button.functionSubType)
                {
                    case FunctionSubType.RC:
                    case FunctionSubType.RCC:
                        i = SolveOrderType(i, button, 8);
                        break;
                    case FunctionSubType.NOP:
                        break;
                    case FunctionSubType.SYH:
                    case FunctionSubType.SYV:
                        i = SolveOrderType(i, button, 2);
                        break;
                    case FunctionSubType.SHL:
                    case FunctionSubType.SHR:
                        i = SolveOrderType(i, button, button.grid.width);
                        break;
                    default:
                        i = SolveArrow(i, button);
                        break;
                }
            }
        }
    
        private int SolveArrow(int index, BaseButton button)
        {
            output.Add(button);
            return index;
        }

        private int SolveOrderType(int index, BaseButton button, int cycle)
        {
            int sameCount = 1;

            Vector2Int previousCoordinate = button.coordinate;

            index += 1;
            for (; index < input.Count; index++)
            {
                BaseButton otherButton = input[index];
                if (otherButton.coordinate != previousCoordinate) break;

                sameCount++;
                previousCoordinate = otherButton.coordinate;
            }

            int appendCount = sameCount % cycle;
            if (appendCount != 0)
            {
                appendCount = cycle - appendCount;
            }

            for (int i = 0; i < appendCount; i++)
            {
                output.Add(button);
            }

            return index - 1;
        }
    }
}