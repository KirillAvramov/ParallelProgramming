using System;
using System.Threading.Tasks;

namespace AtomicSnapshots
{
    internal class Register
    {
        private static readonly object UpdLock = new object();

        private readonly int _regId;
        private int _data;
        private bool[] _bitmask;
        private bool _toggle;
        private int[] _view;

        public Register(int data, int regId, int n)
        {
            _regId = regId;
            _data = data;
            _bitmask = new bool[n];
            _view = new int[n];
        }

        public bool[] GetBitmask()
        {
            return _bitmask;
        }

        public bool GetToggle()
        {
            return _toggle;
        }

        public int GetData()
        {
            return _data;
        }

        public int[] GetView()
        {
            return _view;
        }

        public void AtomicUpdate(int newData, bool[] newBitmask, bool newToggle, int[] snapshot)
        {

            lock (UpdLock)
            {
                _bitmask = newBitmask;
                _toggle = newToggle;
                _view = snapshot;

                _data = newData;
                Console.WriteLine("Task {0} updated register {2} ; data = {1}\n", Task.CurrentId, _data, _regId);
            }
        }
    }

    internal class RegSnap
    {
        private readonly Register[] _registers;
        private readonly bool[,] _qHandShakes;

        public RegSnap(Register[] regs)
        {
            _registers = regs;
            _qHandShakes = new bool[_registers.Length, _registers.Length];
        }

        public int[] Scan(int ind = 0)
        {
            var moved = new bool[_registers.Length];

            while (true)
            {
                for (var j = 0; j < _registers.Length; j++)
                {
                    _qHandShakes[ind, j] = _registers[j].GetBitmask()[ind];
                }

                var a = Collect();
                var b = Collect();

                var result = true;
                for (var k = 0; k < a.Length; k++)
                {
                    if (a[k].GetBitmask()[ind] == b[k].GetBitmask()[ind] &&
                        b[k].GetBitmask()[ind] == _qHandShakes[ind, k] &&
                        a[k].GetToggle() == b[k].GetToggle()) continue;
                    if (moved[k])
                    {
                        return b[k].GetView();
                    }
                    moved[k] = true;

                    result = false;
                    break;
                }

                if (!result) continue;

                var view = new int[_registers.Length];
                for (var i = 0; i < _registers.Length; i++)
                    view[i] = a[i].GetData();
                return view;
            }
        }

        public void Update(int i, int value)
        {
            var newBitmask = new bool[_registers.Length];
            
            for (var j = 0; j < _registers.Length; j++) newBitmask[j] = !_qHandShakes[j, i];
            
            var view = Scan(i);

            _registers[i].AtomicUpdate(value, newBitmask, !_registers[i].GetToggle(), view);
        }

        private Register[] Collect()
        {
            return (Register[])_registers.Clone();
        }

    }

}