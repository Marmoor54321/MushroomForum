// Rozwijanie/zamykanie notatek
document.querySelectorAll('.note-header').forEach(header => {
    header.addEventListener('click', function (e) {
        if (e.target.closest('.dropdown')) return;

        const noteId = header.dataset.noteId;
        const content = document.getElementById('note-content-' + noteId);

        content.style.display = content.style.display === 'none' ? 'block' : 'none';
    });
});

// Obsługa modala usuwania
const deleteModal = document.getElementById('deleteModal');
if (deleteModal) {
    deleteModal.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const id = button.getAttribute('data-id');
        const title = button.getAttribute('data-title');

        deleteModal.querySelector('#noteTitle').textContent = title;
        deleteModal.querySelector('#deleteId').value = id;
    });
}
