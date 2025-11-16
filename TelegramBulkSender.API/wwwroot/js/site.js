(() => {
    const toastPlaceholder = document.getElementById('toast-placeholder');
    window.showToast = (message, type = 'success') => {
        if (!toastPlaceholder) return;
        const wrapper = document.createElement('div');
        wrapper.innerHTML = `
            <div class="toast align-items-center text-bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>`;
        const toastElement = wrapper.firstElementChild;
        toastPlaceholder.append(toastElement);
        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    };
})();
