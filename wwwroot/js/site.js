// Only disable submit buttons after native and unobtrusive validation pass.
document.addEventListener('submit', function (event) {
 if (event.defaultPrevented) return;
 const form = event.target;
 if (!(form instanceof HTMLFormElement) || !form.checkValidity()) return;
 if (window.jQuery && window.jQuery.validator && !window.jQuery(form).valid()) return;
 form.setAttribute('aria-busy', 'true');
 form.querySelectorAll('button[type="submit"]').forEach(button => { button.disabled = true; button.dataset.originalText = button.textContent; button.textContent = 'Đang xử lý…'; });
});
window.addEventListener('pageshow', function () {
 document.querySelectorAll('form[aria-busy]').forEach(form => form.removeAttribute('aria-busy'));
 document.querySelectorAll('button[data-original-text]').forEach(button => { button.disabled = false; button.textContent = button.dataset.originalText; });
});// Native details keeps Enter/Space keyboard behavior; synchronize the accessible state.
document.querySelectorAll('details.nav-group').forEach(group => {
 const summary = group.querySelector('summary');
 const sync = () => summary.setAttribute('aria-expanded', String(group.open));
 sync(); group.addEventListener('toggle', sync);
});

