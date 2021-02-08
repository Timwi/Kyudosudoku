using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Inclusion")]
    sealed class Inclusion : KyuFourCellConstraint, IEquatable<Inclusion>
    {
        public override string Description => $"The four cells around the circle must contain the {(Digits.Length == 1 ? $"digit {Digits[0]}" : $"digits {(Digits.JoinString(", ", lastSeparator: " and "))} in some order")}.";
        public static readonly Example Example = new Example
        {
            Constraints = { new Inclusion(2, new[] { 3, 3, 7 }) },
            Cells = { 2, 3, 12, 11 },
            Good = { 3, 5, 3, 7 },
            Bad = { 3, 5, 8, 7 },
            Reason = "Since the 3 is specified twice, it must occur at least twice."
        };

        public int[] Digits { get; private set; }

        public Inclusion(int topLeftCell, int[] digits) : base(topLeftCell)
        {
            if (digits == null || digits.Length == 0 || digits.Length > 4)
                throw new ArgumentException("Must have between 1 and 3 digits for Inclusion constraint.", nameof(digits));
            Digits = digits.Order().ToArray();
        }
        private Inclusion() { }     // for Classify

        public override string Svg => Digits.Length == 4
            ? $"<circle cx='{x}' cy='{y}' r='.2' stroke='black' stroke-width='.02' fill='white' /><text x='{x}' y='{y - .02}' text-anchor='middle' font-size='.2'>{Digits.Take(2).JoinString()}</text><text x='{x}' y='{y + .16}' text-anchor='middle' font-size='.2'>{Digits.Skip(2).JoinString()}</text>"
            : $"<circle cx='{x}' cy='{y}' r='.2' stroke='black' stroke-width='.02' fill='white' /><text x='{x}' y='{y + .08}' text-anchor='middle' font-size='.2'>{Digits.JoinString()}</text>";

        protected override bool verify(int a, int b, int c, int d)
        {
            var values = new[] { a, b, c, d };
            return Digits.All(d => values.Count(v => v == d) >= Digits.Count(d2 => d2 == d));
        }

        public static IList<KyuConstraint> Generate(int[] sudoku) => generate(sudoku, (cell, a, b, c, d) => Ut.NewArray(
            new Inclusion(cell, new[] { a }),
            new Inclusion(cell, new[] { b }),
            new Inclusion(cell, new[] { c }),
            new Inclusion(cell, new[] { d }),
            new Inclusion(cell, new[] { a, b }),
            new Inclusion(cell, new[] { a, c }),
            new Inclusion(cell, new[] { a, d }),
            new Inclusion(cell, new[] { b, c }),
            new Inclusion(cell, new[] { b, d }),
            new Inclusion(cell, new[] { c, d }),
            new Inclusion(cell, new[] { a, b, c }),
            new Inclusion(cell, new[] { a, b, d }),
            new Inclusion(cell, new[] { a, c, d }),
            new Inclusion(cell, new[] { b, c, d }),
            new Inclusion(cell, new[] { a, b, c, d })
        ).Distinct());

        public bool Equals(Inclusion other) => other.TopLeftCell == TopLeftCell && other.Digits.SequenceEqual(Digits);
        public override int GetHashCode() => unchecked(Ut.ArrayHash(Digits) * 37 + TopLeftCell);
    }
}
