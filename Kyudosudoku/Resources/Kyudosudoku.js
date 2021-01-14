﻿window.addEventListener('DOMContentLoaded', function()
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
        let undoBuffer = [JSON.stringify(state)];
        let redoBuffer = [];

        let mode = 'normal';
        let selectedCells = [];
        let highlightedDigit = null;

        try
        {
            let item;
            if (puzzleDiv.dataset.progress)
                item = JSON.parse(puzzleDiv.dataset.progress);
            else
                item = JSON.parse(localStorage.getItem(`ky${puzzleId}`));
            if (item && item.circledDigits && item.cornerNotation && item.centerNotation && item.enteredDigits)
                state = item;

            undoBuffer = /*JSON.parse(localStorage.getItem(`ky${puzzleId}-undo`)) ||*/[JSON.stringify(state)];
            redoBuffer = /*JSON.parse(localStorage.getItem(`ky${puzzleId}-redo`)) ||*/[];
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
            req.send(`progress=${encodeURIComponent(JSON.stringify(state))}${isSolved ? `&time=${((new Date() - timeLastDbUpdate) / 1000) | 0}` : ''}`);
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

        function getDisplayedSudokuDigit(cell)
        {
            let kyCells = [0, 1, 2, 3]
                .filter(c => cell % 9 >= 3 * (c % 2) && cell % 9 < 6 + 3 * (c % 2) && cell / 9 >= 3 * ((c / 2) | 0) && cell / 9 < 6 + 3 * ((c / 2) | 0))
                .map(c => ({ corner: c, kyCell: cell % 9 - 3 * (c % 2) + 6 * (((cell / 9) | 0) - 3 * ((c / 2) | 0)) }))
                .filter(inf => state.circledDigits[inf.corner][inf.kyCell] === true);
            if (kyCells.length > 1 && kyCells.some(inf => kyudokuGrids[inf.corner][inf.kyCell] !== kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]))
                return null;
            else if (kyCells.length >= 1)
                return parseInt(kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]);
            else if (state.enteredDigits[cell] !== null)
                return parseInt(state.enteredDigits[cell]);
            return null;
        }

        function isSudokuValid()
        {
            // Check the Sudoku rules (rows, columns and regions)
            for (let i = 0; i < 9; i++)
            {
                for (let colA = 0; colA < 9; colA++)
                    for (let colB = colA + 1; colB < 9; colB++)
                        if (getDisplayedSudokuDigit(colA + 9 * i) !== null && getDisplayedSudokuDigit(colA + 9 * i) === getDisplayedSudokuDigit(colB + 9 * i))
                            return false;
                for (let rowA = 0; rowA < 9; rowA++)
                    for (let rowB = rowA + 1; rowB < 9; rowB++)
                        if (getDisplayedSudokuDigit(i + 9 * rowA) !== null && getDisplayedSudokuDigit(i + 9 * rowA) === getDisplayedSudokuDigit(i + 9 * rowB))
                            return false;
                for (let cellA = 0; cellA < 9; cellA++)
                    for (let cellB = cellA + 1; cellB < 9; cellB++)
                        if (getDisplayedSudokuDigit(cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))) !== null && getDisplayedSudokuDigit(cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))) === getDisplayedSudokuDigit(cellB % 3 + 3 * (i % 3) + 9 * (((cellB / 3) | 0) + 3 * ((i / 3) | 0))))
                            return false;
            }

            // Check that all cells in the Sudoku grid have a digit
            for (let cell = 0; cell < 81; cell++)
                if (getDisplayedSudokuDigit(cell) === null)
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
            if (localStorage)
            {
                localStorage.setItem(`ky${puzzleId}`, JSON.stringify(state));
                //localStorage.setItem(`ky${puzzleId}-undo`, JSON.stringify(undoBuffer));
                //localStorage.setItem(`ky${puzzleId}-redo`, JSON.stringify(redoBuffer));
            }
            resetRestartButton();
            for (let corner = 0; corner < 4; corner++)
            {
                for (let cell = 0; cell < 36; cell++)
                {
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-circle-${cell}`).setAttribute('opacity', state.circledDigits[corner][cell] === true ? '1' : '0');
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-x-${cell}`).setAttribute('opacity', state.circledDigits[corner][cell] === false ? '1' : '0');
                    let sudokuCell = cell % 6 + 3 * (corner % 2) + 9 * (((cell / 6) | 0) + 3 * ((corner / 2) | 0));
                    let isHighlighted = (selectedCells.includes(sudokuCell) || highlightedDigit === kyudokuGrids[corner][cell]) && (state.circledDigits[corner][cell] !== false);
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-cell-${cell}`).setAttribute('fill', cellColor(sudokuCell, isHighlighted));
                    document.getElementById(`p-${puzzleId}-kyudo-${corner}-text-${cell}`).setAttribute('fill', textColor(sudokuCell, isHighlighted));
                }
            }

            let digitCounts = Array(9).fill(0);
            for (let cell = 0; cell < 81; cell++)
            {
                let digit = getDisplayedSudokuDigit(cell);
                digitCounts[digit - 1]++;

                let sudokuCell = document.getElementById(`p-${puzzleId}-sudoku-cell-${cell}`);
                let sudokuText = document.getElementById(`p-${puzzleId}-sudoku-text-${cell}`);
                let sudokuCenterText = document.getElementById(`p-${puzzleId}-sudoku-center-text-${cell}`);
                let sudokuCornerTexts = Array(8).fill(null).map((_, ix) => document.getElementById(`p-${puzzleId}-sudoku-corner-text-${cell}-${ix}`));

                let intendedText = null;
                let intendedCenterDigits = null;
                let intendedCornerDigits = null;

                sudokuCell.setAttribute('fill', cellColor(cell, selectedCells.includes(cell) || (highlightedDigit !== null && digit === highlightedDigit)));
                sudokuText.setAttribute('fill', textColor(cell, selectedCells.includes(cell) || (highlightedDigit !== null && digit === highlightedDigit)));
                let kyCells = [0, 1, 2, 3]
                    .filter(c => cell % 9 >= 3 * (c % 2) && cell % 9 < 6 + 3 * (c % 2) && cell / 9 >= 3 * ((c / 2) | 0) && cell / 9 < 6 + 3 * ((c / 2) | 0))
                    .map(c => ({ corner: c, kyCell: cell % 9 - 3 * (c % 2) + 6 * (((cell / 9) | 0) - 3 * ((c / 2) | 0)) }))
                    .filter(inf => state.circledDigits[inf.corner][inf.kyCell] === true);
                if (kyCells.length > 1 && kyCells.some(inf => kyudokuGrids[inf.corner][inf.kyCell] !== kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]))
                {
                    // Two equivalent Kyudoku cells with different numbers have been circled: mark the Sudoku cell red
                    sudokuCell.setAttribute('fill', invalidCellColor);
                }
                else if (kyCells.length >= 1)
                    intendedText = kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell];
                else if (state.enteredDigits[cell] !== null)
                    intendedText = state.enteredDigits[cell];
                else
                {
                    intendedCenterDigits = state.centerNotation[cell].join('');
                    intendedCornerDigits = state.cornerNotation[cell];
                }

                sudokuText.textContent = intendedText !== null ? intendedText : '';
                sudokuCenterText.textContent = intendedCenterDigits !== null ? intendedCenterDigits : '';
                sudokuCenterText.setAttribute('fill', notationColor(cell, selectedCells.includes(cell)));
                for (var i = 0; i < 8; i++)
                {
                    sudokuCornerTexts[i].textContent = intendedCornerDigits !== null && i < intendedCornerDigits.length ? intendedCornerDigits[i] : '';
                    sudokuCornerTexts[i].setAttribute('fill', notationColor(cell, selectedCells.includes(cell)));
                }
            }

            let isSolved = true;
            switch (isSudokuValid())
            {
                case false:
                    isSolved = false;
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
            undoBuffer.push(JSON.stringify(state));
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
            cellRect.onclick = handler(function()
            {
                saveUndo();
                if (state.circledDigits[corner][cell] === null)
                    state.circledDigits[corner][cell] = false;
                else if (state.circledDigits[corner][cell] === false)
                    state.circledDigits[corner][cell] = true;
                else
                    state.circledDigits[corner][cell] = null;
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
            cellRect.onclick = handler(function(ev)
            {
                highlightedDigit = null;
                if (ev.ctrlKey || ev.shiftKey)
                {
                    if (selectedCells.includes(cell))
                        selectedCells.splice(selectedCells.indexOf(cell), 1);
                    else
                        selectedCells.push(cell);
                }
                else
                {
                    if (selectedCells.length === 1 && selectedCells.includes(cell))
                        selectedCells = [];
                    else
                        selectedCells = [cell];
                }
                updateVisuals();
            });
        });

        function undo()
        {
            if (undoBuffer.length > 0)
            {
                redoBuffer.push(JSON.stringify(state));
                var item = undoBuffer.pop();
                state = JSON.parse(item);
                updateVisuals();
            }
        }

        function redo()
        {
            if (redoBuffer.length > 0)
            {
                undoBuffer.push(JSON.stringify(state));
                var item = redoBuffer.pop();
                state = JSON.parse(item);
                updateVisuals();
            }
        }

        function enterCenterNotation(digit)
        {
            saveUndo();
            let allHaveDigit = selectedCells.every(c => state.centerNotation[c].includes(digit));
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
            let allHaveDigit = selectedCells.every(c => state.cornerNotation[c].includes(digit));
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
                        let allHaveDigit = selectedCells.every(c => getDisplayedSudokuDigit(c) === digit);
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

            function ArrowMovement(dx, dy, additive)
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
                    if (additive)
                    {
                        let ix = selectedCells.indexOf(coord);
                        if (ix !== -1)
                            selectedCells.splice(ix, 1);
                        selectedCells.push(coord);
                    }
                    else
                        selectedCells = [coord];
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

                case 'ArrowUp': ArrowMovement(0, -1); break;
                case 'ArrowDown': ArrowMovement(0, 1); break;
                case 'ArrowLeft': ArrowMovement(-1, 0); break;
                case 'ArrowRight': ArrowMovement(1, 0); break;
                case 'Shift+ArrowUp': case 'Ctrl+ArrowUp': ArrowMovement(0, -1, true); break;
                case 'Shift+ArrowDown': case 'Ctrl+ArrowDown': ArrowMovement(0, 1, true); break;
                case 'Shift+ArrowLeft': case 'Ctrl+ArrowLeft': ArrowMovement(-1, 0, true); break;
                case 'Shift+ArrowRight': case 'Ctrl+ArrowRight': ArrowMovement(1, 0, true); break;
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

                // Check
                case 'KeyH':
                    function getDisplayedDigit(cell)
                    {
                        let kyCells = [0, 1, 2, 3]
                            .filter(c => cell % 9 >= 3 * (c % 2) && cell % 9 < 6 + 3 * (c % 2) && cell / 9 >= 3 * ((c / 2) | 0) && cell / 9 < 6 + 3 * ((c / 2) | 0))
                            .map(c => ({ corner: c, kyCell: cell % 9 - 3 * (c % 2) + 6 * (((cell / 9) | 0) - 3 * ((c / 2) | 0)) }))
                            .filter(inf => state.circledDigits[inf.corner][inf.kyCell] === true);
                        if (kyCells.length > 1 && kyCells.some(inf => kyudokuGrids[inf.corner][inf.kyCell] !== kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]))
                            return null;
                        else if (kyCells.length >= 1)
                            return parseInt(kyudokuGrids[kyCells[0].corner][kyCells[0].kyCell]);
                        else if (state.enteredDigits[cell] !== null)
                            return parseInt(state.enteredDigits[cell]);
                        return null;
                    }
                    for (let i = 0; i < 9; i++)
                    {
                        for (let colA = 0; colA < 9; colA++)
                            for (let colB = colA + 1; colB < 9; colB++)
                                if (getDisplayedDigit(colA + 9 * i) !== null && getDisplayedDigit(colA + 9 * i) === getDisplayedDigit(colB + 9 * i))
                                    console.log(`In row ${i + 1}, cells ${colA + 1} and ${colB + 1} have the same value.`);
                        for (let rowA = 0; rowA < 9; rowA++)
                            for (let rowB = rowA + 1; rowB < 9; rowB++)
                                if (getDisplayedDigit(i + 9 * rowA) !== null && getDisplayedDigit(i + 9 * rowA) === getDisplayedDigit(i + 9 * rowB))
                                    console.log(`In column ${i + 1}, cells ${rowA + 1} and ${rowB + 1} have the same value.`);
                        for (let cellA = 0; cellA < 9; cellA++)
                            for (let cellB = cellA + 1; cellB < 9; cellB++)
                                if (getDisplayedDigit(cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))) !== null && getDisplayedDigit(cellA % 3 + 3 * (i % 3) + 9 * (((cellA / 3) | 0) + 3 * ((i / 3) | 0))) === getDisplayedDigit(cellB % 3 + 3 * (i % 3) + 9 * (((cellB / 3) | 0) + 3 * ((i / 3) | 0))))
                                    console.log(`In region ${i + 1}, cells ${cellA + 1} and ${cellB + 1} have the same value.`);
                    }
                    break;

                default:
                    anyFunction = false;
                    console.log(str, ev.code);
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