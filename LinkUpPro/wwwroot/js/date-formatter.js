document.addEventListener("DOMContentLoaded", function () {
    const timeElements = document.querySelectorAll("time.local-time");
    timeElements.forEach(el => {
        const utcDateStr = el.getAttribute("datetime");
        if (utcDateStr) {
            const date = new Date(utcDateStr + "Z"); // Ensure it's treated as UTC
            if (!isNaN(date)) {
                el.textContent = date.toLocaleString();
            }
        }
    });
});
