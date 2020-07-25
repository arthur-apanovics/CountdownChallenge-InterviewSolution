using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameCore.Enums;

namespace GameCore.Models
{
    public class State
    {
        // game rules
        private readonly byte _maxLettersPerType;
        private readonly byte _maxLettersTotal;
        private readonly byte _maxRounds;
        
        /// <summary>
        /// Counts the total number of used letters.
        /// </summary>
        public int UsedTotal => UsedVowels.Count + UsedConsonants.Count;
        /// <summary>
        /// Keeps track of used vowel letters.
        /// </summary>
        public IList<char> UsedVowels {get; private set;}
        /// <summary>
        /// Keeps track of used consonant letters.
        /// </summary>
        public IList<char> UsedConsonants {get; private set;}
        /// <summary>
        /// Lazy way to keep track of the order in which letters were used.
        /// There are better ways to this but there's also little time :)
        /// </summary>
        public string CurrentLetters { get; private set; }
        /// <summary>
        /// Current game round.
        /// </summary>
        public byte Round {get; private set;}
        /// <summary>
        /// Are more rounds allowed
        /// </summary>
        public bool IsRoundLimitReached { get; private set; }
        /// <summary>
        /// Current game score.
        /// </summary>
        public uint Score {get; private set;}

        public State(byte maxLettersPerType, byte maxLettersTotal, byte maxRounds)
        {
            // keep track of rules
            _maxLettersPerType = maxLettersPerType;
            _maxLettersTotal = maxLettersTotal;
            _maxRounds = maxRounds;
            
            // init stuff
            UsedVowels = new List<char>(maxLettersPerType);
            UsedConsonants = new List<char>(maxLettersPerType);
            
            // initialise other variables
            Reset();
        }
        
        public bool IsTotalLetterLimitReached()
        {
            return UsedTotal >= _maxLettersTotal;
        }

        public bool IsLetterLimitReached(LetterType type)
        {
            return type switch
            {
                LetterType.Vowel => UsedVowels.Count >= _maxLettersPerType,
                LetterType.Consonant => UsedConsonants.Count >= _maxLettersPerType,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        internal State Reset()
        {
            UsedVowels.Clear();
            UsedConsonants.Clear();
            CurrentLetters = string.Empty;
            Score = 0;
            Round = 1;

            return this;
        }
        
        public State NewRound()
        {
            UsedVowels.Clear();
            UsedConsonants.Clear();
            CurrentLetters = string.Empty;

            if (Round == _maxRounds)
            {
                IsRoundLimitReached = true;
            }
            else
            {
                Round += 1;
            }

            return this;
        }

        public State AddUsedLetter(LetterType type, in char letter)
        {
            switch (type)
            {
                case LetterType.Vowel:
                    UsedVowels.Add(letter);
                    break;
                case LetterType.Consonant:
                    UsedConsonants.Add(letter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            CurrentLetters += letter;

            return this;
        }

        public State AddPointsToScore(int points)
        {
            Score += (uint) points;
            return this;
        }
    }
}