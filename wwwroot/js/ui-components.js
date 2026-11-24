(() => {
 'use strict';
 document.querySelectorAll('#main-content .table-responsive').forEach(region => {
  if (!region.hasAttribute('tabindex')) region.tabIndex=0;
  region.setAttribute('role','region');
  if (!region.hasAttribute('aria-label')) region.setAttribute('aria-label','Bảng dữ liệu — cuộn ngang để xem đủ cột');
 });
 document.querySelectorAll('.ui-form').forEach(form => {
  const fields=[...form.querySelectorAll('input:not([type="hidden"]),select,textarea')];
  const sync=()=>fields.forEach(input=>{
   const error=[...form.querySelectorAll('[data-valmsg-for]')].find(e=>e.dataset.valmsgFor===input.name);
   if(!error)return;
   if(!error.id)error.id=input.id+'-error';
   const ids=new Set((input.getAttribute('aria-describedby')||'').split(/\s+/).filter(Boolean));ids.add(error.id);
   const hint=input.closest('.mb-4,.mb-3')?.querySelector('.form-hint');
   if(hint){if(!hint.id)hint.id=input.id+'-hint';ids.add(hint.id);}
   input.setAttribute('aria-describedby',[...ids].join(' '));
   input.setAttribute('aria-invalid',String(error.classList.contains('field-validation-error')&&!!error.textContent.trim()));
  });
  sync();
  const first=fields.find(x=>x.getAttribute('aria-invalid')==='true');
  if(first)first.focus();
  else{const summary=form.querySelector('.validation-summary-errors');if(summary)summary.focus();}
  new MutationObserver(sync).observe(form,{subtree:true,childList:true,characterData:true,attributes:true,attributeFilter:['class']});
 });
})();
