(() => {
 'use strict';
 document.querySelectorAll('[data-organization-filter]').forEach(form=>{
  const division=form.querySelector('[name="DivisionId"]'),department=form.querySelector('[name="DepartmentId"]'),notice=form.querySelector('[data-organization-notice]');
  if(!division||!department)return;
  const options=[...department.options];
  function sync(){let removed=false;for(const option of options){const incompatible=!!option.value&&!!division.value&&option.dataset.divisionId!==division.value;option.hidden=option.disabled=incompatible;if(incompatible&&option.selected){option.selected=false;removed=true;}}
   if(removed){department.value='';notice.textContent='Phòng ban trước đó không thuộc đơn vị đang chọn. Vui lòng chọn lại phòng ban hoặc để Tất cả.';notice.hidden=false;}else{notice.hidden=true;notice.textContent='';}}
  division.addEventListener('change',sync);sync();
  form.addEventListener('submit',e=>{const option=department.selectedOptions[0];if(option?.value&&division.value&&option.dataset.divisionId!==division.value){e.preventDefault();sync();department.focus();}});
 });
})();
