window.addEventListener('DOMContentLoaded', function()
{
    Array.from(document.getElementsByClassName('puzzle')).forEach(puzzleDiv =>
    {
        let match = /^puzzle-(\d+)$/.exec(puzzleDiv.id);
        if (!match)
        {
            console.error(`Unexpected puzzle ID: ${puzzleDiv.id}`);
            return;
        }
        let puzzleId = parseInt(match[1]);

        let state = {
            circledDigits: Array(4).fill(null).map(_ => Array(36).fill(null)),
            cornerNotation: Array(81).fill(null).map(_ => []),
            centerNotation: Array(81).fill(null).map(_ => []),
            enteredDigits: Array(81).fill(null)
        };

        let mode = 0;

        let selectedCell = null;

        let kyudokuGrids = [0, 1, 2, 3].map(corner => Array(36).fill(null).map((_, cell) => parseInt(document.getElementById(`kyudo-${corner}-text-${cell}`).textContent)));

        let colors = [
            ["white", "#ccc"],
            ["hsl(0, 100%, 94%)", "hsl(0, 100%, 80%)"],
            ["white", "#ccc"],
            ["hsl(52, 100%, 89%)", "hsl(52, 100%, 70%)"],
            ["hsl(0, 0%, 94%)", "#aaa"],
            ["hsl(226, 100%, 94%)", "hsl(226, 100%, 80%)"],
            ["white", "#ccc"],
            ["hsl(103, 84%, 95%)", "hsl(103, 84%, 80%)"],
            ["white", "#ccc"]
        ];
        let invalidCellColor = '#f00';

        try
        {
            let item = JSON.parse(localStorage.getItem(`ky${puzzleId}`));
            if (item && item.circledDigits && item.cornerNotation && item.centerNotation && item.enteredDigits)
                state = item;
        }
        catch
        {
        }

        function cellColor(cell, isSelected)
        {
            return colors[(((cell % 9) / 3) | 0) + 3 * ((((cell / 9) | 0) / 3) | 0)][isSelected ? 1 : 0];
        }

        function updateVisuals()
        {
            if (localStorage)
                localStorage.setItem(`ky${puzzleId}`, JSON.stringify(state));
            for (let corner = 0; corner < 4; corner++)
            {
                for (let cell = 0; cell < 36; cell++)
                {
                    document.getElementById(`kyudo-${corner}-circle-${cell}`).setAttribute('opacity', state.circledDigits[corner][cell] === true ? '1' : '0');
                    document.getElementById(`kyudo-${corner}-x-${cell}`).setAttribute('opacity', state.circledDigits[corner][cell] === false ? '1' : '0');
                    let sudokuCell = cell % 6 + 3 * (corner % 2) + 9 * (((cell / 6) | 0) + 3 * ((corner / 2) | 0));
                    document.getElementById(`kyudo-${corner}-cell-${cell}`).setAttribute('fill', cellColor(sudokuCell, selectedCell === sudokuCell));
                }
            }

            for (let cell = 0; cell < 81; cell++)
            {
                let sudokuCell = document.getElementById(`sudoku-cell-${cell}`);
                let sudokuText = document.getElementById(`sudoku-text-${cell}`);
                let sudokuCenterText = document.getElementById(`sudoku-center-text-${cell}`);
                let sudokuCornerTexts = Array(8).fill(null).map((_, ix) => document.getElementById(`sudoku-corner-text-${cell}-${ix}`));

                let intendedText = null;
                let intendedCenterDigits = null;
                let intendedCornerDigits = null;

                sudokuCell.setAttribute('fill', cellColor(cell, selectedCell === cell));
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
                for (var i = 0; i < 8; i++)
                    sudokuCornerTexts[i].textContent = intendedCornerDigits !== null && i < intendedCornerDigits.length ? intendedCornerDigits[i] : '';
            }
        }
        updateVisuals();

        Array.from(puzzleDiv.getElementsByClassName('kyudo-cell')).forEach(cellRect =>
        {
            let match = /^kyudo-(\d+)-cell-(\d+)$/.exec(cellRect.id);
            if (!match)
            {
                console.error(`Unexpected cell ID: ${cellRect.id}`);
                return;
            }

            let corner = parseInt(match[1]);
            let cell = parseInt(match[2]);
            cellRect.onclick = function(ev)
            {
                if (state.circledDigits[corner][cell] === null)
                    state.circledDigits[corner][cell] = false;
                else if (state.circledDigits[corner][cell] === false)
                    state.circledDigits[corner][cell] = true;
                else
                    state.circledDigits[corner][cell] = null;
                updateVisuals();
                ev.stopPropagation();
                ev.preventDefault();
                return false;
            };
        });

        Array.from(puzzleDiv.getElementsByClassName('sudoku-cell')).forEach(cellRect =>
        {
            let match = /^sudoku-cell-(\d+)$/.exec(cellRect.id);
            if (!match)
            {
                console.error(`Unexpected cell ID: ${cellRect.id}`);
                return;
            }

            let cell = parseInt(match[1]);
            cellRect.onclick = function(ev)
            {
                if (selectedCell === cell)
                    selectedCell = null;
                else
                    selectedCell = cell;

                updateVisuals();
                ev.stopPropagation();
                ev.preventDefault();
                return false;
            };
        });

        puzzleDiv.addEventListener("keydown", ev => 
        {
            if (selectedCell !== null)
            {
                var str = ev.key;
                if (ev.shiftKey)
                    str = `Shift+${str}`;
                if (ev.altKey)
                    str = `Alt+${str}`;
                if (ev.ctrlKey)
                    str = `Ctrl+${str}`;

                switch (str)
                {
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        switch (mode)
                        {
                            case 0:
                                state.enteredDigits[selectedCell] = str;
                                break;
                            case 1:
                                if (state.centerNotation[selectedCell].includes(str))
                                    state.centerNotation[selectedCell].splice(state.centerNotation[selectedCell].indexOf(str), 1);
                                else
                                {
                                    state.centerNotation[selectedCell].push(str);
                                    state.centerNotation[selectedCell].sort();
                                }
                                break;
                            case 2:
                                if (state.cornerNotation[selectedCell].includes(str))
                                    state.cornerNotation[selectedCell].splice(state.cornerNotation[selectedCell].indexOf(str), 1);
                                else
                                {
                                    state.cornerNotation[selectedCell].push(str);
                                    state.cornerNotation[selectedCell].sort();
                                }
                                break;
                        }
                        break;

                    case 'Delete':
                        state.enteredDigits[selectedCell] = null;
                        state.centerNotation[selectedCell] = [];
                        state.cornerNotation[selectedCell] = [];
                        break;

                    case 'z':
                        mode = 0;
                        break;
                    case 'x':
                        mode = 1;
                        break;
                    case 'c':
                        mode = 2;
                        break;

                    case 'Backspace':
                    case 'Ctrl+z':
                    default:
                }
                updateVisuals();
                ev.stopPropagation();
                ev.preventDefault();
                return false;
            }
        });

        puzzleDiv.onclick = function()
        {
            selectedCell = null;
            updateVisuals();
        };
    });
});