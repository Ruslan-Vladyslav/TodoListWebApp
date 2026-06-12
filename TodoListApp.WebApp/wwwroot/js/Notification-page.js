function submitDeleteSelected() {
    var checkboxes = document.querySelectorAll('.notification-checkbox:checked');
    if (checkboxes.length === 0) {
        alert('Please select at least one notification to delete.');
        return;
    }
    if (!confirm('Delete selected notifications?')) {
        return;
    }
    var form = document.getElementById('deleteSelectedForm');
    var inputs = form.querySelectorAll('input[name="ids"]');
    inputs.forEach(function (i) { i.remove(); });

    checkboxes.forEach(function (cb) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'ids';
        input.value = cb.value;
        form.appendChild(input);
    });
    form.submit();
}