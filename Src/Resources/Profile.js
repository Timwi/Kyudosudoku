window.addEventListener('DOMContentLoaded', function ()
{
    var dateText = document.getElementById("date-text");

    var chartDiv = document.querySelector(".chart");
    var leftArrow = document.getElementById("leftArrow");
    var rightArrow = document.getElementById("rightArrow");
    var data = document.querySelector(".profile-container").dataset;

    var userId = data.userid | 0;
    var month = data.month | 0;
    var year = data.year | 0;

    function sendReq()
    {

        let req = new XMLHttpRequest();
        req.open('POST', `/profile-table`, true);
        req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        req.send(`month=${month}&year=${year}&userId=${userId}`);

        req.onload = function ()
        {
            var json = JSON.parse(req.responseText);

            chartDiv.innerHTML = json.html;
            dateText.innerHTML = json.dateText;
        };
    }

    leftArrow.addEventListener("click", function ()
    {
        month--;
        if (month < 1)
        {
            month = 12;
            year--;
        }

        sendReq();
    });

    rightArrow.addEventListener("click", function ()
    {
        month++;
        if (month > 12)
        {
            month = 1;
            year++;
        }

        sendReq();
    });
});