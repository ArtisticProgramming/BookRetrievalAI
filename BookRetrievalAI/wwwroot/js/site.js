const textarea = document.getElementById("chatInput");
const form = document.getElementById("chatForm");

textarea.addEventListener("keydown", function (e) {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        form.submit();
    }
});

form.addEventListener("submit", function () {
    setTimeout(() => {
        textarea.value = "";
        textarea.style.height = "auto";
    }, 50);
});

textarea.addEventListener("input", function () {
    this.style.height = "auto";
    this.style.height = this.scrollHeight + "px";
});

window.addEventListener("load", function () {
    const textarea = document.getElementById("chatInput");
    if (textarea) {
        textarea.focus();
    }
});