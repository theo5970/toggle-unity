#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Toggle.Core;
using Toggle.Core.Generator;
using Toggle.Game.Data;
using UnityEngine;

namespace Toggle.Game.Mode
{
    public class GeneratorMode : GameMode
    {
        private LevelGenerator generator;

        public override void Prepare()
        {
            base.Prepare();

            generator = new LevelGenerator();
            generator.grid = map.grid;

            gameManager.modeText.color = Color.cyan;
            gameManager.modeText.text = "레벨 생성모드";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(GenerateLevels());
            }
        }

        private bool CheckIfStateSymmetry()
        {
            // 가로 기준 체크
            bool horizontalCheck = true;
            bool verticalCheck = true;
            bool diagonalCheck = true;

            var grid = map.grid;
            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    if (x == grid.width / 2) continue;

                    int flipX = grid.width - 1 - x;
                    if (grid[flipX, y].isOn != grid[x, y].isOn)
                    {
                        horizontalCheck = false;
                    }
                }
            }

            // 세로 기준 체크
            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    if (y == grid.height / 2) continue;

                    int flipY = grid.height - 1 - y;
                    if (grid[x, flipY].isOn != grid[x, y].isOn)
                    {
                        verticalCheck = false;
                    }
                }
            }

            // 대각선 기준 체크
            for (int y = 0; y < grid.height; y++)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    if (x >= y) continue;

                    Vector2Int flipCoord = new Vector2Int(grid.width - 1 - x, grid.height - 1 - y);
                    if (grid[x, y].isOn != grid[flipCoord].isOn)
                    {
                        diagonalCheck = false;
                    }
                }
            }


            return horizontalCheck || verticalCheck || diagonalCheck;
        }


        private IEnumerator GenerateLevels()
        {
            LevelPack pack = new LevelPack();
            pack.name = "auto-generated";

            for (int i = 0; i < 100; i++)
            {
                ToggleClassicLevel classicLevel = new ToggleClassicLevel
                {
                    data = new ToggleLevel()
                };
                yield return GenerateOne(result => { classicLevel.generateOrders = result.clickOrders; });
                map.grid.CopyToLevel(classicLevel.data);

                pack.levels.Add(classicLevel);
            }

            string projectFolder = Path.Combine(Application.dataPath, "../");
            string outputPath = Path.Combine(projectFolder, "auto-generated " + DateTime.Now.Ticks + ".json");
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(pack, Formatting.Indented));
        }

        private IEnumerator GenerateOne(Action<LevelGenerator.Result> resultCallback = null)
        {
            commandManager.ClearHistory();
            clickCount = 0;

            var generateOptions = new LevelGenerator.Options
            {
                mapSize = new Vector2Int(5, 5),
                typeFlags = FunctionTypeFlags.OneArrowLinear | FunctionTypeFlags.OneArrowDiagonal | FunctionTypeFlags.TwoArrowLinear | FunctionTypeFlags.TwoArrowDiagonal,
                mirrorType = MirrorType.None,
                buttonCount = 13,
                clickCount = 6,
            };
            generator.options = generateOptions;

            LevelGenerator.Result generateResult;
            long iterations = 0;
            do
            {
                generateResult = generator.Generate();
                map.RefreshButtons();

                iterations++;
                if (iterations % 1000 == 0)
                {
                    yield return null;
                }
            } while (!CheckIfStateSymmetry());

            resultCallback?.Invoke(generateResult);

            yield return null;
        }
    }
}
#endif