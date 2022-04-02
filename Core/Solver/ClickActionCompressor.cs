using System.Collections;
using System.Collections.Generic;
using Toggle.Core.Function;
using Toggle.Utils;
using UnityEngine;
using Utils;

namespace Toggle.Core.Solver
{
    public class ClickActionCompressor
    {
        struct CompressorInput
        {
            public BaseButton button;
            public BitArray statesAfterClick;
        }

        private readonly List<BaseButton> input;
        private List<BaseButton> output;

        private readonly List<CompressorInput> compressorInputs;
        private int inputCount;

        public ClickActionCompressor()
        {
            compressorInputs = new List<CompressorInput>();
            input = new List<BaseButton>();
            output = new List<BaseButton>();
        }

        /// <summary>
        /// 입력 리스트를 초기화합니다
        /// </summary>
        public void Clear()
        {
            input.Clear();
        }

        /// <summary>
        /// 버튼을 입력 리스트에 추가
        /// </summary>
        /// <param name="button">버튼</param>
        public void Add(BaseButton button)
        {
            input.Add(button);
        }

        /// <summary>
        /// 버튼들을 입력 리스트에 추가
        /// </summary>
        /// <param name="buttons">버튼 IEnumerable</param>
        public void AddRange(IEnumerable<BaseButton> buttons)
        {
            input.AddRange(buttons);
        }

        /// <summary>
        /// 버튼들을 입력 리스트에 추가
        /// </summary>
        public void AddRange(ButtonGrid grid, IEnumerable<Vector2Int> coords)
        {
            foreach (Vector2Int coord in coords)
            {
                input.Add(grid[coord]);
            }
        }

        private BitArray tempStates;


        /// <summary>
        /// 상태 최적화하면서 클릭 순서를 압축
        /// </summary>
        /// <param name="outputList">압축된 순서리스트</param>
        /// <param name="firstStates">처음 그리드의 상태</param>
        public void Compress(List<BaseButton> outputList, BitArray firstStates)
        {
            output = outputList;

            compressorInputs.Clear();
            output.Clear();

            if (input.Count == 0) return;

            ButtonGrid grid = input[0].grid;
            inputCount = input.Count;

            tempStates = new BitArray(grid.width * grid.height);
            grid.BackupStates(tempStates);
            grid.RestoreStates(firstStates);
        
            for (int i = 0; i < inputCount; i++)
            {
                var compressorInput = new CompressorInput
                {
                    button = input[i],
                    statesAfterClick = new BitArray(grid.width * grid.height),
                };
                input[i].Action();
                grid.BackupStates(compressorInput.statesAfterClick);
                compressorInputs.Add(compressorInput);
                // Debug.Log(compressorInput.statesAfterClick.ToBinaryString());
            }
        
            grid.RestoreStates(tempStates);
        
            for (int i = 0; i < input.Count; i++)
            {
                CompressorInput data = compressorInputs[i];
                ProcessJump(ref i, data);
            }

            // Debug.Log("After Jump: " + compressorInputs.Count);

            if (input.Count > 0)
            {
                CompressorInput lastCompressorInput = compressorInputs[compressorInputs.Count - 1];

                if (CompareStates(lastCompressorInput.statesAfterClick, firstStates))
                {
                    input.Clear();
                    compressorInputs.Clear();
                }
                else
                {
                    RemoveDuplicatePart();
                }
            }
        }


