using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    sealed class Battlefield : KyuRowColConstraint
    {
        public override string Name => "Battlefield";
        public override string Description => $"The first and last number in the region represent the sizes of two armies, who march inward; the clue ({Clue}) specifies the sum of the digits that are either sandwiched between the armies or within the armies’ overlap.";
        public override double ExtraTop => IsCol ? .5 : 0;
        public override bool ShownTopLeft => true;

        public Battlefield(bool isCol, int rowCol, int clue) : base(isCol, rowCol)
        {
            Clue = clue;
        }
        private Battlefield() { }    // for Classify

        public int Clue { get; private set; }

        protected override Constraint getConstraint() => new BattlefieldUniquenessConstraint(Clue, GetAffectedCells(false));

        public override bool Verify(int[] grid) => BattlefieldUniquenessConstraint.CalculateBattlefieldClue(GetAffectedCells(false).Select(cell => grid[cell]).ToArray()) == Clue;

        public override string Svg => $@"<g transform='translate({Kyudosudoku.SudokuX + (IsCol ? RowCol : -1)}, {Kyudosudoku.SudokuY + (IsCol ? -.85 : RowCol)}) scale(.001)'>
            <linearGradient id='battlefield-e'>
              <stop offset='0' stop-color='#fff'/>
              <stop offset='1' stop-color='#fff' stop-opacity='0'/>
            </linearGradient>
            <linearGradient id='battlefield-d'>
              <stop offset='0' stop-color='#fff'/>
              <stop offset='.778' stop-color='#cbcbcb'/>
              <stop offset='1' stop-color='#8c8c8c'/>
            </linearGradient>
            <linearGradient id='battlefield-a'>
              <stop offset='0' stop-color='maroon'/>
              <stop offset='.5' stop-color='#ff0c0c'/>
              <stop offset='1' stop-color='maroon'/>
            </linearGradient>
            <linearGradient id='battlefield-b'>
              <stop offset='0' stop-color='#898989'/>
              <stop offset='.5' stop-color='#e6e6e6'/>
              <stop offset='1' stop-color='#a4a4a4'/>
            </linearGradient>
            <linearGradient id='battlefield-q' x1='172.584' x2='508.164' y1='523.652' y2='527.761' gradientTransform='translate(0 -109.238)' gradientUnits='userSpaceOnUse' xlink:href='#battlefield-a'/>
            <linearGradient id='battlefield-u' x1='156.148' x2='276.683' y1='615.423' y2='683.909' gradientTransform='matrix(1.00332 0 0 1 -4.184 -113.875)' gradientUnits='userSpaceOnUse' spreadMethod='reflect' xlink:href='#battlefield-b'/>
            <linearGradient id='battlefield-s' x1='45.442' x2='330.588' y1='449.999' y2='533.551' gradientTransform='matrix(1 0 0 -1 -2.318 1035.857)' gradientUnits='userSpaceOnUse' spreadMethod='reflect' xlink:href='#battlefield-b'/>
            <linearGradient id='battlefield-t' x1='166.405' x2='300.026' y1='707.505' y2='740.378' gradientTransform='matrix(1 0 0 -1 -2.318 1035.857)' gradientUnits='userSpaceOnUse' spreadMethod='reflect' xlink:href='#battlefield-b'/>
            <linearGradient id='battlefield-c'>
              <stop offset='0' stop-color='#696969'/>
              <stop offset='.5' stop-color='#fff'/>
              <stop offset='1' stop-color='#939393'/>
            </linearGradient>
            <linearGradient id='battlefield-w' x1='719.457' x2='803.146' y1='591.232' y2='638.464' gradientTransform='matrix(1.00332 0 0 1 -455.509 -74.515)' gradientUnits='userSpaceOnUse' spreadMethod='reflect' xlink:href='#battlefield-b'/>
            <linearGradient id='battlefield-i'>
              <stop offset='0' stop-color='#434343'/>
              <stop offset='.5' stop-color='#fff'/>
              <stop offset='1' stop-color='#5a5a5a'/>
            </linearGradient>
            <linearGradient id='battlefield-n'>
              <stop offset='0' stop-color='#333'/>
              <stop offset='1' stop-color='#cecece'/>
            </linearGradient>
            <linearGradient id='battlefield-f'>
              <stop offset='0' stop-color='#fff'/>
              <stop offset='.689' stop-color='#bfbfbf'/>
              <stop offset='1' stop-color='#525252'/>
            </linearGradient>
            <linearGradient id='battlefield-g'>
              <stop offset='0' stop-color='#502d16'/>
              <stop offset='1' stop-color='#2a170b'/>
            </linearGradient>
            <linearGradient id='battlefield-h'>
              <stop offset='0' stop-color='#fff'/>
              <stop offset='1' stop-color='#fff' stop-opacity='0'/>
            </linearGradient>
            <linearGradient id='battlefield-p'>
              <stop offset='0' stop-color='#fff'/>
              <stop offset='1' stop-color='#4a4a4a'/>
            </linearGradient>
            <linearGradient id='battlefield-k'>
              <stop offset='0' stop-color='#6b6b6b'/>
              <stop offset='.5' stop-color='#eee'/>
              <stop offset='1' stop-color='#6d6d6d' stop-opacity='.984'/>
            </linearGradient>
            <linearGradient id='battlefield-o'>
              <stop offset='0' stop-color='#fff'/>
              <stop offset='.617' stop-color='#979797' stop-opacity='.498'/>
              <stop offset='1' stop-color='#303030' stop-opacity='0'/>
            </linearGradient>
            <linearGradient id='battlefield-j'>
              <stop offset='0' stop-color='#6b6b6b'/>
              <stop offset='.5' stop-color='#b1b1b1'/>
              <stop offset='1' stop-color='#6d6d6d' stop-opacity='.984'/>
            </linearGradient>
            <linearGradient id='battlefield-l'>
              <stop offset='0' stop-color='#1a1a1a'/>
              <stop offset='1' stop-color='#1a1a1a' stop-opacity='0'/>
            </linearGradient>
            <linearGradient id='battlefield-m'>
              <stop offset='0' stop-color='#686868'/>
              <stop offset='1' stop-color='#686868' stop-opacity='0'/>
            </linearGradient>
            <radialGradient id='battlefield-v' cx='-184.381' cy='586.537' r='73.397' fx='-184.381' fy='586.537' gradientTransform='matrix(.9285 0 0 .91956 516.958 -12.554)' gradientUnits='userSpaceOnUse' xlink:href='#battlefield-d'/>
            <radialGradient id='battlefield-x' cx='426.2' cy='518.42' r='29.133' fx='426.2' fy='518.42' gradientTransform='matrix(.9872 .68634 -.89224 1.28335 447.713 -373.275)' gradientUnits='userSpaceOnUse' xlink:href='#battlefield-e'/>
            <filter id='battlefield-r'>
              <feGaussianBlur stdDeviation='2.77'/>
            </filter>
            <filter id='battlefield-y'>
              <feGaussianBlur stdDeviation='3.843'/>
            </filter>
            <path fill='url(#battlefield-q)' fill-rule='evenodd' d='M149.438 305.168v217.375a20.5 20.5 0 012.28-.125h184.25v-217.25zm227.375 0v217.25h185.374v-217.25zm-227.375 261v83.719c0 54.151 49.214 113.965 105.187 158.531 27.987 22.284 56.622 40.622 78.281 53.188 1.078.625 2.037 1.165 3.063 1.75V566.293h-184.25a20.5 20.5 0 01-2.281-.125zm227.375.125v296.688c.648-.372 1.24-.71 1.906-1.094 21.617-12.505 50.228-30.832 78.218-53.156 55.981-44.65 105.25-104.699 105.25-158.844v-83.594z' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='#500' fill-rule='evenodd' d='M151.222 303.968v213.904c.74-.084 1.496-.126 2.259-.126h3.187V309.45h177.229v-5.482zm225.112 0V521.75h5.445v-212.3h178.084v-5.483zm-225.112 261.64V649.534c0 42.673 30.122 88.84 69.922 128.346-37.082-38.256-64.477-82.15-64.477-122.864v-83.924c.74.084 1.496.125 2.259.125h176.97v-5.482H153.482c-.763 0-1.518-.041-2.259-.125zm225.112.126V863.15c.641-.372 1.228-.71 1.887-1.096 1.14-.668 2.379-1.43 3.558-2.13V571.215h178.084v-5.482z' filter='url(#battlefield-r)' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='url(#battlefield-s)' fill-rule='evenodd' stroke='url(#battlefield-t)' stroke-width='4.623' d='M115.317 266.404c-1.297 0-2.344 1.15-2.344 2.563v376.281c0 145.099 234.728 261.528 240.5 261.906 5.76.378 240.531-116.807 240.531-261.906V268.967c0-1.413-1.078-2.563-2.375-2.563zm31.812 34.125H333.66v217.25H149.41a20.5 20.5 0 00-2.28.125zm227.375 0H559.88v217.25H374.504zm-227.375 261c.748.084 1.51.125 2.281.125h184.25v297.063c-1.025-.586-1.985-1.125-3.062-1.75-21.66-12.566-50.295-30.905-78.281-53.188-55.973-44.567-105.188-104.38-105.188-158.531v-49.406zm227.375.125H559.88v83.594c0 54.145-49.269 114.194-105.25 158.844-27.99 22.324-56.6 40.65-78.219 53.156-.665.385-1.258.722-1.906 1.094z' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='url(#battlefield-u)' fill-rule='evenodd' d='M145.743 298.49v220.442c.75-.085 1.516-.126 2.29-.126h3.229V303.985h183.632v-5.496zm228.13 0v218.316h5.518V303.985h180.472v-5.496zm-228.13 262.28v84.131c0 42.777 30.526 89.057 70.86 128.66-37.58-38.35-65.341-82.352-65.341-123.165V566.267c.75.084 1.515.125 2.288.125h179.344v-5.496H148.032c-.773 0-1.538-.041-2.289-.125zm228.13.126v298.145c.65-.373 1.245-.712 1.913-1.1 1.156-.669 2.41-1.432 3.605-2.135V566.392h180.472v-5.496z' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='#333' fill-rule='evenodd' d='M409.42 492.733c11.096 12.816 17.849 29.511 17.906 47.781v.219c0 40.515-32.892 73.406-73.407 73.406-18.37 0-35.122-6.802-48-17.969 13.461 15.525 33.293 25.375 55.438 25.375 40.515 0 73.406-32.89 73.406-73.406v-.219c-.069-22.045-9.898-41.777-25.344-55.187z' filter='url(#battlefield-r)' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='#b3b3b3' fill-rule='evenodd' d='M432.277 483.202a73.397 73.397 0 01-73.339 73.397 73.397 73.397 0 01-73.454-73.282 73.397 73.397 0 0173.225-73.512 73.397 73.397 0 0173.568 73.168' overflow='visible' style='marker:none'/>
            <path fill='url(#battlefield-v)' fill-rule='evenodd' d='M427.03 542.546a68.149 67.493 0 01-68.096 67.493 68.149 67.493 0 01-68.202-67.387 68.149 67.493 0 0167.99-67.598 68.149 67.493 0 0168.307 67.282' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='#333' fill-rule='evenodd' d='M360.205 411.109c-40.515 0-73.407 32.891-73.407 73.406 0 40.515 32.892 73.406 73.407 73.406 40.515 0 73.375-32.89 73.375-73.406v-.219c-.127-40.434-32.94-73.187-73.375-73.187zm0 5.25c37.543 0 68.007 30.1 68.125 67.281v.219c0 37.256-30.507 67.5-68.125 67.5-37.618 0-68.157-30.244-68.157-67.5s30.539-67.5 68.157-67.5z' overflow='visible' style='marker:none'/>
            <path fill='url(#battlefield-w)' fill-rule='evenodd' d='M357.58 468.485c-40.514 0-73.406 32.891-73.406 73.406 0 40.515 32.892 73.406 73.407 73.406 40.515 0 73.375-32.89 73.375-73.406v-.219c-.127-40.434-32.94-73.187-73.375-73.187zm0 5.25c37.544 0 68.008 30.1 68.126 67.281v.219c0 37.256-30.507 67.5-68.125 67.5-37.618 0-68.157-30.244-68.157-67.5s30.539-67.5 68.157-67.5z' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='url(#battlefield-x)' fill-rule='evenodd' d='M413.43 547.311c12.331-1.66 13.447 11.78 2.491 28.404-10.955 16.624-25.964 26.276-38.295 27.936-12.33 1.66-17.337-6.593-6.381-23.217 10.934-16.59 29.775-31.405 42.116-33.113' overflow='visible' style='marker:none' transform='translate(0 -60)'/>
            <path fill='#fff' d='M142.369 165.392l-11.687-2.31 10.472 68.418-19.23-66.593-9.793 6.785 2.31-11.687-69.048 11.102 67.223-19.86-6.784-9.793 11.687 2.31-9.842-65.898 18.6 64.073 9.792-6.784-2.31 11.687 67.788-6.693-65.963 15.45z' filter='url(#battlefield-y)' overflow='visible' style='marker:none' transform='matrix(.25394 0 0 .24933 182.231 168.163)'/>
            <path fill='#fff' d='M142.369 165.392l-11.687-2.31 10.472 68.418-19.23-66.593-9.793 6.785 2.31-11.687-69.048 11.102 67.223-19.86-6.784-9.793 11.687 2.31-9.842-65.898 18.6 64.073 9.792-6.784-2.31 11.687 67.788-6.693-65.963 15.45z' filter='url(#battlefield-y)' overflow='visible' style='marker:none' transform='matrix(.25394 0 0 .24933 449.415 684.506)'/>
            <path fill='#fff' d='M142.369 165.392l-11.687-2.31 10.472 68.418-19.23-66.593-9.793 6.785 2.31-11.687-69.048 11.102 67.223-19.86-6.784-9.793 11.687 2.31-9.842-65.898 18.6 64.073 9.792-6.784-2.31 11.687 67.788-6.693-65.963 15.45z' filter='url(#battlefield-y)' overflow='visible' style='marker:none' transform='matrix(.25394 0 0 .24933 312.963 374.008)'/>
            <text x='775' y='575' font-size='256'>{Clue}</text>
        </g>";

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var constraints = new List<KyuConstraint>();
            foreach (var isCol in new[] { false, true })
                for (var rowCol = 0; rowCol < 9; rowCol++)
                    constraints.Add(new Battlefield(isCol, rowCol, BattlefieldUniquenessConstraint.CalculateBattlefieldClue(Ut.NewArray(9, x => sudoku[isCol ? (rowCol + 9 * x) : (x + 9 * rowCol)]))));
            return constraints;
        }
    }
}