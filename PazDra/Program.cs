using System;
using System.Timers;

namespace PazDra
{
    class Program
    {
        // 升目情報
        private const int _GRID_ROW = 8;
        private const int _GRID_COLUMN = 8;

        // Cusol
        private static int _CurSolRow;
        private static int _CurSolCol;

        // 選択
        private static int _SelectedRow =-1;
        private static int _SelectedCol = -1;


        enum CELL_TYPE {
            CELL_TYPE_NONE = 0,
            CELL_TYPE_0 = 1,
            CELL_TYPE_1 = 2,
            CELL_TYPE_2 = 3,
            CELL_TYPE_3 = 4,
            CELL_TYPE_4 = 5,
            CELL_TYPE_5 = 6,
            CELL_TYPE_6 = 7,
            CELL_TYPE_MAX = 8,
        }

        private static char[] cellAA = {
             '・',
             '〇',
             '△',
             '□',
             '●',
             '▲',
             '■',
             '☆',
        };
        // セル状態
        private static int[,] cells = new int[_GRID_ROW,_GRID_COLUMN];

        // セル確認
        private static bool[,] checkedCell = new bool[_GRID_ROW, _GRID_COLUMN];

        // ユーザー入力LOCK
        private static bool locked = false;

        static void Main(string[] args)
        {
            Random r = new Random((int)DateTime.Now.Ticks);

            for (var x = 0; x < _GRID_ROW; x++)
            {
                for (var y = 0; y < _GRID_COLUMN; y++)
                {
                    cells[y, x] = (int)CELL_TYPE.CELL_TYPE_0 + r.Next(7);
                }
            }

            // タイマーの間隔(ミリ秒)
            DateTime time = DateTime.Now;
            // 描写
            while (true)
            {
                var timespan = DateTime.Now - time;

                if (0 < timespan.Seconds)
                {
                    time = DateTime.Now;
                    if(locked)
                    {
                        locked = false;
                        for (var x = _GRID_ROW -2; x >=0;x--)
                        {
                            for (var y = 0; y < _GRID_COLUMN; y++)
                            {
                                if(cells[x,y] != (int)CELL_TYPE.CELL_TYPE_NONE
                                    && cells[x + 1,y] == (int)CELL_TYPE.CELL_TYPE_NONE)
                                {
                                    cells[x + 1, y] = cells[x, y];
                                    cells[x, y] = (int)CELL_TYPE.CELL_TYPE_NONE;
                                    locked = true;
                                }
                            }
                        }
                        r = new Random((int)DateTime.Now.Ticks);
                        for (var y = 0; y < _GRID_COLUMN; y++)
                        {
                            if (cells[0, y] == (int)CELL_TYPE.CELL_TYPE_NONE)
                            {
                                cells[0, y] = (int)CELL_TYPE.CELL_TYPE_0 + r.Next(7);
                                locked = true;
                            }
                        }
                        if(locked == false)
                        {
                            eraseConnectedBlockAll();
                        }
                    }
                    Dispay();
                }
                if (Console.KeyAvailable && locked == false)
                {
                    // ユーザー入力
                    var key = Console.ReadKey().KeyChar;
                    switch (key)
                    {
                        case 'w': _CurSolRow--; break;
                        case 's': _CurSolRow++; break;
                        case 'a': _CurSolCol--; break;
                        case 'd': _CurSolCol++; break;
                        default:
                            if (_SelectedCol < 0)
                            {
                                _SelectedCol = _CurSolCol;
                                _SelectedRow = _CurSolRow;

                            }
                            else
                            {
                                int distance = Math.Abs(_SelectedCol - _CurSolCol) + Math.Abs(_SelectedRow - _CurSolRow);

                                if(distance == 0)
                                {
                                    _SelectedCol = _SelectedRow = -1;
                                }

                                if (distance == 1)
                                {
                                    int temp = cells[_CurSolRow, _CurSolCol];
                                    cells[_CurSolRow, _CurSolCol] = cells[_SelectedRow, _SelectedCol];
                                    cells[_SelectedRow, _SelectedCol] = temp;
                                    
                                    eraseConnectedBlockAll();
                                    _SelectedCol = _SelectedRow = -1;
                                    locked = true;
                                }
                                else
                                {
                                    Console.WriteLine("\a");
                                }

                            }
                            break;
                    };
                    Dispay();
                }
            }
        }


        static int GetConnectedBlockCount(int x,int y, int cellType, int count)
        {
            if(x < 0 || x >= _GRID_ROW || y < 0 || y >= _GRID_COLUMN || checkedCell[x,y]
                || cells[x,y] ==(int)CELL_TYPE.CELL_TYPE_NONE
                || cells[x, y] != cellType)
            {
                return count;
            }

            count++;
            checkedCell[x, y] = true;


            count = GetConnectedBlockCount(x,y -1 ,cellType,count);
            count = GetConnectedBlockCount(x-1, y, cellType, count);
            count = GetConnectedBlockCount(x, y+1, cellType, count);
            count = GetConnectedBlockCount(x+1, y, cellType, count);


            return count;
        }

        static void eraseConnectedBlock(int x, int y, int cellType)
        {

            if (x < 0 || x >= _GRID_ROW || y < 0 || y >= _GRID_COLUMN
                || cells[x, y] == (int)CELL_TYPE.CELL_TYPE_NONE
                || cells[x, y] != cellType)
            {
                return;
            }
            cells[x, y] = (int)CELL_TYPE.CELL_TYPE_NONE;


            eraseConnectedBlock(x, y-1, cellType);
            eraseConnectedBlock(x-1, y, cellType);
            eraseConnectedBlock(x, y+1, cellType);
            eraseConnectedBlock(x+1, y, cellType);
        }

        static void eraseConnectedBlockAll()
        {
            Array.Clear(checkedCell, 0, checkedCell.Length);
            // 描写
            for (var x = 0; x < _GRID_ROW; x++)
            {
                for (var y = 0; y < _GRID_COLUMN; y++)
                {
                    var n = GetConnectedBlockCount(x, y, cells[x, y], 0);
                    if (n >= 3)
                    {
                        eraseConnectedBlock(x, y, cells[x, y]);
                        locked = true;
                    }
                }
            }
        }

        static void Dispay()
        {
            // コンソール画面クリア
            Console.Clear();
            // 描写
            for (var x = 0; x < _GRID_ROW; x++)
            {
                for (var y = 0; y < _GRID_COLUMN; y++)
                {
                    if (_CurSolRow == x && _CurSolCol == y && locked == false)
                    {
                        Console.Write("◎");
                    }
                    else
                    {
                        Console.Write(cellAA[cells[x, y]]);
                    }
                }
                if (x == _SelectedRow)
                {
                    Console.Write("←");
                }
                Console.WriteLine("");
            }

            for (var y = 0; y < _GRID_COLUMN; y++)
            {
                Console.Write((y == _SelectedCol) ? "↑" : "　");
            }
        }
    }
}
