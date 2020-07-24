using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameCore.Enums;

namespace GameCore
{
    public class Countdown
    {
        private static readonly char[] Vowels =
            {'A', 'E', 'I', 'O', 'U'};

        private static readonly char[] Consonants =
            {'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'X', 'Z', 'W', 'Y'};

        private const byte LetterLimitTotal = 9;
        private const byte LetterLimit = 5;
        
        private int UsedTotal => _usedVowels.Count + _usedConsonants.Count;
        private readonly List<char> _usedVowels = new List<char>();
        private readonly List<char> _usedConsonants = new List<char>();
        private uint _score = 0;

        private readonly string[] _words = File.ReadAllLines(@".\words_alpha.txt", Encoding.UTF8);
        private readonly Random _rnd = new Random();

        public Countdown()
        {
        }

        public char? GetLetterOrNull(LetterType letterType)
        {
            if (IsLetterLimitReached())
            {
                return null;
            }
            
            return letterType switch
            {
                LetterType.Vowel => GetVowelOrNull(),
                LetterType.Consonant => GetConsonantOrNull(),
                _ => throw new Exception("Unknown letter type")
            };
        }

        private bool IsLetterLimitReached()
        {
            return UsedTotal >= LetterLimitTotal;
        }
        
        private char? GetVowelOrNull()
        {
            if (_usedVowels.Count >= LetterLimit)
            {
                return null;
            }

            var vowel = Vowels[_rnd.Next(0, Vowels.Length)];
            _usedVowels.Add(vowel);
            
            return vowel;
        }

        private char? GetConsonantOrNull()
        {
            if (_usedConsonants.Count >= LetterLimit)
            {
                return null;
            }

            var consonant = Vowels[_rnd.Next(0, Vowels.Length)];
            _usedConsonants.Add(consonant);
            
            return consonant;
        }

        public IEnumerable<string> GetPossibleWords()
        {
            var allLeters = _usedVowels.Concat(_usedConsonants).ToArray();
            return _words.Where(w => w.IndexOfAny(allLeters) != -1);
        }

        public void Reset()
        {
            _usedVowels.Clear();
            _usedConsonants.Clear();
            _score = 0;
        }
    }
}