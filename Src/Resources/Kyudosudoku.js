window.addEventListener('DOMContentLoaded', function()
{
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

    let colors = [
        ["white", "#aaa"],
        ["hsl(0, 100%, 94%)", "hsl(0, 70%, 50%)"],
        ["white", "#aaa"],
        ["hsl(52, 100%, 89%)", "hsl(52, 80%, 40%)"],
        ["hsl(0, 0%, 94%)", "#999"],
        ["hsl(226, 100%, 94%)", "hsl(226, 60%, 50%)"],
        ["white", "#aaa"],
        ["hsl(103, 84%, 95%)", "hsl(103, 50%, 50%)"],
        ["white", "#aaa"]
    ];
    let invalidCellColor = '#f00';

    let first = true;
    let draggingMode = null;
    let hasDragged = false;
    document.body.onmouseup = handler(function() { draggingMode = null; });

    Array.from(document.getElementsByClassName('puzzle')).forEach(puzzleDiv =>
    {
        let match = /^puzzle-(\d+)$/.exec(puzzleDiv.id);
        if (!match)
        {
            console.error(`Unexpected puzzle ID: ${puzzleDiv.id}`);
            return;
        }
        let puzzleId = parseInt(match[1]);

        if (first)
        {
            puzzleDiv.focus();
            first = false;
        }

        let kyudokuGrids = [0, 1, 2, 3].map(corner => Array(36).fill(null).map((_, cell) => parseInt(document.getElementById(`p-${puzzleId}-kyudo-${corner}-text-${cell}`).textContent)));

        let state = {
            circledDigits: Array(4).fill(null).map(_ => Array(36).fill(null)),
            cornerNotation: Array(81).fill(null).map(_ => []),
            centerNotation: Array(81).fill(null).map(_ => []),
            enteredDigits: Array(81).fill(null)
        };
        let undoBuffer = [JSON.parse(JSON.stringify(state))];
        let redoBuffer = [];

        let mode = 'normal';
        let selectedCells = [];
        let highlightedDigit = null;

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
            let item;
            if (puzzleDiv.dataset.progress)
                item = JSON.parse(puzzleDiv.dataset.progress);
            else
            {
                str = localStorage.getItem(`ky${puzzleId}`);
                if (str !== null)
                    try { item = JSON.parse(localStorage.getItem(`ky${puzzleId}`)); }
                    catch { item = decodeState(str); }
            }
            if (item && item.circledDigits && item.cornerNotation && item.centerNotation && item.enteredDigits)
                state = item;

            let undoB = localStorage.getItem(`ky${puzzleId}-undo`);
            let redoB = localStorage.getItem(`ky${puzzleId}-redo`);

            undoBuffer = undoB ? undoB.split(' ').map(decodeState) : [JSON.parse(JSON.stringify(state))];
            redoBuffer = redoB ? redoB.split(' ').map(decodeState) : [];
        }
        catch
        {
        }

        let dbUpdater = setInterval(dbUpdate, 10000);
        let timeLastDbUpdate = new Date();

        function dbUpdate(isSolved)
        {
            let req = new XMLHttpRequest();
            req.open('POST', `db-update/${puzzleId}`, true);
            req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            req.send(`progress=${encodeURIComponent(JSON.stringify(state))}${isSolved ? `&time=${((new Date() - timeLastDbUpdate) / 1000) | 0}&getdata=1` : ''}`);
            req.onload = function()
            {
                if (req.responseText)
                {
                    var json = JSON.parse(req.responseText);
                    if (json)
                    {
                        puzzleDiv.querySelector('text.inf-time').textContent = json.time ? (json.time < 60 ? `${json.time} seconds` : json.time < 60 * 60 ? `${(json.time / 60) | 0} min ${json.time % 60} sec` : `${(json.time / 60 / 60) | 0} h ${((json.time / 60) | 0) % 60} min ${json.time % 60} sec`) : 'not recorded';
                        puzzleDiv.querySelector('text.inf-avg').textContent = json.avg ? (json.avg < 60 ? `${json.avg} seconds` : json.avg < 60 * 60 ? `${(json.avg / 60) | 0} min ${json.avg % 60} sec` : `${(json.avg / 60 / 60) | 0} h ${((json.avg / 60) | 0) % 60} min ${json.avg % 60} sec`) : 'unknown';
                        puzzleDiv.querySelector('text.inf-count').textContent = json.count === 1 ? "once" : `${json.count | 0} times`;
                    }
                }
            };

            timeLastDbUpdate = new Date();
            if (isSolved)
                clearInterval(dbUpdater);
        }

        function cellColor(cell, isSelected)
        {
            return colors[(((cell % 9) / 3) | 0) + 3 * ((((cell / 9) | 0) / 3) | 0)][isSelected ? 1 : 0];
        }

        function textColor(cell, isSelected)
        {
            return isSelected ? 'white' : 'black';
        }

        function notationColor(cell, isSelected)
        {
            return isSelected ? 'white' : 'hsl(217, 80%, 50%)';
        }

        function resetRestartButton()
        {
            puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart`).classList.remove('warning');
            puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart>text`).textContent = 'Restart';
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
            if (kyu === null && st.enteredDigits[cell] !== null)
                return st.enteredDigits[cell];
            return kyu === false ? null : kyu;
        }

        function isSudokuValid()
        {
            // Check the Sudoku rules (rows, columns and regions)
            for (let i = 0; i < 9; i++)
            {
                for (let colA = 0; colA < 9; colA++)
                    for (let colB = colA + 1; colB < 9; colB++)
                        if (getDisplayedSudokuDigit(state, colA + 9 * i) !== null && getDisplayedSudokuDigit(state, colA + 9 * i) === getDisplayedSudokuDigit(state, colB + 9 * i))
                            return false;
                for (let rowA = 0; rowA < 9; rowA++)
                    for (let rowB = rowA + 1; rowB < 9; rowB++)
                        if (getDisplayedSudokuDigit(state, i + 9 * rowA) !== null && getDisplayedSudokuDigit(state, i + 9 * rowA) === getDisplayedSudokuDigit(state, i + 9 * rowB))
                            return false;
                for (let cellA = 0; cellA < 9; cellA++)
                    for (let cellB = cellA + 1; cellB < 9; cellB++)
                        if (getDisplayedSudokuDigit(state, cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))) !== null && getDisplayedSudokuDigit(state, cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))) === getDisplayedSudokuDigit(state, cellB % 3 + 3 * (i % 3) + 9 * (((cellB / 3) | 0) + 3 * ((i / 3) | 0))))
                            return false;
            }

            // Check that all cells in the Sudoku grid have a digit
            for (let cell = 0; cell < 81; cell++)
                if (getDisplayedSudokuDigit(state, cell) === null)
                    return null;

            return true;
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

        function updateVisuals()
        {
            // Update localStorage
            if (localStorage)
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
                    if (puzzleDiv.dataset.showerrors === '1')
                        document.getElementById(`p-${puzzleId}-sudoku-frame`).classList.add('invalid-glow');
                    break;

                case true:
                    document.getElementById(`p-${puzzleId}-sudoku-frame`).classList.remove('invalid-glow');
                    break;

                case null:
                    isSolved = false;
                    document.getElementById(`p-${puzzleId}-sudoku-frame`).classList.remove('invalid-glow');
                    break;
            }

            for (let corner = 0; corner < 4; corner++)
            {
                switch (isKyudokuValid(corner))
                {
                    case false:
                        isSolved = false;
                        if (puzzleDiv.dataset.showerrors === '1')
                            document.getElementById(`p-${puzzleId}-kyudo-${corner}-frame`).classList.add('invalid-glow');
                        break;

                    case true:
                        document.getElementById(`p-${puzzleId}-kyudo-${corner}-frame`).classList.remove('invalid-glow');
                        break;

                    case null:
                        isSolved = false;
                        document.getElementById(`p-${puzzleId}-kyudo-${corner}-frame`).classList.remove('invalid-glow');
                        break;
                }
            }

            if (isSolved)
            {
                puzzleDiv.classList.add('solved');
                dbUpdate(true);
            }
            else
                puzzleDiv.classList.remove('solved');

            // Kyudoku grids (digits, highlights, X’s/O’s — not red glow, that’s done further up)
            for (let corner = 0; corner < 4; corner++)
            {
                for (let cell = 0; cell < 36; cell++)
                {
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-circle-${cell}`).setAttribute('opacity', state.circledDigits[corner][cell] === true ? '1' : '0');
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-x-${cell}`).setAttribute('opacity', (state.circledDigits[corner][cell] === false || (state.circledDigits[corner][cell] === null && isSolved)) ? '1' : '0');
                    let sudokuCell = cell % 6 + 3 * (corner % 2) + 9 * (((cell / 6) | 0) + 3 * ((corner / 2) | 0));
                    let isHighlighted = (selectedCells.includes(sudokuCell) || highlightedDigit === kyudokuGrids[corner][cell]) && (state.circledDigits[corner][cell] !== false) && !isSolved;
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-cell-${cell}`).setAttribute('fill', cellColor(sudokuCell, isHighlighted));
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-text-${cell}`).setAttribute('fill', textColor(sudokuCell, isHighlighted));
                }
            }

            // Sudoku grid (digits, highlights — not red glow, that’s done further up)
            let digitCounts = Array(9).fill(0);
            for (let cell = 0; cell < 81; cell++)
            {
                let kyDigit = getKyudokuCircledDigit(state, cell);
                let digit = getDisplayedSudokuDigit(state, cell);
                digitCounts[digit - 1]++;

                let sudokuCell = document.getElementById(`p-${puzzleId}-sudoku-cell-${cell}`);
                let sudokuText = document.getElementById(`p-${puzzleId}-sudoku-text-${cell}`);
                let sudokuCenterText = document.getElementById(`p-${puzzleId}-sudoku-center-text-${cell}`);
                let sudokuCornerTexts = Array(8).fill(null).map((_, ix) => document.getElementById(`p-${puzzleId}-sudoku-corner-text-${cell}-${ix}`));

                let intendedText = null;
                let intendedCenterDigits = null;
                let intendedCornerDigits = null;

                let isHighlighted = (selectedCells.includes(cell) || (highlightedDigit !== null && digit === highlightedDigit)) && !isSolved;
                sudokuCell.setAttribute('fill', cellColor(cell, isHighlighted));
                sudokuText.setAttribute('fill', textColor(cell, isHighlighted));
                if (kyDigit === false)
                    // Two equivalent Kyudoku cells with different numbers have been circled: mark the Sudoku cell red
                    sudokuCell.setAttribute('fill', invalidCellColor);
                else if (digit !== null)
                    intendedText = digit;
                else
                {
                    intendedCenterDigits = state.centerNotation[cell].join('');
                    intendedCornerDigits = state.cornerNotation[cell];
                }

                sudokuText.textContent = intendedText !== null ? intendedText : '';
                sudokuCenterText.textContent = intendedCenterDigits !== null ? intendedCenterDigits : '';
                sudokuCenterText.setAttribute('fill', notationColor(cell, isHighlighted));
                for (var i = 0; i < 8; i++)
                {
                    sudokuCornerTexts[i].textContent = intendedCornerDigits !== null && i < intendedCornerDigits.length ? intendedCornerDigits[i] : '';
                    sudokuCornerTexts[i].setAttribute('fill', notationColor(cell, isHighlighted));
                }
            }

            // Button highlights
            "normal,center,corner".split(',').forEach(btn =>
            {
                if (mode === btn)
                    document.getElementById(`p-${puzzleId}-btn-${btn}`).classList.add('selected');
                else
                    document.getElementById(`p-${puzzleId}-btn-${btn}`).classList.remove('selected');
            });

            Array(9).fill(null).forEach((_, digit) =>
            {
                let btn = document.getElementById(`p-${puzzleId}-num-${digit + 1}`);
                btn.classList.remove('selected');
                btn.classList.remove('success');

                if (highlightedDigit === digit + 1)
                    btn.classList.add('selected');
                else if (digitCounts[digit] === 9)
                    btn.classList.add('success');
            });
        }
        updateVisuals();

        function saveUndo()
        {
            undoBuffer.push(JSON.parse(JSON.stringify(state)));
            redoBuffer = [];
        }

        Array.from(puzzleDiv.getElementsByClassName('kyudo-cell')).forEach(cellRect =>
        {
            let match = /^p-\d+-kyudo-(\d+)-cell-(\d+)$/.exec(cellRect.id);
            if (!match)
            {
                console.error(`Unexpected cell ID: ${cellRect.id}`);
                return;
            }

            let corner = parseInt(match[1]);
            let cell = parseInt(match[2]);
            cellRect.onclick = handler(function(ev)
            {
                saveUndo();
                if (state.circledDigits[corner][cell] === null)
                    state.circledDigits[corner][cell] = (ev.shiftKey ? true : false);
                else if (state.circledDigits[corner][cell] === false)
                    state.circledDigits[corner][cell] = (ev.shiftKey ? null : true);
                else
                    state.circledDigits[corner][cell] = (ev.shiftKey ? false : null);
                updateVisuals();
            });
        });

        Array.from(puzzleDiv.getElementsByClassName('sudoku-cell')).forEach(cellRect =>
        {
            let match = /^p-\d+-sudoku-cell-(\d+)$/.exec(cellRect.id);
            if (!match)
            {
                console.error(`Unexpected cell ID: ${cellRect.id}`);
                return;
            }

            let cell = parseInt(match[1]);
            cellRect.onclick = handler(function() { });
            cellRect.onmousedown = handler(function(ev)
            {
                let shift = ev.ctrlKey || ev.shiftKey;
                draggingMode = shift && selectedCells.includes(cell) ? 'remove' : 'add';
                highlightedDigit = null;
                selectCell(cell, shift ? draggingMode : 'toggle');
                updateVisuals();
            });
            cellRect.onmousemove = handler(function()
            {
                if (draggingMode === null)
                    return;
                let oldLength = selectedCells.length;
                selectCell(cell, draggingMode);
                if (selectedCells.length !== oldLength)
                    hasDragged = true;
                updateVisuals();
            });
        });

        function undo()
        {
            if (undoBuffer.length > 0)
            {
                redoBuffer.push(state);
                var item = undoBuffer.pop();
                state = item;
                updateVisuals();
            }
        }

        function redo()
        {
            if (redoBuffer.length > 0)
            {
                undoBuffer.push(state);
                var item = redoBuffer.pop();
                state = item;
                updateVisuals();
            }
        }

        function enterCenterNotation(digit)
        {
            saveUndo();
            let allHaveDigit = selectedCells.filter(c => getDisplayedSudokuDigit(state, c) === null).every(c => state.centerNotation[c].includes(digit));
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
        }

        function enterCornerNotation(digit)
        {
            saveUndo();
            let allHaveDigit = selectedCells.filter(c => getDisplayedSudokuDigit(state, c) === null).every(c => state.cornerNotation[c].includes(digit));
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

        Array(9).fill(null).forEach((_, btn) =>
        {
            puzzleDiv.querySelector(`#p-${puzzleId}-num-${btn + 1}`).onclick = handler(function()
            {
                pressDigit(btn + 1);
                updateVisuals();
            });
        });

        "normal,corner,center".split(',').forEach(btn =>
        {
            puzzleDiv.querySelector(`#p-${puzzleId}-btn-${btn}>rect`).onclick = handler(function()
            {
                mode = btn;
                updateVisuals();
            });
        });

        puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart>rect`).onclick = handler(function()
        {
            var elem = puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart`);
            if (!elem.classList.contains('warning'))
            {
                elem.classList.add('warning');
                puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart>text`).textContent = 'Confirm?';
            }
            else
            {
                saveUndo();
                elem.classList.remove('warning');
                puzzleDiv.querySelector(`#p-${puzzleId}-btn-restart>text`).textContent = 'Restart';
                state = {
                    circledDigits: Array(4).fill(null).map(_ => Array(36).fill(null)),
                    cornerNotation: Array(81).fill(null).map(_ => []),
                    centerNotation: Array(81).fill(null).map(_ => []),
                    enteredDigits: Array(81).fill(null)
                };
                updateVisuals();
            }
        });

        puzzleDiv.querySelector(`#p-${puzzleId}-btn-undo>rect`).onclick = handler(undo);
        puzzleDiv.querySelector(`#p-${puzzleId}-btn-redo>rect`).onclick = handler(redo);

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
                    break;

                case 'KeyF':
                    saveUndo();
                    for (let cell of selectedCells)
                        if (getDisplayedSudokuDigit(state, cell) === null)
                        {
                            let poss = [1, 2, 3, 4, 5, 6, 7, 8, 9];
                            for (let otherCell = 0; otherCell < 81; otherCell++)
                            {
                                let dd = getDisplayedSudokuDigit(state, otherCell);
                                if (dd !== null && poss.includes(dd) && (cell % 9 === otherCell % 9 || ((cell / 9) | 0) === ((otherCell / 9) | 0) || ((((cell % 9) / 3) | 0) === (((otherCell % 9) / 3) | 0) && ((((cell / 9) | 0) / 3) | 0) === ((((otherCell / 9) | 0) / 3) | 0))))
                                    poss.splice(poss.indexOf(dd), 1);
                            }
                            state.centerNotation[cell] = poss;
                        }
                    break;

                // Navigation
                case 'KeyZ':
                    mode = 'normal';
                    break;
                case 'KeyX':
                    mode = 'corner';
                    break;
                case 'KeyC':
                    mode = 'center';
                    break;

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
                    break;
                case 'Escape': selectedCells = []; highlightedDigit = null; break;

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
                updateVisuals();
                ev.stopPropagation();
                ev.preventDefault();
                return false;
            }
        });

        puzzleDiv.onclick = handler(function()
        {
            if (hasDragged)
            {
                hasDragged = false;
                return;
            }
            selectedCells = [];
            updateVisuals();
        });

        let puzzleSvg = puzzleDiv.getElementsByTagName('svg')[0];
        window.onresize = function()
        {
            // Set the width to 100% in order to measure its height
            puzzleSvg.style.width = '100%';
            let puzzleHeight = puzzleDiv.offsetHeight;
            let availableHeight = window.innerHeight - document.querySelector('.top-bar').offsetHeight;
            let warning = document.querySelector('.warning');
            if (warning !== null)
                availableHeight -= warning.offsetHeight;
            if (puzzleHeight > availableHeight)
            {
                puzzleDiv.style.display = 'none';
                puzzleSvg.style.width = `${100 * availableHeight / puzzleHeight}%`;
                puzzleDiv.style.display = '';
            }
        };
        window.onresize();
    });
});