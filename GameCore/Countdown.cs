using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GameCore.Enums;
using GameCore.Models;

namespace GameCore
{
    /// <summary>
    /// Contains game logic, rules and state
    /// </summary>
    public class Countdown
    {
        private static readonly char[] Vowels =
            {'A', 'E', 'I', 'O', 'U'};

        private static readonly char[] Consonants =
            {'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'X', 'Z', 'W', 'Y'};

        // game rules to use in state
        private const byte MaxLettersTotal = 9;
        private const byte MaxLettersPerType = 5;
        private const byte MaxRounds = 4;
        
        public State State { get; }

        private readonly string[] _words;
        private readonly Random _rnd = new Random();

        public Countdown()
        {
            // load words into memory
            var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _words = File.ReadAllLines(@$"{workingDir}\words_alpha.txt", Encoding.UTF8);

            // sort & filter words.
            // no longer needed as results have been written to file, keeping in case dictionary changes
            // Array.Sort(_words, new CharAndLengthComparer().Compare);
            // _words = _words.Where(w => w.Length <= 9 || w.Length < 2).ToArray();

            // initialise state
            State = new State(MaxLettersPerType, MaxLettersTotal, MaxRounds);
        }

        /// <summary>
        /// Fetches a random letter of specific type and updates state.
        /// </summary>
        /// <param name="type">Type of letter to fetch</param>
        /// <returns>requested letter of type or null if this letter type limit
        /// or total letter limit has been exhausted</returns>
        public char? GetLetterAndUpdateStateOrNull(LetterType type)
        {
            if (State.IsTotalLetterLimitReached())
            {
                return null;
            }
            if (State.IsLetterLimitReached(type))
            {
                return null;
            }

            var letter = type switch
            {
                LetterType.Vowel => GetVowel(),
                LetterType.Consonant => GetConsonant(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)

            };

            State.AddUsedLetter(type, letter);
            return letter;
        }

        private char GetVowel()
        {
            return Vowels[_rnd.Next(0, Vowels.Length)];
        }

        private char GetConsonant()
        {
            return Consonants[_rnd.Next(0, Consonants.Length)];
        }

        public string GetLongestWord()
        {
            return "fakeword";
        }
        
        private bool IsValidWord(string guess)
        {
            return _words.Contains(guess);
        }

        public bool IsValidGuessAndUpdateScore(string guess)
        {
            // todo: check guess uses letters in state
            
            if (IsValidWord(guess))
            {
                State.AddPointsToScore(guess.Length);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Start new round and return state.
        /// </summary>
        /// <returns>New round state or Null if no more rounds left</returns>
        /// <exception cref="Exception"></exception>
        public State StartNewRoundAndGetStateOrNull()
        {
            if (State.IsRoundLimitReached)
            {
                throw new Exception($"Cannot start new round - maximum of {MaxRounds} rounds reached");
            }
            
            return State.NewRound();
        }
        
        public State ResetGame()
        {
            return State.Reset();
        }
    }
}