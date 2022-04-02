using System.Collections.Generic;
using System.IO;
using Toggle.Utils;
using UnityEngine;

namespace Toggle.Game.Data
{
    /*
     * States
     * 0 : 시도 안함
     * 1 : 시도함
     * 2 : 별 1개
     * 3 : 별 2개
     * 4 : 별 3개 (퍼펙트)
     */
    public class LevelPackSave
    {
        public string packName;
        public List<byte> states;
        public string checksum;

        // 레벨 상태 가져오기
        public LevelTryState GetLevelState(int levelIndex)
        {
            byte value = states[levelIndex];
            switch (value)
            {
                case 0:
                    return LevelTryState.NotTried;
                case 1:
                    return LevelTryState.Tried;
                case 2:
                case 3:
                case 4:
                    return LevelTryState.Clear;
                default:
                    return LevelTryState.NotTried;
            }
        }


        // 레벨 상태 설정하기
        public void SetLevelState(int levelIndex, LevelTryState newState)
        {
            byte newStateByte = 0;
            switch (newState)
            {
                case LevelTryState.NotTried:
                    newStateByte = 0;
                    break;
                case LevelTryState.Tried:
                    newStateByte = 1;
                    break;
                case LevelTryState.Clear:
                    newStateByte = 2;
                    break;
            }

            states[levelIndex] = newStateByte;
        }

        public int GetStarCount(int levelIndex)
        {
            byte value = states[levelIndex];
            if (value <= 1)
            {
                return 0;
            }
            else
            {
                return Mathf.Clamp(value - 1, 1, 3);
            }
        }

        public void SetStarCount(int levelIndex, int starCount)
        {
            byte newStateByte;
            if (starCount < 1 || starCount > 3)
            {
                throw new System.ArgumentException("Star count must be '1 <= x <= 3'!");
            }
            else
            {
                newStateByte = (byte)(1 + starCount);
            }
            states[levelIndex] = newStateByte;
        }

        public int CountClearedLevels()
        {
            int result = 0;
            for (int i = 0; i < states.Count; i++)
            {
                byte levelState = states[i];
                if (levelState >= 2)
                {
                    result++;
                }
            }

            return result;
        }

        public int CountPerfectLevels()
        {
            int result = 0;
            for (int i = 0; i < states.Count; i++)
            {
                byte levelState = states[i];
                if (levelState == 4)
                {
                    result++;
                }
            }
            return result;
        }

        // 변조 방지용 Secret Salt (SHA256 해시에 쓸 예정)

        private static readonly byte[] startSecretSalt = Global.SaveFileSaltStart;
        private static readonly byte[] endSecretSalt = Global.SaveFileSaltEnd;

        public string CalculateChecksum()
        {
            string result;
            using (MemoryStream ms = new MemoryStream())
            {
                for (int i = 0; i < startSecretSalt.Length; i++)
                {
                    ms.WriteByte(startSecretSalt[i]);
                }

                for (int i = states.Count - 1; i >= 0; i--)
                {
                    ms.WriteByte((byte)(states[i] ^ i));
                }

                for (int i = 0; i < endSecretSalt.Length; i++)
                {
                    ms.WriteByte(endSecretSalt[i]);
                }

                ms.Position = 0;
                result = HashUtils.GenerateSHA256(ms);
            }

            return result;
        }
    }
}