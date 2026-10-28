/* Native details/checkbox dropdown. The original select remains the FormData source. */
window.DashboardMultiSelect=class {
 constructor(select){
  this.select=select;this.box=document.createElement('details');this.box.className='overview-multi';
  this.summary=document.createElement('summary');this.summary.id=select.id+'-toggle';this.summary.className='overview-multi-toggle';
  const label=document.querySelector('label[for="'+select.id+'"]');this.label=label?.textContent||'';
  if(label){label.htmlFor=this.summary.id;label.addEventListener('click',()=>this.summary.focus());}
  this.panel=document.createElement('div');this.panel.className='overview-multi-panel';this.panel.setAttribute('role','group');this.panel.setAttribute('aria-label',this.label);
  this.box.append(this.summary,this.panel);select.after(this.box);select.hidden=true;
  this.box.addEventListener('toggle',()=>{if(this.box.open)document.querySelectorAll('.overview-multi').forEach(other=>{if(other!==this.box)other.open=false;});});
  this.box.addEventListener('keydown',e=>{if(e.key==='Escape'){this.box.open=false;this.summary.focus();e.preventDefault();}});
  document.addEventListener('click',e=>{if(!this.box.contains(e.target))this.box.open=false;});
  document.addEventListener('focusin',e=>{if(!this.box.contains(e.target))this.box.open=false;});
  this.sync();
 }
 sync(){
  const selected=[...this.select.selectedOptions];const text=selected.length===0?this.select.dataset.placeholder:selected.length===1?selected[0].textContent:`${selected.length} đã chọn`;
  this.summary.textContent=text;this.summary.title=selected.map(x=>x.textContent).join(', ')||text;this.summary.setAttribute('aria-label',this.label+': '+this.summary.title);
  // Preserve focused checkbox when data or selection changes.
  const focused=document.activeElement?.dataset.optionValue;this.panel.replaceChildren();
  if(!this.select.options.length){const empty=document.createElement('span');empty.className='text-secondary';empty.textContent='Không có lựa chọn trong phạm vi hiện tại';this.panel.append(empty);}
  for(const option of this.select.options){const label=document.createElement('label');label.className='overview-multi-option';const input=document.createElement('input');input.type='checkbox';input.className='form-check-input';input.checked=option.selected;input.dataset.optionValue=option.value;const text=document.createElement('span');text.textContent=option.textContent;
   input.addEventListener('change',e=>{e.stopPropagation();option.selected=input.checked;this.select.dispatchEvent(new Event('change',{bubbles:true}));});
   label.append(input,text);this.panel.append(label);if(this.box.open&&focused===option.value)input.focus();
  }
 }
};
