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

        pageInfo.textContent = `Page ${currentPage} / ${totalPages || 1}`;

        // désactiver boutons
        prevBtn.disabled = currentPage === 1;
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
