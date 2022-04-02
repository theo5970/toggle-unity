using System.Collections;
using System.Collections.Generic;
using Toggle.Core.Function;
using UnityEngine;

namespace Toggle.Core
{
    public class ButtonGrid
    {
        private List<BaseButton> buttonList;
        public BaseButton this[int x, int y]
        {
            get
            {
                int index = y * width + x;
                return buttonList[index];
            }
            set
            {
                int index = y * width + x;
                buttonList[index] = value;
            }
        }

        public BaseButton this[Vector2Int coord]
        {
            get
            {
                return this[coord.x, coord.y];
            }
            set
            {
                this[coord.x, coord.y] = value;
            }
        }

        /// <summary>
        /// 가로 크기
        /// </summary>
        public int width { get; private set; }
    
        /// <summary>
        /// 세로 크기
        /// </summary>
        public int height { get; private set; }

        public int totalButtons => width * height;

        public ButtonGrid()
        {
            buttonList = new List<BaseButton>();
        }

        public void Resize(Vector2Int size)
        {
            Resize(size.x, size.y);
        }
    
        /// <summary>
        /// 그리드 크기 재조정
        /// </summary>
        /// <param name="newWidth">가로 크기</param>
        /// <param name="newHeight">세로 크기</param>
        public void Resize(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;

            int diffCount = (newHeight * newWidth) - buttonList.Count;
            if (diffCount > 0)
            {
                for (int i = 0; i < diffCount; i++)
                {
                    BaseButton button = new BaseButton(this);
                    buttonList.Add(button);
                }
                ReassignCoordinates();
            }
            else
            {
                diffCount *= -1;
                for (int i = 0; i < diffCount; i++)
                {
                    buttonList.RemoveAt(buttonList.Count - 1);
                }
                ReassignCoordinates();
            }
        }
    
        private void ReassignCoordinates()
        {
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int index = r * width + c;

                    buttonList[index].coordinate = new Vector2Int(c, r);
                }
            }
        }

        /// <summary>
        /// 주어진 좌표가 그리드 내에 있는지 확인
        /// </summary>
        /// <param name="coordinate">2차원 좌표</param>
        /// <returns>그리드 안에 있으면 true</returns>
        public bool CheckRange(Vector2Int coordinate)
        {
            return CheckRange(coordinate.x, coordinate.y);
        }

        /// <summary>
        /// 주어진 좌표가 그리드 내에 있는지 확인
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>그리드 안에 있으면 true</returns>
        public bool CheckRange(int x, int y)
        {
            return (x >= 0 && x < width) && (y >= 0 && y < height);
        }

        /// <summary>
        /// 주어진 좌표에 있는 버튼을 반환 시도
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="btn">결과 버튼</param>
        /// <returns>좌표가 그리드 내 포함여부</returns>
        public bool TryGetAt(int x, int y, out BaseButton btn)
        {
            return TryGetAt(new Vector2Int(x, y), out btn);
        }

        /// <summary>
        /// 주어진 좌표에 있는 버튼을 반환 시도
        /// </summary>
        /// <param name="coordinate">좌표</param>
        /// <param name="btn">결과 버튼</param>
        /// <returns>좌표가 그리드 내 포함여부</returns>
        public bool TryGetAt(Vector2Int coordinate, out BaseButton btn)
        {
            if (CheckRange(coordinate))
            {
                int index = coordinate.y * width + coordinate.x;
                btn = buttonList[index];
                return (btn != null);
            }
            else
            {
                btn = null;
                return false;
            }
        }

        /// <summary>
        /// 켜진 버튼의 개수를 반환
        /// </summary>
        public int GetActiveCount()
        {
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (this[x, y].isOn) count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 그리드 내 모든 버튼의 상태를 바꾼다
        /// </summary>
        /// <param name="newState">상태 (true이면 On)</param>
        public void SetAllState(bool newState)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    this[x, y].isOn = newState;
                }
            }

        }

        /// <summary>
        /// 그리드의 On/Off 상태를 states에 백업
        /// </summary>
        /// <param name="states">저장할 states 비트배열</param>
        public void BackupStates(BitArray states)
        {
            int index = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    states[index] = this[x, y].isOn;
                    index++;
                }
            }
        }

        public BitArray BackupStates()
        {
            BitArray states = new BitArray(width * height);
            
            int index = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    states[index] = this[x, y].isOn;
                    index++;
                }
            }

            return states;
        }

        /// <summary>
        /// 주어진 states에 따라 그리드의 On/Off 상태를 복원
        /// </summary>
        /// <param name="states">복원할 states 비트배열</param>
        public void RestoreStates(BitArray states)
        {
            int index = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    this[x, y].isOn = states[index];
                    index++;
                }
            }
        }
        
        public void CopyToLevel(ToggleLevel target)
        {
            target.width = width;
            target.height = height;

            int count = target.width * target.height;
            target.buttons = new FunctionSubType[count];

            int index = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    target.buttons[index] = this[x, y].functionSubType;
                    index++;
                }
            }

            target.states = new BitArray(count);
            BackupStates(target.states);
        }

    }
}