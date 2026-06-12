document.addEventListener("DOMContentLoaded", function () {

    const tagForm = document.getElementById("tagForm");
    const tagInput = document.getElementById("newTagName");
    const tagError = document.getElementById("tagError");

    if (tagForm) {
        tagForm.addEventListener("submit", function (e) {
            const value = tagInput.value.trim();

            if (value.length === 0) {
                e.preventDefault();
                tagError.textContent = "Tag name cannot be empty";
            } else if (value.length < 2) {
                e.preventDefault();
                tagError.textContent = "Tag name is too short";
            } else {
                tagError.textContent = "";
            }
        });
    }


    const commentForm = document.getElementById("commentForm");
    const commentInput = document.getElementById("commentContent");
    const commentError = document.getElementById("commentError");

    if (commentForm) {
        commentForm.addEventListener("submit", function (e) {
            const value = commentInput.value.trim();

            if (value.length === 0) {
                e.preventDefault();
                commentError.textContent = "Comment cannot be empty";
            } else if (value.length < 2) {
                e.preventDefault();
                commentError.textContent = "Comment is too short";
            } else {
                commentError.textContent = "";
            }
        });
    }


    const addExistingForm = document.getElementById("addExistingTagForm");
    const existingError = document.getElementById("existingTagError");

    if (addExistingForm) {
        addExistingForm.addEventListener("submit", function (e) {

            const select = addExistingForm.querySelector("select");

            if (!select || select.disabled || select.options.length === 0) {
                e.preventDefault();
                existingError.textContent = "No tags available to add";
            }
        });
    }

    const todoListSelect = document.getElementById("TodoListId");
    const assignedUserSelect = document.getElementById("AssignedUserId");

    if (todoListSelect && assignedUserSelect) {
        todoListSelect.addEventListener("change", function () {
            const listId = this.value;
            if (!listId) return;

            fetch(`/TodoTask/GetListMembers?listId=${listId}`)
                .then(response => {
                    if (!response.ok) throw new Error("Network response was not ok");
                    return response.json();
                })
                .then(data => {
                    // Save currently selected user to re-select if possible
                    const selectedUserId = assignedUserSelect.value;
                    
                    // Clear existing options
                    assignedUserSelect.innerHTML = '<option value="">Not assigned</option>';
                    
                    // Populate new options
                    let userFound = false;
                    data.forEach(user => {
                        const option = document.createElement("option");
                        option.value = user.id;
                        option.textContent = user.userName;
                        if (user.id === selectedUserId) {
                            option.selected = true;
                            userFound = true;
                        }
                        assignedUserSelect.appendChild(option);
                    });

                    // If previously selected user is no longer a member, default to "Not assigned"
                    if (!userFound && selectedUserId) {
                        assignedUserSelect.value = "";
                    }
                })
                .catch(error => console.error("Error fetching list members:", error));
        });
    }
});