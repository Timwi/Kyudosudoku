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

    let loadedCriteria = JSON.parse(localStorage.getItem('criteria'));

    let form = document.getElementById('find-form');
    let constraints = JSON.parse(form.dataset.constraints);
    let curSort = (loadedCriteria && loadedCriteria.sort) || 'solvetime';
    let curAsc = (loadedCriteria && loadedCriteria.asc) || false;
    let constraintOptions = (loadedCriteria && loadedCriteria.constraints) || { 'include-constraints': [], 'exclude-constraints': [] };
    constraintOptions['include-constraints'] = constraintOptions['include-constraints'].filter(c => constraints.some(c2 => c2.id === c));
    constraintOptions['exclude-constraints'] = constraintOptions['exclude-constraints'].filter(c => constraints.some(c2 => c2.id === c));

    if (loadedCriteria && loadedCriteria.what)
        form.elements.what.value = loadedCriteria.what;
    if (loadedCriteria && loadedCriteria.filteravgmin)
        form.elements.filteravgmin.value = loadedCriteria.filteravgmin;
    if (loadedCriteria && loadedCriteria.filteravgmax)
        form.elements.filteravgmax.value = loadedCriteria.filteravgmax;

    function submit(pg, pgCount)
    {
        if (pg !== undefined && pgCount !== undefined && (pg < 0 || pg >= pgCount))
            return;

        let criteria = {
            sort: curSort,
            asc: curAsc,
            what: form.elements.what.value,
            page: pg | 0,
            filteravgmin: Math.min(form.elements.filteravgmin.value | 0, form.elements.filteravgmax.value | 0),
            filteravgmax: Math.max(form.elements.filteravgmin.value | 0, form.elements.filteravgmax.value | 0),
            constraints: constraintOptions
        };

        let req = new XMLHttpRequest();
        req.open('POST', `/find-puzzles`, true);
        req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        req.send(`criteria=${encodeURIComponent(JSON.stringify(criteria))}`);
        req.onload = function()
        {
            if (req.responseText)
            {
                var json = JSON.parse(req.responseText);
                if (json.html)
                {
                    let pageCount = json.pageCount;
                    let pageNum = json.pageNum;
                    let paginationControls = '';
                    if (pageCount > 1)
                    {
                        let pageNums = [];
                        if (pageCount <= 6)
                            pageNums = Array(pageCount).fill(null).map((_, c) => c);
                        else
                        {
                            pageNums.push(0);
                            if (pageNum >= 4)
                                pageNums.push(null);
                            for (let n = Math.max(1, pageNum - 2); n <= Math.min(pageCount - 2, pageNum + 2); n++)
                                pageNums.push(n);
                            if (pageNum < pageCount - 4)
                                pageNums.push(null);
                            pageNums.push(pageCount - 1);
                        }

                        paginationControls = `
                            <div class='pagination'>
                                <div>
                                    <button id='pag-prev'>Prev</button>
                                    ${pageNums.map(p => p === null ? " ... " : `<button class="pag${p === pageNum ? ' selected' : ''}" data-page='${p}'>${p + 1}</button>`).join(' ')}
                                    <button id='pag-next'>Next</button>
                                </div>
                                <div>
                                    <span class='page-label'>Page:</span> <input id='pag-input' value='${pageNum + 1}' type='number' min='1' max='${pageCount}' />
                                    <button id='pag-go'>Go</button>
                                </div>
                            </div>
                        `;
                    }

                    document.getElementById('results').innerHTML = json.html + paginationControls;

                    if (paginationControls)
                    {
                        document.getElementById('pag-prev').onclick = handler(function() { submit(pageNum - 1, pageCount); });
                        document.getElementById('pag-next').onclick = handler(function() { submit(pageNum + 1, pageCount); });
                        Array.from(document.getElementsByClassName('pag')).forEach(btn => { btn.onclick = handler(function() { submit(btn.dataset.page); }); });
                        document.getElementById('pag-go').onclick = handler(function() { submit(document.getElementById('pag-input').value - 1, pageCount); });
                    }

                    Array.from(document.querySelectorAll('a.sorter')).forEach(a =>
                    {
                        let sort = a.dataset.sort;
                        if (sort === curSort)
                            a.parentNode.appendChild(document.createTextNode(curAsc ? ' ▲' : ' ▼'));
                        a.onclick = handler(function()
                        {
                            if (sort === curSort)
                                curAsc = !curAsc;
                            else
                            {
                                curSort = sort;
                                curAsc = false;
                            }
                            submit();
                        });
                    });
                }
            }
        };

        localStorage.setItem('criteria', JSON.stringify(criteria));
    }

    submit(0, 1);

    Array.from(document.querySelectorAll('.trigger')).forEach(i => { i.onchange = function() { submit(); }; });

    function setConstraintUI(elem)
    {
        let str = '';
        let options = '';
        for (let tup of constraints)
        {
            if (constraintOptions[elem.id].includes(tup.id))
                str += `<button class='button remove-constraint' data-id='${tup.id}'>${tup.name}</button>`;
            else
                options += `<option value='${tup.id}'>${tup.name}</option>`;
        }
        elem.innerHTML = `${str}${options.length === 0 ? '' : `<select accesskey='${elem.id === 'include-constraints' ? ',' : '.'}'><option value=''>(pick one)</option>${options}</select>`}`;

        Array.from(elem.getElementsByTagName('select')).forEach(slct =>
        {
            slct.onchange = function(ev)
            {
                if (ev.target.value === '')
                    return;
                if (!constraintOptions[elem.id].includes(ev.target.value))
                    constraintOptions[elem.id].push(ev.target.value);
                setConstraintUI(elem);
                submit();
            };
        });

        Array.from(elem.getElementsByClassName('remove-constraint')).forEach(rm =>
        {
            rm.onclick = function()
            {
                let p = constraintOptions[elem.id].indexOf(rm.dataset.id);
                if (p !== -1)
                    constraintOptions[elem.id].splice(p, 1);
                setConstraintUI(elem);
                submit();
            };
        });
    }

    ['include-constraints', 'exclude-constraints'].forEach(id => { setConstraintUI(document.getElementById(id)); });
});