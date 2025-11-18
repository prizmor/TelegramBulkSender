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

    const root = document.documentElement;
    const toggle = document.getElementById('theme-toggle');
    const label = toggle ? toggle.querySelector('.theme-label') : null;

    const applyTheme = theme => {
        if (theme === 'dark') {
            root.setAttribute('data-theme', 'dark');
        } else {
            root.removeAttribute('data-theme');
        }
        if (label) {
            label.textContent = theme === 'dark' ? 'Светлая тема' : 'Тёмная тема';
        }
    };

    const storedTheme = localStorage.getItem('theme');
    const initialTheme = storedTheme === 'dark' || storedTheme === 'light' ? storedTheme : 'light';
    applyTheme(initialTheme);

    if (toggle) {
        toggle.addEventListener('click', () => {
            const current = root.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
            const next = current === 'dark' ? 'light' : 'dark';
            localStorage.setItem('theme', next);
            applyTheme(next);
        });
    }
})();

