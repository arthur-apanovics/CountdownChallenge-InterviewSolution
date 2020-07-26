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
        private const byte MinLettersInWord = 2;

        public State State { get; }

        private readonly List<string> _words;
        private readonly Random _rnd = new Random();

        /// <summary>
        /// Loads word list and initialises state.
        /// </summary>
        public Countdown()
        {
            // load words into memory
            var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // word list from https://github.com/dwyl/english-words/
            // (seems to have a lot of weird words in it but that's not the point of this exercise)
            _words = File .ReadAllLines(@$"{workingDir}\words_alpha.txt", Encoding.UTF8) .ToList();

            // sort & filter words.
            // no longer needed as results have been written to file, keeping in case dictionary changes
            // _words = _words.
            //     OrderBy(w => w.Length).
            //     ThenBy(w => w)
            //     .ToList();
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

        /// <summary>
        /// Finds all words containing letters defined in state.
        /// </summary>
        /// <param name="returnOnFirstMatch">Will stop looking and return results after first matching word length</param>
        /// <returns>List of matching words or null if none found</returns>
        private IEnumerable<string> GetWordsFromSelectedLetters(bool returnOnFirstMatch = false)
        {
            // store indexes for all word lengths (2-9)
            var indexes = new Dictionary<int, int>()
            {
                // we know that our dictionary is sorted by word length
                // and only includes words of minimum and maximum allowed lengths,
                // therefore first index is always 0
                {MinLettersInWord, 0}
            };
            // populate indexes between 3-9 
            for (var i = MinLettersInWord + 1; i <= MaxLettersTotal; i++)
            {
                indexes.TryGetValue(i - 1, out var lastIdx);
                var idx = _words.FindIndex(lastIdx, w => w.Length == i);
                indexes.Add(i, idx);
            }

            // add "end" index for (MaxLettersTotal + 1)
            indexes.Add(MaxLettersTotal + 1, _words.Count - 1);

            // find words that contain all letters
            var letters = State.CurrentLetters.ToLower().ToCharArray();
            var matches = new List<string>();
            for (var i = MaxLettersTotal; i >= MinLettersInWord; i--)
            {
                // define range for words of length 'i'
                indexes.TryGetValue(i, out var start);
                indexes.TryGetValue(i + 1, out var end);
                // get words from range
                matches.AddRange(_words
                    .GetRange(start, end - start)
                    .Where(w => w.All(letters.Contains))
                );

                if (returnOnFirstMatch && matches.Count > 0)
                {
                    break;
                }
            }

            return matches;
        }

        /// <summary>
        /// Scan word list for longest word matching letters contained in state.
        /// </summary>
        /// <returns>First word in list or null if none found for current letter pattern</returns>
        public string GetLongestWordOrNull()
        {
            return GetWordsFromSelectedLetters(true)
                .FirstOrDefault();
        }

        private bool IsValidWord(string guess)
        {
            return _words.Contains(guess);
        }

        /// <summary>
        /// Checks if user submitted guess is valid and updates score if necessary.
        /// </summary>
        /// <param name="guess"></param>
        /// <returns>Guess correct or not</returns>
        public bool IsValidGuessAndUpdateScore(string guess)
        {
            var letters = State.CurrentLetters.ToLower().ToCharArray();
            if (!guess.All(letters.Contains))
            {
                return false;
            }

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

        /// <summary>
        /// Resets game to initial state.
        /// </summary>
        /// <returns></returns>
        public State ResetGame()
        {
            return State.Reset();
        }
    }
}