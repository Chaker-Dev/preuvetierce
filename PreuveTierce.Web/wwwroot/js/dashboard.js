window.copyHash = async function (element, hash) {
    try {
        await navigator.clipboard.writeText(hash);

        const span = element.querySelector("span");
        if (!span) return;

        const originalText = span.dataset.original || span.textContent;
        span.dataset.original = originalText;

        span.classList.add("bg-green-100", "text-green-700", "scale-105");
        span.textContent = "Copié ✓";

        setTimeout(() => {
            span.textContent = span.dataset.original;
            span.classList.remove("bg-green-100", "text-green-700", "scale-105");
        }, 1200);

    } catch (err) {
        console.error("Clipboard error:", err);
    }
};
document.addEventListener("DOMContentLoaded", () => {

    const tableBody = document.getElementById("tableBody");
    if (!tableBody) return;

    const rowsPerPage = 5;
    let currentPage = 1;

    const rows = Array.from(tableBody.querySelectorAll("tr"));
    const searchInput = document.getElementById("searchInput");
    const pageInfo = document.getElementById("pageInfo");
    const prevBtn = document.getElementById("prevPage");
    const nextBtn = document.getElementById("nextPage");

    function renderTable() {
        if (!rows.length) return;

        const filter = searchInput?.value.toLowerCase() ?? "";
        const filteredRows = rows.filter(r =>
            r.innerText.toLowerCase().includes(filter)
        );

        const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
        currentPage = Math.min(currentPage, totalPages || 1);

        rows.forEach(r => r.style.display = "none");

        filteredRows
            .slice((currentPage - 1) * rowsPerPage, currentPage * rowsPerPage)
            .forEach(r => r.style.display = "");

        if (pageInfo)
            pageInfo.textContent = `Page ${currentPage} / ${totalPages || 1}`;

        if (prevBtn)
            prevBtn.disabled = currentPage === 1;

        if (nextBtn)
            nextBtn.disabled = currentPage === totalPages || totalPages === 0;
    }

    searchInput?.addEventListener("input", () => {
        currentPage = 1;
        renderTable();
    });

    prevBtn?.addEventListener("click", () => {
        if (currentPage > 1) {
            currentPage--;
            renderTable();
        }
    });

    nextBtn?.addEventListener("click", () => {
        currentPage++;
        renderTable();
    });

    renderTable();
});
