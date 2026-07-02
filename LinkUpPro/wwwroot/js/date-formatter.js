document.addEventListener("DOMContentLoaded", function () {
    const timeElements = document.querySelectorAll("time.local-time");
    timeElements.forEach(el => {
        const utcDateStr = el.getAttribute("datetime");
        if (utcDateStr) {
            let parseStr = utcDateStr;
            if (!parseStr.endsWith("Z") && !parseStr.includes("+") && !parseStr.includes("-", 10)) {
                parseStr += "Z";
            }
            const date = new Date(parseStr);
            if (!isNaN(date)) {
                el.textContent = date.toLocaleString([], { dateStyle: 'short', timeStyle: 'short' });
            }
        }
    });
});
