using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarCRUD
{
    class MathB
    {
        #region Clamping
        /// <summary>
        /// Clamps a value between the <paramref name="min"/> and <paramref name="max"/>. If it exceeds <paramref name="min"/>, it returns <paramref name="min"/>. So as with <paramref name="max"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static long Clamp(long value, long min, long max)
        {
            long result = value > max ? max : value;
            result = result < min ? min : result;

            return result;
        }

        /// <summary>
        /// Clamps a value between the <paramref name="min"/> and <paramref name="max"/>. If it exceeds <paramref name="min"/>, it returns <paramref name="min"/>. So as with <paramref name="max"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int Clamp(int value, int min, int max)
        {
            int result = value > max ? max : value;
            result = result < min ? min : result;

            return result;
        }

        /// <summary>
        /// Clamps a value between the <paramref name="min"/> and <paramref name="max"/>. If it exceeds <paramref name="min"/>, it returns <paramref name="max"/>. If the value exceeds <paramref name="max"/>, it returns <paramref name="min"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static long InverseClamp(long value, long min, long max)
        {
            long result = value > max ? min : value;
            result = result < min ? max : result;

            return result;
        }

        /// <summary>
        /// Clamps a value between the <paramref name="min"/> and <paramref name="max"/>. If it exceeds <paramref name="min"/>, it returns <paramref name="max"/>. If the value exceeds <paramref name="max"/>, it returns <paramref name="min"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int InverseClamp(int value, int min, int max)
        {
            int result = value > max ? min : value;
            result = result < min ? max : result;

            return result;
        }
        #endregion

        #region Get SubNumbers
        public static async Task<List<MathBSequence>> GetSubNumbersAsync(int _startValue, int _numberToReach, int _maxNumber, int _length)
        {
            return await Task.Run(() => GetSubNumbers(_startValue, _numberToReach, _maxNumber, _length));
        }

        public static List<MathBSequence> GetSubNumbers(int _startValue, int _numberToReach, int _maxNumber, int _length)
        {
            bool ended = false;            
            List<MathBSequence> result = new List<MathBSequence>();
            List<MathBNumber> numbers = new List<MathBNumber>();


            for (int i = 0; i < _length; i++)
            {
                if (i > 0) numbers.Add(new MathBNumber(_startValue, _maxNumber, _numberToReach, numbers[i - 1]));
                else numbers.Add(new MathBNumber(_startValue, _maxNumber, _numberToReach, null));
            }

            while (!ended)
            {               
                bool sequenceResulted = CreateSubSequences(ref numbers);

                if (sequenceResulted)
                {
                    MathBSequence newSeq = GetSequenceFromNumbers(ref numbers);
                    //newSeq.numbers.Sort();

                    MathBSequence match = CheckExistance(newSeq, ref result);

                    if (match == null) result.Add(newSeq);
                    else match.appearance++;
                }
                else ended = true;
            }

            return result;
        }

        private static bool CreateSubSequences(ref List<MathBNumber> _numbers)
        {
            int lastIndex = _numbers.Count - 1;

            while (true)
            {
                int sum = _numbers.Sum(c => c.value);
                MathBNumber.IncrementResult result = _numbers[lastIndex].Increase(sum);

                if (result == MathBNumber.IncrementResult.DesiredSequenceReached)
                    return true;

                if (result == MathBNumber.IncrementResult.MissingParent)
                    return false;
            }
        }

        private static MathBSequence GetSequenceFromNumbers(ref List<MathBNumber> _numbers)
        {
            MathBSequence result = new MathBSequence();

            foreach (MathBNumber number in _numbers)
                result.numbers.Add(number.value);

            return result;
        }

        private static MathBSequence CheckExistance(MathBSequence _source, ref List<MathBSequence> _sequences)
        {
            foreach(MathBSequence current in _sequences)
            {
                bool result = CheckSequenceMatch(_source, current);
                if (result) return current;
            }

            return null;
        }

        private static bool CheckSequenceMatch(MathBSequence _first, MathBSequence _second)
        {
            var first = _first.numbers.Except(_second.numbers);
            var second = _second.numbers.Except(_first.numbers);

            return !first.Any() && !second.Any();
        }
        #endregion

        #region Random
        public static int Random(int min, int max)
        {
            Random rnd = new Random();
            return rnd.Next(min, max);
        }
        #endregion
    }

    class MathBSequence
    {
        public int appearance = 1;
        public List<int> numbers = new List<int>();
    }

    class MathBNumber
    {
        public int value { get; private set; }
        public MathBNumber parent;

        private int startValue = 0;
        private int maxValue = 0;
        private int desiredSequenceSum = 0;

        public MathBNumber(int _startValue, int _maxValue, int _desiredSequenceSum, MathBNumber _parent)
        {
            value = _startValue;
            startValue = _startValue;
            maxValue = _maxValue;
            parent = _parent;
            desiredSequenceSum = _desiredSequenceSum;
        }

        public IncrementResult Increase(int _sequenceSum)
        {
            _sequenceSum -= value;
            value++;            

            //Chech if reached max value
            if(value > maxValue)
            {
                if (parent == null) return IncrementResult.MissingParent;

                value = startValue;
                _sequenceSum += value;
                return parent.Increase(_sequenceSum);
            }

            if ((value + _sequenceSum) == desiredSequenceSum)
                return IncrementResult.DesiredSequenceReached;

            return IncrementResult.Success;
        }

        public enum IncrementResult
        {
            Success,
            MissingParent,
            DesiredSequenceReached
        }
    }
}