        /// <summary>
        /// 두 개의 BitArray가 일치하는 지 비교
        /// </summary>
        /// <param name="firstStates">첫 번째 BitArray</param>
        /// <param name="secondStates">두 번째 BitArray</param>
        /// <returns>일치 여부</returns>
        private bool CompareStates(BitArray firstStates, BitArray secondStates)
        {
            if (firstStates.Length != secondStates.Length) return false;
            for (int i = 0; i < firstStates.Length; i++)
            {
                if (firstStates[i] != secondStates[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// 클릭 순서에서 상태(states)가 겹치는 부분들을 제거한다
        /// ex) ABCDCBE -> 1, 5번째에 B가 있으므로 1~5번을 제거.
        /// </summary>
        /// <param name="index">참조 인덱스</param>
        /// <param name="data">CompressorInput</param>
        private void ProcessJump(ref int index, CompressorInput data)
        {
            int jumpEndIndex = -1;
            for (int k = index + 1; k < compressorInputs.Count; k++)
            {
                CompressorInput otherData = compressorInputs[k];

                if (CompareStates(data.statesAfterClick, otherData.statesAfterClick))
                {
                    jumpEndIndex = k;
                }
            }

            if (jumpEndIndex != -1)
            {
                int removeStart = index + 1;
                int removeCount = (jumpEndIndex - removeStart + 1);
                for (int k = 0; k < removeCount; k++)
                {
                    compressorInputs.RemoveAt(removeStart);
                    input.RemoveAt(removeStart);
                }
            
            }
        
        }

        struct ButtonCounter
        {
            public BaseButton button;
            public int counter;

            public ButtonCounter(BaseButton button)
            {
                this.button = button;
                counter = 1;
            }
        }

        private readonly List<int> sameIndices = new List<int>();

        private Stack<ButtonCounter> stack = new Stack<ButtonCounter>(128);
        private void RemoveDuplicatePart()
        {
            stack.Clear();
            stack.Push(new ButtonCounter(input[0]));
            for (int i = 1; i < input.Count; i++)
            {
                var button = input[i];

                if (stack.Count > 0)
                {
                    var otherButtonCounter = stack.Pop();

                    if (button.coordinate != otherButtonCounter.button.coordinate)
                    {
                        stack.Push(otherButtonCounter);
                        stack.Push(new ButtonCounter(button));
                    }
                    else
                    {
                        int newCounter = otherButtonCounter.counter++;

                        newCounter++;
                        FunctionSubType functionSubType = otherButtonCounter.button.functionSubType;
                        switch (functionSubType)
                        {
                            case FunctionSubType.RC:
                            case FunctionSubType.RCC:
                                newCounter %= 8;
                                break;
                            case FunctionSubType.SYH:
                            case FunctionSubType.SYV:
                                newCounter %= 2;
                                break;
                            case FunctionSubType.SHL:
                            case FunctionSubType.SHR:
                                newCounter %= otherButtonCounter.button.grid.width;
                                break;
                            default:
                                newCounter %= 2;
                                break;
                        }

                        if (newCounter != 0)
                        {
                            otherButtonCounter.counter = newCounter;
                            stack.Push(otherButtonCounter);
                        }
                    }
                }
                else
                {
                    stack.Push(new ButtonCounter(button));
                }
            }

            while (stack.Count > 0)
            {
                var counter = stack.Pop();
                for (int i = 0; i < counter.counter; i++)
                {
                    output.Add(counter.button);
                }
            }

            output.Reverse();

            for (int i = 0; i < output.Count; i++)
            {
                BaseButton button = output[i];
                if (TypeUtils.CheckIfOrderImportant(button.functionSubType)) continue;

                sameIndices.Clear();

                for (int j = i + 1; j < output.Count; j++)
                {
                    BaseButton otherButton = output[j];
                    if (TypeUtils.CheckIfOrderImportant(otherButton.functionSubType)) break;

                    if (button.coordinate == otherButton.coordinate)
                    {
                        sameIndices.Add(j);
                    }
                }

                if (sameIndices.Count > 0)
                {
                    bool isCountEven = (sameIndices.Count + 1) % 2 == 0;
                    int removeCount = 0;

                    if (isCountEven)
                    {
                        output.RemoveAt(i);
                        i--;
                        removeCount++;
                    }

                    for (int k = 0; k < sameIndices.Count; k++)
                    {
                        output.RemoveAt(sameIndices[k] - removeCount);
                        removeCount++;
                    }
                }
            }
        }
    }
}