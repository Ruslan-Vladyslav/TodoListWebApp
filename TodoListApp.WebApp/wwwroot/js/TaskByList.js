document.addEventListener("DOMContentLoaded", function () {

    document.querySelectorAll(".task-row").forEach(row => {
        row.addEventListener("click", function () {
            const id = this.dataset.id;

            window.location.href = `/TodoTask/Details/${id}`;
        });
    });

});