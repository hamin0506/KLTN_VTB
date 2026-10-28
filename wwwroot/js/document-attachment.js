(() => {
 const root=document.querySelector('[data-attachment-editor]');
 const input=root?.querySelector('#Attachment');if(!input)return;
 const find=s=>root.querySelector(s), selected=find('[data-selected-file]'), error=find('#attachment-error'), panel=find('[data-preview-panel]'), content=find('[data-preview-content]');
 const remove=find('#RemoveAttachment'), removeButton=find('[data-remove-existing]'), undo=find('[data-undo-remove]'), status=find('[data-remove-status]');
 let objectUrl=null, readyFile=null, generation=0, checking=false;
 function closePreview(){content.replaceChildren();panel.hidden=true;if(objectUrl){URL.revokeObjectURL(objectUrl);objectUrl=null;}}
 function removal(value){if(!remove)return;remove.value=value?'true':'false';removeButton.hidden=value;undo.hidden=!value;status.hidden=!value;}
 function clear(){generation++;checking=false;closePreview();readyFile=null;input.value='';selected.hidden=true;find('[data-file-name]').textContent='';error.hidden=true;input.setCustomValidity('');}
 function fail(message){error.textContent=message;error.hidden=false;input.setCustomValidity(message);}
 input.addEventListener('change',async()=>{
  const version=++generation;checking=true;closePreview();readyFile=null;selected.hidden=true;error.hidden=true;input.setCustomValidity('');
  const file=input.files[0];if(!file){checking=false;return;}
  // Selecting a replacement cancels any pending deletion. Clearing it later preserves the old file.
  removal(false);
  try{
   if(file.size===0||file.size>10*1024*1024)throw Error('Tệp phải có nội dung và không vượt quá 10 MB.');
   const ext=file.name.split('.').pop().toLowerCase();if(!['pdf','jpg','jpeg','png'].includes(ext))throw Error('Chỉ chấp nhận PDF, JPG, JPEG hoặc PNG.');
   const head=new Uint8Array(await file.slice(0,33).arrayBuffer());const tail=new Uint8Array(await file.slice(Math.max(0,file.size-2048)).arrayBuffer());
   const text=b=>new TextDecoder('ascii').decode(b);
   const valid=ext==='pdf'?text(head).startsWith('%PDF-')&&text(tail).includes('%%EOF'):ext==='png'?head.length>=33&&[137,80,78,71,13,10,26,10].every((v,i)=>head[i]===v)&&text(head.slice(12,16))==='IHDR'&&text(tail.slice(-8,-4))==='IEND':head[0]===255&&head[1]===216&&head[2]===255&&tail.at(-2)===255&&tail.at(-1)===217;
   if(!valid)throw Error('Nội dung tệp không khớp định dạng đã chọn hoặc tệp bị hỏng.');
   if(version!==generation)return;
   readyFile=new Blob([file],{type:ext==='pdf'?'application/pdf':ext==='png'?'image/png':'image/jpeg'});
   input.setCustomValidity('');error.hidden=true;find('[data-file-name]').textContent=file.name;find('[data-preview-file]').hidden=false;selected.hidden=false;
  }catch(ex){if(version===generation){input.value='';find('[data-file-name]').textContent=file.name;find('[data-preview-file]').hidden=true;selected.hidden=false;fail(ex.message||'Không thể đọc tệp đã chọn.');}}
  finally{if(version===generation)checking=false;}
 });
 find('[data-clear-file]').addEventListener('click',clear);
 find('[data-close-preview]').addEventListener('click',closePreview);
 find('[data-preview-file]').addEventListener('click',()=>{
  if(!readyFile)return;closePreview();objectUrl=URL.createObjectURL(readyFile);
  const element=document.createElement(readyFile.type==='application/pdf'?'iframe':'img');
  element.src=objectUrl;element.style.width='100%';
  if(readyFile.type==='application/pdf'){element.title='Xem trước PDF chưa lưu';element.style.height='65vh';element.setAttribute('sandbox','allow-same-origin');}
  else{element.alt='Ảnh đã chọn, chưa lưu';element.style.maxHeight='65vh';element.style.objectFit='contain';}
  content.append(element);panel.hidden=false;
 });
 removeButton?.addEventListener('click',()=>{if(window.confirm('Xóa tệp hiện có khi lưu dữ liệu? Bản ghi tài liệu vẫn được giữ lại.')){clear();removal(true);}});
 undo?.addEventListener('click',()=>removal(false));
 input.form.addEventListener('submit',event=>{if(checking){event.preventDefault();fail('Đang kiểm tra tệp, vui lòng chờ rồi nhấn Lưu dữ liệu.');}else input.setCustomValidity('');});
 window.addEventListener('pagehide',closePreview);
})();
