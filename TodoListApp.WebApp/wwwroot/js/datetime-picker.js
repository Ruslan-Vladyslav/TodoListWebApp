function initDateTimePickers() {

    document.querySelectorAll(".date-time-picker").forEach(picker => {

        const date = picker.querySelector(".datePart");
        const hour = picker.querySelector(".hourPart");
        const minute = picker.querySelector(".minutePart");

        const form = picker.closest("form");
        const hidden = form.querySelector(".dueDateHidden");

        form.addEventListener("submit", function () {

            if (!date.value || !hour.value || !minute.value) return;

            let h = hour.value;

            if (h === "24") {
                h = "00";
            }

            hidden.value = `${date.value}T${h}:${minute.value}`;
        });
    });
}

document.addEventListener("DOMContentLoaded", initDateTimePickers);