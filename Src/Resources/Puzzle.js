window.onload = (function()
{
    function remoteLog(msg)
    {
        //let req = new XMLHttpRequest();
        //req.open('POST', '/remote-log', true);
        //req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        //req.send(`msg=${encodeURIComponent(msg)}`);
    }

    function inRange(x) { return x >= 0 && x < 9; }
    function dx(dir) { return dir === 'Left' ? -1 : dir === 'Right' ? 1 : 0 }
    function dy(dir) { return dir === 'Up' ? -1 : dir === 'Down' ? 1 : 0 }
    function Adjacent(cell)
    {
        let list = [];
        let x = cell % 9;
        let y = (cell / 9) | 0;
        for (let xx = x - 1; xx <= x + 1; xx++)
            if (inRange(xx))
                for (let yy = y - 1; yy <= y + 1; yy++)
                    if (inRange(yy) && (xx != x || yy != y))
                        list.push(xx + 9 * yy);
        return list;
    }

    function Orthogonal(cell)
    {
        let list = [];
        let x = cell % 9;
        let y = (cell / 9) | 0;
        for (let xx = x - 1; xx <= x + 1; xx++)
            if (inRange(xx))
                for (let yy = y - 1; yy <= y + 1; yy++)
                    if (inRange(yy) && (xx == x || yy == y) && (xx != x || yy != y))
                        list.push(xx + 9 * yy);
        return list;
    }

    function validateConstraint(grid, constr)
    {
        switch (constr[':type'])
        {
            // CELL CONSTRAINTS

            case 'OddEven':
                return grid[constr.Cell] === null ? null : grid[constr.Cell] % 2 === (constr.Odd ? 1 : 0);

            case 'AntiBishop': {
                if (grid[constr.Cell] === null)
                    return null;
                let diagonals = Array(81).fill(null).map((_, c) => c).filter(c => c != constr.Cell && Math.abs(c % 9 - constr.Cell % 9) === Math.abs(((c / 9) | 0) - ((constr.Cell / 9) | 0)));
                return diagonals.some(c => grid[c] !== null && grid[c] === grid[constr.Cell]) ? false :
                    diagonals.some(c => grid[c] === null) ? null : true;
            }

            case 'AntiKnight': {
                let x = constr.Cell % 9;
                let y = (constr.Cell / 9) | 0;
                let knightsMoves = [];
                for (let dx of [-2, -1, 1, 2])
                    if (inRange(x + dx))
                        for (let dy of (dx === 1 || dx === -1) ? [-2, 2] : [-1, 1])
                            if (inRange(y + dy))
                                knightsMoves.push(x + dx + 9 * (y + dy));
                return knightsMoves.some(c => grid[c] !== null && grid[c] === grid[constr.Cell]) ? false :
                    knightsMoves.some(c => grid[c] === null) ? null : true;
            }

            case 'AntiKing':
                return Adjacent(constr.Cell).some(c => grid[c] !== null && grid[c] === grid[constr.Cell]) ? false :
                    Adjacent(constr.Cell).some(c => grid[c] === null) ? null : true;

            case 'NoConsecutive':
                return grid[constr.Cell] !== null && Orthogonal(constr.Cell).some(c => grid[c] !== null && Math.abs(grid[c] - grid[constr.Cell]) === 1) ? false :
                    grid[constr.Cell] === null || Orthogonal(constr.Cell).some(c => grid[c] === null) ? null : true;

            case 'MaximumCell':
                return grid[constr.Cell] !== null && Orthogonal(constr.Cell).some(c => grid[c] !== null && grid[c] >= grid[constr.Cell]) ? false :
                    grid[constr.Cell] === null || Orthogonal(constr.Cell).some(c => grid[c] === null) ? null : true;

            case 'FindThe9':
                return grid[constr.Cell] === null || grid[constr.Cell + (dx(constr.Direction) + 9 * dy(constr.Direction)) * grid[constr.Cell]] === null ? null :
                    grid[constr.Cell + (dx(constr.Direction) + 9 * dy(constr.Direction)) * grid[constr.Cell]] === 9;


            // ROW/COLUMN CONSTRAINTS

            case 'Sandwich': {
                let numbers = Array(9).fill(null).map((_, x) => grid[constr.IsCol ? (constr.RowCol + 9 * x) : (x + 9 * constr.RowCol)]);
                let p1 = numbers.indexOf(constr.Digit1);
                let p2 = numbers.indexOf(constr.Digit2);
                if (p1 === -1 || p2 === -1)
                    return numbers.some(n => n === null) ? null : false;
                let sandwich = numbers.slice(Math.min(p1, p2) + 1, Math.max(p1, p2));
                return sandwich.some(n => n === null) ? null : sandwich.reduce((p, n) => p + n, 0) === constr.Sum;
            }

            case 'ToroidalSandwich': {
                let numbers = Array(9).fill(null).map((_, x) => grid[constr.IsCol ? (constr.RowCol + 9 * x) : (x + 9 * constr.RowCol)]);
                let p1 = numbers.indexOf(constr.Digit1);
                let p2 = numbers.indexOf(constr.Digit2);
                if (p1 === -1 || p2 === -1)
                    return numbers.some(n => n === null) ? null : false;
                let s = 0;
                let i = (p1 + 1) % numbers.length;
                while (i !== p2)
                {
                    if (numbers[i] === null)
                        return null;
                    s += numbers[i];
                    i = (i + 1) % numbers.length;
                }
                return s === constr.Sum;
            }

            case 'Skyscraper': {
                let numbers = Array(9).fill(null).map((_, x) => grid[constr.IsCol ? (constr.RowCol + 9 * (constr.Reverse ? 8 - x : x)) : ((constr.Reverse ? 8 - x : x) + 9 * constr.RowCol)]);
                if (numbers.some(n => n === null))
                    return null;
                let c = 0, p = 0;
                for (let n of numbers)
                    if (n > p)
                    {
                        p = n;
                        c++;
                    }
                return c === constr.Clue;
            }

            case 'XSum': {
                let numbers = Array(9).fill(null).map((_, x) => grid[constr.IsCol ? (constr.RowCol + 9 * (constr.Reverse ? 8 - x : x)) : ((constr.Reverse ? 8 - x : x) + 9 * constr.RowCol)]);
                if (numbers[0] === null || numbers.slice(0, numbers[0]).some(n => n === null))
                    return null;
                return constr.Clue === numbers.slice(0, numbers[0]).reduce((p, n) => p + n, 0);
            }

            case 'Battlefield': {
                let numbers = Array(9).fill(null).map((_, x) => grid[constr.IsCol ? (constr.RowCol + 9 * x) : (x + 9 * constr.RowCol)]);
                if (numbers[0] === null || numbers[8] === null)
                    return null;
                let left = numbers[0];
                let right = numbers[numbers.length - 1];
                let sum = 0;
                if (numbers.length - left - right >= 0)
                    for (let ix = left; ix < numbers.length - right; ix++)
                    {
                        if (numbers[ix] === null)
                            return null;
                        sum += numbers[ix];
                    }
                else
                    for (let ix = numbers.length - right; ix < left; ix++)
                    {
                        if (numbers[ix] === null)
                            return null;
                        sum += numbers[ix];
                    }
                return sum === constr.Clue;
            }

            case 'Binairo': {
                let numbers = Array(9).fill(null).map((_, x) => grid[constr.IsCol ? (constr.RowCol + 9 * x) : (x + 9 * constr.RowCol)]);
                for (let i = 1; i < numbers.length - 1; i++)
                    if (numbers[i - 1] !== null && numbers[i] !== null && numbers[i + 1] !== null && numbers[i - 1] % 2 === numbers[i] % 2 && numbers[i + 1] % 2 === numbers[i] % 2)
                        return false;
                return numbers.some(n => n === null) ? null : true;
            }

            // REGION CONSTRAINTS

            case 'Thermometer': {
                for (let i = 0; i < constr.Cells.length; i++)
                    for (let j = i + 1; j < constr.Cells.length; j++)
                        if (grid[constr.Cells[i]] !== null && grid[constr.Cells[j]] !== null && grid[constr.Cells[i]] >= grid[constr.Cells[j]])
                            return false;
                return constr.Cells.some(c => grid[c] === null) ? null : true;
            }

            case 'Arrow':
                return constr.Cells.some(c => grid[c] === null) ? null : grid[constr.Cells[0]] === constr.Cells.slice(1).reduce((sum, cell) => sum + grid[cell], 0);

            case 'Palindrome':
                for (let i = 0; i < (constr.Cells.length / 2) | 0; i++)
                    if (grid[constr.Cells[i]] !== null && grid[constr.Cells[constr.Cells.length - 1 - i]] !== null && grid[constr.Cells[i]] !== grid[constr.Cells[constr.Cells.length - 1 - i]])
                        return false;
                return constr.Cells.some(c => grid[c] === null) ? null : true;

            case 'KillerCage': {
                for (let i = 0; i < constr.Cells.length; i++)
                    for (let j = i + 1; j < constr.Cells.length; j++)
                        if (grid[constr.Cells[i]] !== null && grid[constr.Cells[j]] !== null && grid[constr.Cells[i]] === grid[constr.Cells[j]])
                            return false;
                return constr.Cells.some(c => grid[c] === null) ? null : (constr.Sum === null || constr.Cells.reduce((p, n) => p + grid[n], 0) === constr.Sum);
            }

            case 'RenbanCage': {
                let numbers = constr.Cells.map(c => grid[c]);
                return numbers.some(n => n === null) ? null : numbers.filter(n => !numbers.includes(n + 1)).length === 1;
            }

            case 'Snowball': {
                let offsets = [...new Set(Array(constr.Cells1.length).fill(null).map((_, ix) => grid[constr.Cells1[ix]] === null || grid[constr.Cells2[ix]] === null ? null : grid[constr.Cells2[ix]] - grid[constr.Cells1[ix]]).filter(c => c !== null))];
                return offsets.length > 1 ? false : constr.Cells1.some(n => grid[n] === null) || constr.Cells2.some(n => grid[n] === null) ? null : true;
            }

            // FOUR-CELL CONSTRAINTS

            case 'Clockface': {
                let numbers = [0, 1, 10, 9].map(o => grid[constr.TopLeftCell + o]);
                if (numbers.some(n => n === null))
                    return null;
                let a = numbers[0], b = numbers[1], c = numbers[2], d = numbers[3];
                return constr.Clockwise
                    ? (a < b && b < c && c < d) || (b < c && c < d && d < a) || (c < d && d < a && a < b) || (d < a && a < b && b < c)
                    : (a > b && b > c && c > d) || (b > c && c > d && d > a) || (c > d && d > a && a > b) || (d > a && a > b && b > c);
            }

            case 'Inclusion': {
                let numbers = [0, 1, 10, 9].map(o => grid[constr.TopLeftCell + o]);
                if (numbers.some(n => n === null))
                    return null;
                return constr.Digits.every(d => numbers.filter(n => n === d).length >= constr.Digits.filter(d2 => d2 === d).length);
            }

            case 'Battenburg': {
                let offsets = [0, 1, 10, 9].map(c => constr.TopLeftCell + c);
                for (let i = 0; i < 4; i++)
                    if (grid[offsets[i]] !== null && grid[offsets[(i + 1) % offsets.length]] !== null && grid[offsets[i]] % 2 === grid[offsets[(i + 1) % offsets.length]] % 2)
                        return false;
                return offsets.some(c => grid[c] === null) ? null : true;
            }

            // OTHER CONSTRAINTS

            case 'ConsecutiveNeighbors':
                return grid[constr.Cell1] === null || grid[constr.Cell2] === null ? null : Math.abs(grid[constr.Cell1] - grid[constr.Cell2]) === 1;

            case 'DoubleNeighbors':
                return grid[constr.Cell1] === null || grid[constr.Cell2] === null ? null : grid[constr.Cell1] * 2 === grid[constr.Cell2] || grid[constr.Cell2] * 2 === grid[constr.Cell1];

            case 'LittleKiller': {
                let affectedCells = [];
                switch (constr.Direction)
                {
                    case 'SouthEast': affectedCells = Array(9 - constr.Offset).fill(null).map((_, i) => constr.Offset + 10 * i); break;
                    case 'SouthWest': affectedCells = Array(9 - constr.Offset).fill(null).map((_, i) => 8 + 9 * constr.Offset + 8 * i); break;
                    case 'NorthWest': affectedCells = Array(9 - constr.Offset).fill(null).map((_, i) => 80 - constr.Offset - 10 * i); break;
                    case 'NorthEast': affectedCells = Array(9 - constr.Offset).fill(null).map((_, i) => 72 - 9 * constr.Offset - 8 * i); break;
                };
                return affectedCells.some(c => grid[c] === null) ? null : affectedCells.reduce((p, n) => p + grid[n], 0) === constr.Sum;
            }
        }
    }

    function handler(fnc)
    {
        return function(ev)
        {
            fnc(ev);
            ev.stopPropagation();
            ev.preventDefault();
            return false;
        };
    }

    function setClass(elem, className, setUnset)
    {
        if (setUnset)
            elem.classList.add(className);
        else
            elem.classList.remove(className);
    }

    let first = true;
    let draggingMode = null;
    document.body.onmouseup = handler(document.body.ontouchend = function(ev)
    {
        if (ev.type !== 'touchend' || ev.touches.length === 0)
            draggingMode = null;
        remoteLog(`${ev.type} document.body null`);
    });

    Array.from(document.getElementsByClassName('puzzle')).forEach(puzzleDiv =>
    {
        let puzzleId = puzzleDiv.dataset.puzzleid | 0;
        let kyudokusRaw = puzzleDiv.dataset.kyudokus;
        let kyudokuGrids = [0, 1, 2, 3].map(corner => Array(36).fill(null).map((_, cell) => parseInt(kyudokusRaw.substr(36 * corner + cell, 1))));
        let constraints = JSON.parse(puzzleDiv.dataset.constraints || null) || [];

        if (first)
        {
            puzzleDiv.focus();
            first = false;
        }

        let state = {
            circledDigits: Array(4).fill(null).map(_ => Array(36).fill(null)),
            cornerNotation: Array(81).fill(null).map(_ => []),
            centerNotation: Array(81).fill(null).map(_ => []),
            enteredDigits: Array(81).fill(null)
        };
        let undoBuffer = [JSON.parse(JSON.stringify(state))];
        let redoBuffer = [];

        let mode = 'normal';
        let helpEnabled = localStorage.getItem('kyu-help') !== 'Off';
        let selectedCells = [];
        let highlightedDigit = null;
        let hoveredKyDigit = null;    // if not null, it’s an array of [corner, cell]
        let showErrors = puzzleDiv.dataset.showerrors === '1';
        let semitransparentXs = puzzleDiv.dataset.semitransparentxs === '1';
        let mobileLeft = true;

        function remoteLog2(msg)
        {
            remoteLog(`${msg} [${selectedCells.join()}] ${draggingMode ?? "null"}`);
        }

        function encodeState(st)
        {
            let val = 0n;

            // Encode the Sudoku grid
            for (let cell = 0; cell < 81; cell++)
            {
                // Skip cells that are fully defined by a circled Kyudoku digit
                if (getKyudokuCircledDigit(st, cell) !== null)
                    continue;

                // Compact representation of an entered digit or a completely empty cell
                if (st.enteredDigits[cell] !== null)
                    val = (val * 11n) + BigInt(st.enteredDigits[cell]);
                else if (st.cornerNotation[cell].length === 0 && st.centerNotation[cell].length === 0)
                    val = (val * 11n);
                else
                {
                    // corner notation
                    for (let digit = 1; digit <= 9; digit++)
                        val = (val * 2n) + (st.cornerNotation[cell].includes(digit) ? 1n : 0n);

                    // center notation
                    for (let digit = 1; digit <= 9; digit++)
                        val = (val * 2n) + (st.centerNotation[cell].includes(digit) ? 1n : 0n);

                    val = (val * 11n) + 10n;
                }
            }
            for (let corner = 0; corner < 4; corner++)
                for (let cell = 0; cell < 36; cell++)
                    val = (val * 3n) + (st.circledDigits[corner][cell] === true ? 2n : st.circledDigits[corner][cell] === false ? 1n : 0n);

            // Safe characters to use: 0x21 - 0xD7FF and 0xE000 - 0xFFFD
            // (0x20 will later be used as a separator)
            let maxValue = BigInt(0xfffd - 0xe000 + 1 + 0xd7ff - 0x21 + 1);
            function getChar(v) { return String.fromCharCode(v > 0xd7ff - 0x21 + 1 ? 0xe000 + (v - (0xd7ff - 0x21 + 1)) : 0x21 + v); }

            let str = '';
            while (val > 0n)
            {
                str += getChar(Number(val % maxValue));
                val = val / maxValue;
            }
            return str;
        }

        function decodeState(str)
        {
            // Safe characters to use: 0x21 - 0xD7FF and 0xE000 - 0xFFFD
            // (0x20 will later be used as a separator)
            let maxValue = BigInt(0xfffd - 0xe000 + 1 + 0xd7ff - 0x21 + 1);
            function charToVal(ch) { return ch >= 0xe000 ? ch - 0xe000 + 0xd7ff - 0x21 + 1 : ch - 0x21; }

            let val = 0n;
            for (let ix = str.length - 1; ix >= 0; ix--)
                val = (val * maxValue) + BigInt(charToVal(str.charCodeAt(ix)));

            let st = {
                circledDigits: Array(4).fill(null).map(_ => Array(36).fill(null)),
                cornerNotation: Array(81).fill(null).map(_ => []),
                centerNotation: Array(81).fill(null).map(_ => []),
                enteredDigits: Array(81).fill(null)
            };

            // Decode Kyudoku grid
            for (let corner = 4 - 1; corner >= 0; corner--)
                for (let cell = 36 - 1; cell >= 0; cell--)
                {
                    st.circledDigits[corner][cell] = (val % 3n === 0n ? null : val % 3n === 1n ? false : true);
                    val = val / 3n;
                }

            // Decode Sudoku grid
            for (let cell = 81 - 1; cell >= 0; cell--)
            {
                // Skip cells that are fully defined by a circled Kyudoku digit
                if (getKyudokuCircledDigit(st, cell) !== null)
                    continue;

                let code = val % 11n;
                val = val / 11n;
                // Complex case: center notation and corner notation
                if (code === 10n)
                {
                    // Center notation
                    for (let digit = 9; digit >= 1; digit--)
                    {
                        if (val % 2n === 1n)
                            st.centerNotation[cell].unshift(digit);
                        val = val / 2n;
                    }

                    // Corner notation
                    for (let digit = 9; digit >= 1; digit--)
                    {
                        if (val % 2n === 1n)
                            st.cornerNotation[cell].unshift(digit);
                        val = val / 2n;
                    }
                }
                else if (code > 0n)
                    st.enteredDigits[cell] = Number(code);
            }
            return st;
        }

        try
        {
            let undoB = localStorage.getItem(`ky${puzzleId}-undo`);
            let redoB = localStorage.getItem(`ky${puzzleId}-redo`);

            undoBuffer = undoB ? undoB.split(' ').map(decodeState) : [JSON.parse(JSON.stringify(state))];
            redoBuffer = redoB ? redoB.split(' ').map(decodeState) : [];

            let item = null;
            if (puzzleDiv.dataset.progress)
            {
                item = JSON.parse(puzzleDiv.dataset.progress);
                if (undoB && undoB.includes(encodeState(item)))
                    item = null;
            }

            if (item === null)
            {
                str = localStorage.getItem(`ky${puzzleId}`);
                if (str !== null)
                    try { item = JSON.parse(localStorage.getItem(`ky${puzzleId}`)); }
                    catch { item = decodeState(str); }
            }
            if (item && item.circledDigits && item.cornerNotation && item.centerNotation && item.enteredDigits)
                state = item;
        }
        catch
        {
        }

        let dbUpdater = null;
        let timeLastDbUpdate = new Date();
        let puzzleIsSolved = false;

        function dbUpdate(isSolved)
        {
            let req = new XMLHttpRequest();
            req.open('POST', `db-update/${puzzleId}`, true);
            req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            req.send(`progress=${encodeURIComponent(JSON.stringify(state))}&time=${Math.round((new Date() - timeLastDbUpdate) / 1000)}${isSolved ? '&getdata=1' : ''}`);
            let reqStart = new Date();
            req.onload = function()
            {
                var json = req.responseText ? JSON.parse(req.responseText) : null;
                if (json)
                {
                    puzzleDiv.querySelector('text.inf-time').textContent = json.time ? (json.time < 60 ? `${json.time} seconds` : json.time < 60 * 60 ? `${(json.time / 60) | 0} min ${json.time % 60} sec` : `${(json.time / 60 / 60) | 0} h ${((json.time / 60) | 0) % 60} min ${json.time % 60} sec`) : 'not recorded';
                    puzzleDiv.querySelector('text.inf-avg').textContent = json.avg ? (json.avg < 60 ? `${json.avg} seconds` : json.avg < 60 * 60 ? `${(json.avg / 60) | 0} min ${json.avg % 60} sec` : `${(json.avg / 60 / 60) | 0} h ${((json.avg / 60) | 0) % 60} min ${json.avg % 60} sec`) : 'unknown';
                    puzzleDiv.querySelector('text.inf-count').textContent = json.count === 1 ? "once" : `${json.count | 0} times`;
                }
                timeLastDbUpdate = reqStart;
            };

            if (isSolved)
            {
                clearInterval(dbUpdater);
                puzzleIsSolved = true;
            }
        }

        function timerChanger()
        {
            if (puzzleIsSolved)
                return;
            if (document.hidden && dbUpdater !== null)
            {
                dbUpdate(false);
                clearInterval(dbUpdater);
                dbUpdater = null;
                puzzleDiv.querySelector('.full-puzzle').style.filter = `url(#p-${puzzleId}-timer-paused)`;
                puzzleDiv.querySelector('.timer-paused').style.opacity = '1';
            }
            else if (!document.hidden && dbUpdater === null)
            {
                timeLastDbUpdate = new Date();
                dbUpdater = setInterval(dbUpdate, 10000);
                puzzleDiv.querySelector('.full-puzzle').style.filter = null;
                puzzleDiv.querySelector('.timer-paused').style.opacity = '0';
            }
        }
        document.addEventListener('visibilitychange', timerChanger);
        timerChanger();

        function resetRestartButton()
        {
            document.getElementById(`p-${puzzleId}-btn-restart`).classList.remove('warning');
            document.querySelector(`#p-${puzzleId}-btn-restart>text`).textContent = 'Restart';
            document.getElementById(`p-${puzzleId}-btn-restart-left`).classList.remove('warning');
            document.querySelector(`#p-${puzzleId}-btn-restart-left>text`).textContent = 'Restart';
        }

        /// Returns:
        /// • “false” if multiple equivalent Kyudoku cells are circled with *different* digits
        /// • “null” if no corresponding Kyudoku cell is circled
        /// • the circled digit otherwise
        function getKyudokuCircledDigit(st, cell)
        {
            let kyCells = [0, 1, 2, 3]
                .filter(c => cell % 9 >= 3 * (c % 2) && cell % 9 < 6 + 3 * (c % 2) && cell / 9 >= 3 * ((c / 2) | 0) && cell / 9 < 6 + 3 * ((c / 2) | 0))
                .map(c => ({ corner: c, kyCell: cell % 9 - 3 * (c % 2) + 6 * (((cell / 9) | 0) - 3 * ((c / 2) | 0)) }))
                .filter(inf => st.circledDigits[inf.corner][inf.kyCell] === true);
            if (kyCells.length > 1 && kyCells.some(inf => kyudokuGrids[inf.corner][inf.kyCell] !== kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]))
                return false;
            else if (kyCells.length >= 1)
                return parseInt(kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]);
            return null;
        }

        function getDisplayedSudokuDigit(st, cell)
        {
            let kyu = getKyudokuCircledDigit(st, cell);
            return (kyu === null && st.enteredDigits[cell] !== null) ? st.enteredDigits[cell] : kyu;
        }

        function isSudokuValid()
        {
            let grid = Array(81).fill(null).map((_, c) => getDisplayedSudokuDigit(state, c)).map(x => x === false ? null : x);

            // Check the Sudoku rules (rows, columns and regions)
            for (let i = 0; i < 9; i++)
            {
                for (let colA = 0; colA < 9; colA++)
                    for (let colB = colA + 1; colB < 9; colB++)
                        if (grid[colA + 9 * i] !== null && grid[colA + 9 * i] === grid[colB + 9 * i])
                            return false;
                for (let rowA = 0; rowA < 9; rowA++)
                    for (let rowB = rowA + 1; rowB < 9; rowB++)
                        if (grid[i + 9 * rowA] !== null && grid[i + 9 * rowA] === grid[i + 9 * rowB])
                            return false;
                for (let cellA = 0; cellA < 9; cellA++)
                    for (let cellB = cellA + 1; cellB < 9; cellB++)
                        if (grid[cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))] !== null &&
                            grid[cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))] === grid[cellB % 3 + 3 * (i % 3) + 9 * (((cellB / 3) | 0) + 3 * ((i / 3) | 0))])
                            return false;
            }

            // Check if any constraints are violated
            for (let constr of constraints)
                if (validateConstraint(grid, constr) === false)
                    return false;

            // Check that all cells in the Sudoku grid have a digit
            return grid.some(c => c === null) ? null : true;
        }

        function isKyudokuValid(corner)
        {
            let digitCounts = Array(9).fill(0);
            let availableDigitCounts = Array(9).fill(0);
            let rowSums = Array(6).fill(0);
            let colSums = Array(6).fill(0);
            for (let cell = 0; cell < 36; cell++)
            {
                if (state.circledDigits[corner][cell] === true)
                {
                    digitCounts[kyudokuGrids[corner][cell] - 1]++;
                    rowSums[(cell / 6) | 0] += kyudokuGrids[corner][cell];
                    colSums[cell % 6] += kyudokuGrids[corner][cell];
                }
                if (state.circledDigits[corner][cell] !== false)
                    availableDigitCounts[kyudokuGrids[corner][cell] - 1]++;
            }
            if (rowSums.some(r => r > 9) || colSums.some(r => r > 9) || digitCounts.some(c => c > 1) || availableDigitCounts.some(c => c === 0))
                return false;
            if (digitCounts.some(c => c === 0))
                return null;
            return true;
        }

        function updateVisuals(udpateStorage)
        {
            // Update localStorage (only do this when necessary because encodeState() is relatively slow on Firefox)
            if (localStorage && udpateStorage)
            {
                localStorage.setItem(`ky${puzzleId}`, encodeState(state));
                localStorage.setItem(`ky${puzzleId}-undo`, undoBuffer.map(encodeState).join(' '));
                localStorage.setItem(`ky${puzzleId}-redo`, redoBuffer.map(encodeState).join(' '));
            }
            resetRestartButton();

            // Check if there are any conflicts (red glow) and/or the puzzle is solved
            let isSolved = true;
            switch (isSudokuValid())
            {
                case false:
                    isSolved = false;
                    if (showErrors)
                        document.getElementById(`p-${puzzleId}-sudoku-frame`).classList.add('invalid');
                    break;

                case true:
                    document.getElementById(`p-${puzzleId}-sudoku-frame`).classList.remove('invalid');
                    break;

                case null:
                    isSolved = false;
                    document.getElementById(`p-${puzzleId}-sudoku-frame`).classList.remove('invalid');
                    break;
            }

            for (let corner = 0; corner < 4; corner++)
            {
                switch (isKyudokuValid(corner))
                {
                    case false:
                        isSolved = false;
                        if (showErrors)
                            document.getElementById(`p-${puzzleId}-kyudo-${corner}-frame`).classList.add('invalid');
                        break;

                    case true:
                        document.getElementById(`p-${puzzleId}-kyudo-${corner}-frame`).classList.remove('invalid');
                        break;

                    case null:
                        isSolved = false;
                        document.getElementById(`p-${puzzleId}-kyudo-${corner}-frame`).classList.remove('invalid');
                        break;
                }
            }

            setClass(puzzleDiv, 'solved', isSolved);
            if (isSolved)
                dbUpdate(true);

            // Kyudoku grids (digits, highlights, X’s/O’s — not red glow, that’s done further up)
            for (let corner = 0; corner < 4; corner++)
            {
                for (let cell = 0; cell < 36; cell++)
                {
                    let elem = document.getElementById(`p-${puzzleId}-kyudo-${corner}-${cell}`);
                    let sudokuCell = cell % 6 + 3 * (corner % 2) + 9 * (((cell / 6) | 0) + 3 * ((corner / 2) | 0));
                    setClass(elem, 'circled', state.circledDigits[corner][cell] === true);
                    let isXed = state.circledDigits[corner][cell] === false || (state.circledDigits[corner][cell] === null && (isSolved || (state.enteredDigits[sudokuCell] !== null && state.enteredDigits[sudokuCell] !== kyudokuGrids[corner][cell])));
                    setClass(elem, 'xed', isXed);
                    setClass(elem, 'highlighted', (selectedCells.includes(sudokuCell) || highlightedDigit === kyudokuGrids[corner][cell]) && (!isXed || semitransparentXs) && !isSolved);
                }
            }

            // Sudoku grid (digits, highlights — not red glow, that’s done further up)
            let digitCounts = Array(9).fill(0);
            for (let cell = 0; cell < 81; cell++)
            {
                let digit = getDisplayedSudokuDigit(state, cell);
                digitCounts[digit - 1]++;

                let sudokuCell = document.getElementById(`p-${puzzleId}-sudoku-${cell}`);
                setClass(sudokuCell, 'highlighted', (selectedCells.includes(cell) || (highlightedDigit !== null && digit === highlightedDigit)) && !isSolved);
                setClass(sudokuCell, 'invalid', digit === false);

                let intendedText = null;
                let intendedCenterDigits = null;
                let intendedCornerDigits = null;

                if (digit)
                    intendedText = digit;
                else
                {
                    intendedCenterDigits = state.centerNotation[cell].join('');
                    intendedCornerDigits = state.cornerNotation[cell];
                }

                document.getElementById(`p-${puzzleId}-sudoku-text-${cell}`).textContent = intendedText !== null ? intendedText : '';
                document.getElementById(`p-${puzzleId}-sudoku-center-text-${cell}`).textContent = intendedCenterDigits !== null ? intendedCenterDigits : '';
                for (var i = 0; i < 8; i++)
                    document.getElementById(`p-${puzzleId}-sudoku-corner-text-${cell}-${i}`).textContent = intendedCornerDigits !== null && i < intendedCornerDigits.length ? intendedCornerDigits[i] : '';
            }

            // Button highlights
            for (let btn of ["normal", "center", "corner"])
                setClass(document.getElementById(`p-${puzzleId}-btn-${btn}`), 'selected', mode === btn);

            for (let digit = 0; digit < 9; digit++)
            {
                setClass(document.getElementById(`p-${puzzleId}-btn-${digit + 1}`), 'selected', highlightedDigit === digit + 1);
                setClass(document.getElementById(`p-${puzzleId}-btn-${digit + 1}-left`), 'selected', highlightedDigit === digit + 1);
                setClass(document.getElementById(`p-${puzzleId}-btn-${digit + 1}`), 'success', digitCounts[digit] === 9);
                setClass(document.getElementById(`p-${puzzleId}-btn-${digit + 1}-left`), 'success', digitCounts[digit] === 9);
            }

            setClass(document.getElementById(`p-${puzzleId}-btn-help`), 'selected', helpEnabled);
        }
        updateVisuals(true);

        function saveUndo()
        {
            undoBuffer.push(JSON.parse(JSON.stringify(state)));
            redoBuffer = [];
        }

        function undo()
        {
            if (undoBuffer.length > 0)
            {
                redoBuffer.push(state);
                var item = undoBuffer.pop();
                state = item;
                updateVisuals(true);
            }
        }

        function redo()
        {
            if (redoBuffer.length > 0)
            {
                undoBuffer.push(state);
                var item = redoBuffer.pop();
                state = item;
                updateVisuals(true);
            }
        }

        function enterCenterNotation(digit)
        {
            if (selectedCells.every(c => getDisplayedSudokuDigit(state, c)))
                return;
            saveUndo();
            let allHaveDigit = selectedCells.filter(c => !getDisplayedSudokuDigit(state, c)).every(c => state.centerNotation[c].includes(digit));
            selectedCells.forEach(cell =>
            {
                if (allHaveDigit)
                    state.centerNotation[cell].splice(state.centerNotation[cell].indexOf(digit), 1);
                else if (!state.centerNotation[cell].includes(digit))
                {
                    state.centerNotation[cell].push(digit);
                    state.centerNotation[cell].sort();
                }
            });
            updateVisuals(true);
        }

        function enterCornerNotation(digit)
        {
            if (selectedCells.every(c => getDisplayedSudokuDigit(state, c)))
                return;
            saveUndo();
            let allHaveDigit = selectedCells.filter(c => !getDisplayedSudokuDigit(state, c)).every(c => state.cornerNotation[c].includes(digit));
            selectedCells.forEach(cell =>
            {
                if (allHaveDigit)
                    state.cornerNotation[cell].splice(state.cornerNotation[cell].indexOf(digit), 1);
                else if (!state.cornerNotation[cell].includes(digit))
                {
                    state.cornerNotation[cell].push(digit);
                    state.cornerNotation[cell].sort();
                }
            });
            updateVisuals(true);
        }

        function pressDigit(digit)
        {
            if (selectedCells.length === 0)
            {
                // Highlight digits in the Kyudokus

                if (highlightedDigit === digit)
                    highlightedDigit = null;
                else
                    highlightedDigit = digit;
                updateVisuals();
            }
            else
            {
                // Enter a digit in the Sudoku
                switch (mode)
                {
                    case 'normal':
                        saveUndo();
                        let allHaveDigit = selectedCells.every(c => getDisplayedSudokuDigit(state, c) === digit);
                        if (allHaveDigit)
                            selectedCells.forEach(selectedCell => { state.enteredDigits[selectedCell] = null; });
                        else
                            selectedCells.forEach(selectedCell => { state.enteredDigits[selectedCell] = digit; });
                        updateVisuals(true);
                        break;
                    case 'center':
                        enterCenterNotation(digit);
                        break;
                    case 'corner':
                        enterCornerNotation(digit);
                        break;
                }
            }
        }

        function autofill()
        {
            var anyChanges = false;
            for (let cell of selectedCells)
                if (!getDisplayedSudokuDigit(state, cell))
                {
                    let poss = [1, 2, 3, 4, 5, 6, 7, 8, 9];
                    for (let otherCell = 0; otherCell < 81; otherCell++)
                    {
                        let dd = getDisplayedSudokuDigit(state, otherCell);
                        if (dd && poss.includes(dd) && (cell % 9 === otherCell % 9 || ((cell / 9) | 0) === ((otherCell / 9) | 0) || ((((cell % 9) / 3) | 0) === (((otherCell % 9) / 3) | 0) && ((((cell / 9) | 0) / 3) | 0) === ((((otherCell / 9) | 0) / 3) | 0))))
                            poss.splice(poss.indexOf(dd), 1);
                    }
                    if (!anyChanges && (poss.join(',') !== state.centerNotation[cell].join(',')))
                    {
                        anyChanges = true;
                        saveUndo();
                    }
                    state.centerNotation[cell] = poss;
                }
            if (anyChanges)
                updateVisuals(true);
        }

        Array.from(puzzleDiv.getElementsByClassName('kyudo-cell')).forEach(cellRect =>
        {
            let corner = cellRect.dataset.corner | 0;
            let cell = cellRect.dataset.cell | 0;
            function doClick(ev)
            {
                saveUndo();
                if (state.circledDigits[corner][cell] === null)
                    state.circledDigits[corner][cell] = (ev.shiftKey ? true : false);
                else if (state.circledDigits[corner][cell] === false)
                    state.circledDigits[corner][cell] = (ev.shiftKey ? null : true);
                else
                    state.circledDigits[corner][cell] = (ev.shiftKey ? false : null);
                updateVisuals(true);
            };
            cellRect.onclick = handler(doClick);
            cellRect.onmousedown = handler(function() { });
            cellRect.onmouseenter = function() { hoveredKyDigit = [corner, cell]; };
            cellRect.onmouseout = function() { hoveredKyDigit = null; };
            cellRect.ontouchend = handler(doClick);
        });

        var tooltip = null;
        function clearTooltip()
        {
            if (tooltip !== null)
            {
                tooltip.parentNode.removeChild(tooltip);
                tooltip = null;
            }
        }

        Array.from(puzzleDiv.getElementsByClassName('sudoku-cell')).forEach(cellRect =>
        {
            let cell = parseInt(cellRect.dataset.cell);
            cellRect.onclick = handler(function() { remoteLog2(`onclick ${cell}`); });
            cellRect.onmousedown = cellRect.ontouchstart = handler(function(ev)
            {
                if (draggingMode !== null)
                {
                    remoteLog2(`${ev.type} ${cell} (canceled)`);
                    return;
                }
                let shift = ev.ctrlKey || ev.shiftKey;
                draggingMode = shift && selectedCells.includes(cell) ? 'remove' : 'add';
                highlightedDigit = null;
                selectCell(cell, shift ? draggingMode : 'toggle');
                updateVisuals();
                remoteLog2(`${ev.type} ${cell} (${ev.x}, ${ev.y})`);
            });
            cellRect.onmousemove = function(ev)
            {
                if (draggingMode === null)
                {
                    remoteLog2(`onmousemove ${cell} (canceled)`);
                    return;
                }
                selectCell(cell, draggingMode);
                updateVisuals();
                remoteLog2(`onmousemove ${cell} (${ev.x}, ${ev.y})`);
            };
            cellRect.ontouchmove = function(ev)
            {
                if (draggingMode === null)
                {
                    remoteLog2(`ontouchmove ${cell} (canceled)`);
                    return;
                }
                let any = false;
                for (let touch of ev.touches)
                {
                    let elem = document.elementFromPoint(touch.pageX, touch.pageY);
                    if (elem && elem.dataset.cell !== undefined)
                    {
                        selectCell(elem.dataset.cell | 0, draggingMode);
                        any = true;
                    }
                }
                if (any)
                    updateVisuals();
                remoteLog2(`ontouchmove ${cell}`);
            };
        });

        Array.from(puzzleDiv.getElementsByClassName('has-tooltip')).forEach(rect =>
        {
            rect.onmouseout = handler(clearTooltip);
            rect.onmouseenter = function()
            {
                if (!helpEnabled || !rect.dataset.description)
                    return;
                function e(name) { return document.createElementNS('http://www.w3.org/2000/svg', name); }
                tooltip = e('g');
                tooltip.setAttribute('text-anchor', 'middle');
                tooltip.setAttribute('font-size', '.35');
                let y = -.3;
                function makeText(str, isBold, offset)
                {
                    let elem = e('text');
                    elem.textContent = str;
                    if (isBold)
                        elem.setAttribute('font-weight', 'bold');
                    elem.setAttribute('x', '0');
                    elem.setAttribute('y', y);
                    tooltip.appendChild(elem);
                    y += offset;
                    return elem;
                }
                let names = JSON.parse(rect.dataset.name);
                let descrs = JSON.parse(rect.dataset.description);
                for (let cn = 0; cn < names.length; cn++)
                {
                    y += .3;
                    makeText(names[cn], true, .7);
                    let str = descrs[cn];
                    let wordWrapWidth = 55;
                    while (str.length > 0)
                    {
                        let txt = str;
                        if (str.length > wordWrapWidth)
                        {
                            let p = str.lastIndexOf(' ', wordWrapWidth);
                            txt = str.substr(0, p === -1 ? wordWrapWidth : p + 1);
                        }
                        str = str.substr(txt.length).trim();
                        makeText(txt.trim(), false, .5);
                    }
                }
                let tooltipWidth = 9.75;
                let rightEdge = (rect.getAttribute('x') | 0) === 9;
                tooltip.setAttribute('transform', rightEdge
                    ? `translate(${8.7 - tooltipWidth / 2}, ${(rect.getAttribute('y') | 0) + .75})`
                    : `translate(${(rect.getAttribute('x') | 0) - tooltipWidth / 2 + 1.25}, ${(rect.getAttribute('y') | 0) + 2})`);

                let path = e('path');
                path.setAttribute('d', rightEdge ? `m${-tooltipWidth / 2} -.7 ${tooltipWidth} 0 0 .25 .25 .25 -.25 .25 v ${y - .05} h ${-tooltipWidth} z` : `m${-tooltipWidth / 2} -.7 ${tooltipWidth - 1} 0 .25 -.25 .25 .25 .5 0 v ${y + .7} h ${-tooltipWidth} z`);
                path.setAttribute('fill', '#fcedca');
                path.setAttribute('stroke', 'black');
                path.setAttribute('stroke-width', '.025');
                tooltip.insertBefore(path, tooltip.firstChild);

                document.getElementById(`p-${puzzleId}-sudoku`).appendChild(tooltip);
            };
        });

        function setButtonHandler(btn, click)
        {
            btn.onclick = handler(ev => click(ev));
            btn.onmousedown = handler(function() { });
        }

        Array(9).fill(null).forEach((_, btn) =>
        {
            setButtonHandler(document.getElementById(`p-${puzzleId}-btn-${btn + 1}`), function() { pressDigit(btn + 1); });
            setButtonHandler(document.getElementById(`p-${puzzleId}-btn-${btn + 1}-left`), function() { pressDigit(btn + 1); });
        });

        ["normal", "corner", "center"].forEach(btn => setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-${btn}>rect`), function()
        {
            mode = btn;
            updateVisuals();
        }));

        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-help>rect`), function()
        {
            helpEnabled = !helpEnabled;
            updateVisuals();
            if (helpEnabled)
                localStorage.removeItem('kyu-help');
            else
                localStorage.setItem('kyu-help', 'Off');
        });

        ['', '-left'].forEach(xtr =>
        {
            setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart${xtr}>rect`), function()
            {
                let elem = document.getElementById(`p-${puzzleId}-btn-restart`);
                if (!elem.classList.contains('warning'))
                {
                    elem.classList.add('warning');
                    document.getElementById(`p-${puzzleId}-btn-restart-left`).classList.add('warning');
                    puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart>text`).textContent = 'Confirm?';
                    puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart-left>text`).textContent = 'Confirm?';
                }
                else
                {
                    resetRestartButton();
                    saveUndo();
                    state = {
                        circledDigits: Array(4).fill(null).map(_ => Array(36).fill(null)),
                        cornerNotation: Array(81).fill(null).map(_ => []),
                        centerNotation: Array(81).fill(null).map(_ => []),
                        enteredDigits: Array(81).fill(null)
                    };
                    updateVisuals(true);
                }
            });
        });

        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-undo>rect`), undo);
        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-undo-left>rect`), undo);
        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-redo>rect`), redo);
        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-redo-left>rect`), redo);
        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-fill>rect`), autofill);
        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-switch>rect`), function() { mobileLeft = !mobileLeft; setView(); });
        setButtonHandler(puzzleDiv.querySelector(`#p-${puzzleId}-btn-switch-left>rect`), function() { mobileLeft = !mobileLeft; setView(); });

        function selectCell(cell, mode)
        {
            if (mode === 'toggle')
            {
                if (selectedCells.length === 1 && selectedCells.includes(cell))
                    selectedCells = [];
                else
                    selectedCells = [cell];
            }
            else if (mode === 'remove')
            {
                let ix = selectedCells.indexOf(cell);
                if (ix !== -1)
                    selectedCells.splice(ix, 1);
            }
            else if (mode === 'clear')
            {
                selectedCells = [cell];
                keepMove = false;
            }
            else if (mode === 'add' || (mode === 'move' && keepMove))
            {
                let ix = selectedCells.indexOf(cell);
                if (ix !== -1)
                    selectedCells.splice(ix, 1);
                selectedCells.push(cell);
                keepMove = false;
            }
            else    // mode === 'move' && !keepMove
            {
                selectedCells.pop();
                selectedCells.push(cell);
            }
        }

        let keepMove = false;
        puzzleDiv.addEventListener("keydown", ev =>
        {
            let str = ev.code;
            if (ev.shiftKey)
                str = `Shift+${str}`;
            if (ev.altKey)
                str = `Alt+${str}`;
            if (ev.ctrlKey)
                str = `Ctrl+${str}`;

            let anyFunction = true;

            function ArrowMovement(dx, dy, mode)
            {
                highlightedDigit = null;
                if (selectedCells.length === 0)
                    selectedCells = [0];
                else
                {
                    let lastCell = selectedCells[selectedCells.length - 1];
                    let newX = ((lastCell % 9) + 9 + dx) % 9;
                    let newY = (((lastCell / 9) | 0) + 9 + dy) % 9;
                    let coord = newX + 9 * newY;
                    selectCell(coord, mode);
                }
                updateVisuals();
            }

            switch (str)
            {
                // Keys that change something
                case 'Digit1': case 'Numpad1':
                case 'Digit2': case 'Numpad2':
                case 'Digit3': case 'Numpad3':
                case 'Digit4': case 'Numpad4':
                case 'Digit5': case 'Numpad5':
                case 'Digit6': case 'Numpad6':
                case 'Digit7': case 'Numpad7':
                case 'Digit8': case 'Numpad8':
                case 'Digit9': case 'Numpad9':
                    pressDigit(parseInt(str.substr(str.length - 1)));
                    break;

                case 'Ctrl+Digit1': case 'Ctrl+Numpad1':
                case 'Ctrl+Digit2': case 'Ctrl+Numpad2':
                case 'Ctrl+Digit3': case 'Ctrl+Numpad3':
                case 'Ctrl+Digit4': case 'Ctrl+Numpad4':
                case 'Ctrl+Digit5': case 'Ctrl+Numpad5':
                case 'Ctrl+Digit6': case 'Ctrl+Numpad6':
                case 'Ctrl+Digit7': case 'Ctrl+Numpad7':
                case 'Ctrl+Digit8': case 'Ctrl+Numpad8':
                case 'Ctrl+Digit9': case 'Ctrl+Numpad9':
                    enterCenterNotation(parseInt(str.substr(str.length - 1)));
                    break;

                case 'Shift+Digit1': case 'Shift+Numpad1':
                case 'Shift+Digit2': case 'Shift+Numpad2':
                case 'Shift+Digit3': case 'Shift+Numpad3':
                case 'Shift+Digit4': case 'Shift+Numpad4':
                case 'Shift+Digit5': case 'Shift+Numpad5':
                case 'Shift+Digit6': case 'Shift+Numpad6':
                case 'Shift+Digit7': case 'Shift+Numpad7':
                case 'Shift+Digit8': case 'Shift+Numpad8':
                case 'Shift+Digit9': case 'Shift+Numpad9':
                    enterCornerNotation(parseInt(str.substr(str.length - 1)));
                    break;

                case 'Delete':
                    saveUndo();
                    selectedCells.forEach(selectedCell =>
                    {
                        state.enteredDigits[selectedCell] = null;
                        state.centerNotation[selectedCell] = [];
                        state.cornerNotation[selectedCell] = [];
                    });
                    updateVisuals(true);
                    break;

                case 'KeyF': autofill(); break;

                case 'KeyQ':
                case 'KeyW':
                case 'KeyE':
                    if (hoveredKyDigit !== null)
                    {
                        saveUndo();
                        state.circledDigits[hoveredKyDigit[0]][hoveredKyDigit[1]] = (str === 'KeyE' ? null : str === 'KeyQ');
                        updateVisuals(true);
                    }
                    break;

                // Navigation
                case 'KeyZ': mode = 'normal'; updateVisuals(); break;
                case 'KeyX': mode = 'corner'; updateVisuals(); break;
                case 'KeyC': mode = 'center'; updateVisuals(); break;

                case 'ArrowUp': ArrowMovement(0, -1, 'clear'); break;
                case 'ArrowDown': ArrowMovement(0, 1, 'clear'); break;
                case 'ArrowLeft': ArrowMovement(-1, 0, 'clear'); break;
                case 'ArrowRight': ArrowMovement(1, 0, 'clear'); break;
                case 'Shift+ArrowUp': ArrowMovement(0, -1, 'add'); break;
                case 'Shift+ArrowDown': ArrowMovement(0, 1, 'add'); break;
                case 'Shift+ArrowLeft': ArrowMovement(-1, 0, 'add'); break;
                case 'Shift+ArrowRight': ArrowMovement(1, 0, 'add'); break;
                case 'Ctrl+ArrowUp': ArrowMovement(0, -1, 'move'); break;
                case 'Ctrl+ArrowDown': ArrowMovement(0, 1, 'move'); break;
                case 'Ctrl+ArrowLeft': ArrowMovement(-1, 0, 'move'); break;
                case 'Ctrl+ArrowRight': ArrowMovement(1, 0, 'move'); break;
                case 'Ctrl+ControlLeft': case 'Ctrl+ControlRight': keepMove = true; break;
                case 'Ctrl+Space':
                    if (highlightedDigit !== null)
                    {
                        selectedCells = [];
                        for (let cell = 0; cell < 81; cell++)
                            if (getDisplayedSudokuDigit(state, cell) === highlightedDigit)
                                selectedCells.push(cell);
                        highlightedDigit = null;
                    }
                    else if (selectedCells.length >= 2 && selectedCells[selectedCells.length - 2] === selectedCells[selectedCells.length - 1])
                        selectedCells.splice(selectedCells.length - 1, 1);
                    else
                        keepMove = !keepMove;
                    updateVisuals();
                    break;
                case 'Escape': selectedCells = []; highlightedDigit = null; updateVisuals(); break;

                // Undo/redo
                case 'Backspace':
                case 'Ctrl+KeyZ':
                    undo();
                    break;

                case 'Shift+Backspace':
                case 'Ctrl+KeyY':
                    redo();
                    break;

                default:
                    anyFunction = false;
                    //console.log(str, ev.code);
                    break;
            }

            if (anyFunction)
            {
                ev.stopPropagation();
                ev.preventDefault();
                return false;
            }
        });

        puzzleDiv.onmousedown = function(ev)
        {
            if (!ev.shiftKey && !ev.ctrlKey)
            {
                selectedCells = [];
                highlightedDigit = null;
                updateVisuals();
                remoteLog2(`onmousedown puzzleDiv`);
            }
            else
                remoteLog2(`onmousedown puzzleDiv (canceled)`);
        };

        let puzzleSvg = puzzleDiv.getElementsByTagName('svg')[0];

        function setViewBox(left, top, width, height)
        {
            puzzleSvg.setAttribute('viewBox', `${left} ${top} ${width} ${height}`);
        }

        function setView()
        {
            let isMobile = (window.innerWidth <= 860);

            let numBarLeft = document.getElementById(`p-${puzzleId}-btns-numleft`);
            let btns = null;
            if (!isMobile)
            {
                setViewBox(-.5, -.5 - (+puzzleSvg.dataset.extratop), 24 + (+puzzleSvg.dataset.extraright), 13.75 + (+puzzleSvg.dataset.extratop));
                numBarLeft.setAttribute('transform', 'translate(0, 20)');   // intentionally outside the viewBox
                document.getElementById(`p-${puzzleId}-btn-switch`).setAttribute('transform', 'translate(0, 20)'); // intentionally outside the viewBox
                btns = {
                    'restart': { t: 'translate(0, 2.2)', w: 2.5 },
                    'undo': { t: 'translate(2.73, 2.2)', w: 2.5 },
                    'redo': { t: 'translate(5.47, 2.2)', w: 2.5 }
                };
                document.getElementById(`p-${puzzleId}-solved-sticker`).setAttribute('transform', 'translate(11.5, 6) rotate(-15)');
            }
            else if (mobileLeft)
            {
                setViewBox(-.25, -.25, 13.25, 17);
                numBarLeft.setAttribute('transform', 'translate(0, 13.75) scale(1.5)');
                document.getElementById(`p-${puzzleId}-solved-sticker`).setAttribute('transform', 'translate(6.375, 6.375) scale(.8) rotate(-15)');
            }
            else
            {
                setViewBox(13.5 - (+puzzleSvg.dataset.extraleft), -.5 - (+puzzleSvg.dataset.extratop), 10 + (+puzzleSvg.dataset.extraleft) + (+puzzleSvg.dataset.extraright), 13.5 + (+puzzleSvg.dataset.extratop));
                document.getElementById(`p-${puzzleId}-btn-switch`).setAttribute('transform', 'translate(0, 2.2)');
                btns = {
                    'restart': { t: 'translate(1.0375, 2.2)', w: 2.45 },
                    'undo': { t: 'translate(3.725, 2.2)', w: 2 },
                    'redo': { t: 'translate(5.9625, 2.2)', w: 2 }
                };
                document.getElementById(`p-${puzzleId}-solved-sticker`).setAttribute('transform', 'translate(18.5, 4.5) scale(.59) rotate(-15)');
            }
            if (btns)
            {
                for (let k in btns)
                {
                    puzzleDiv.querySelector(`#p-${puzzleId}-btn-${k}`).setAttribute('transform', btns[k].t);
                    puzzleDiv.querySelector(`#p-${puzzleId}-btn-${k}>rect`).setAttribute('width', btns[k].w);
                    let txt = puzzleDiv.querySelector(`#p-${puzzleId}-btn-${k}>text`);
                    if (txt)
                        txt.setAttribute('x', btns[k].w / 2);
                }
            }

            // Set the width to 100% in order to measure its height, then dynamically resize if the SVG is too wide.
            puzzleSvg.style.width = '100vw';
            puzzleDiv.style.minHeight = '';
            let puzzleHeight = puzzleDiv.offsetHeight;
            let availableHeight = window.innerHeight - document.getElementById('topbar').offsetHeight;
            let warning = document.querySelector('.warning');
            if (warning !== null)
                availableHeight -= warning.offsetHeight;
            if (puzzleHeight > availableHeight)
            {
                puzzleDiv.style.display = 'none';
                puzzleSvg.style.width = `${100 * availableHeight / puzzleHeight}vw`;
                puzzleDiv.style.display = '';
            }
            puzzleDiv.style.minHeight = `calc(100vh - ${isMobile ? 1 : 2}cm)`;
        };

        window.addEventListener('resize', setView);
    });

    window.setTimeout(function() { window.dispatchEvent(new Event('resize')); }, 1);
});