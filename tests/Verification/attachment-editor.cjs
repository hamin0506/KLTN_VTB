const fs=require('fs'),vm=require('vm'),assert=require('assert');
const elements=new Map();
const el=()=>({hidden:false,value:'',textContent:'',style:{},files:[],handlers:{},addEventListener(n,f){this.handlers[n]=f},setCustomValidity(v){this.validity=v},replaceChildren(){this.children=[]},append(v){this.children=[v]},setAttribute(k,v){this[k]=v}});
for(const s of ['#Attachment','[data-selected-file]','#attachment-error','[data-preview-panel]','[data-preview-content]','#RemoveAttachment','[data-remove-existing]','[data-undo-remove]','[data-remove-status]','[data-file-name]','[data-preview-file]','[data-clear-file]','[data-close-preview]'])elements.set(s,el());
const get=s=>elements.get(s),input=get('#Attachment');input.form=el();
let confirm=true,created=0,revoked=0;
const root={querySelector:get};const window={confirm:()=>confirm,addEventListener(){}};
vm.runInNewContext(fs.readFileSync('wwwroot/js/document-attachment.js','utf8'),{document:{querySelector:()=>root,createElement:el},window,URL:{createObjectURL:()=>{created++;return 'blob:test'},revokeObjectURL:()=>revoked++},Blob,Uint8Array,TextDecoder,Error});
async function select(name,body,size){const b=new Blob([body]);b.name=name;if(size)Object.defineProperty(b,'size',{value:size});input.files=[b];await input.handlers.change();}
(async()=>{
 await select('a.pdf','%PDF-1.7\n%%EOF');assert.equal(get('[data-selected-file]').hidden,false);assert.equal(get('#RemoveAttachment').value,'false');
 get('[data-preview-file]').handlers.click();assert.equal(created,1);assert.equal(get('[data-preview-content]').children[0].title,'Xem trước PDF chưa lưu');
 get('[data-clear-file]').handlers.click();assert.equal(input.value,'');assert.equal(revoked,1);assert.equal(get('#RemoveAttachment').value,'false');
 confirm=false;get('[data-remove-existing]').handlers.click();assert.equal(get('#RemoveAttachment').value,'false');
 confirm=true;get('[data-remove-existing]').handlers.click();assert.equal(get('#RemoveAttachment').value,'true');
 await select('a.pdf','%PDF-1.7\n%%EOF');assert.equal(get('#RemoveAttachment').value,'false');
 get('[data-clear-file]').handlers.click();assert.equal(get('#RemoveAttachment').value,'false');
 await select('bad.exe','abc');assert.equal(get('#attachment-error').hidden,false);assert.equal(get('[data-preview-file]').hidden,true);assert.ok(input.validity);
 get('[data-clear-file]').handlers.click();assert.equal(input.validity,'');
 await select('large.pdf','%PDF-1.7\n%%EOF',10485761);assert.ok(get('#attachment-error').textContent.includes('10 MB'));
 get('[data-clear-file]').handlers.click();
 await select('fake.pdf','not PDF');assert.ok(input.validity);
 const png=Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=','base64');
 await select('image.png',png);get('[data-preview-file]').handlers.click();assert.equal(get('[data-preview-content]').children[0].alt,'Ảnh đã chọn, chưa lưu');
 get('[data-close-preview]').handlers.click();assert.equal(revoked,created);
 console.log('PASS: PDF/PNG preview, URL release, clear selection, confirm/cancel deletion, replacement priority, invalid extension/content/size. No server calls.');
})().catch(e=>{console.error(e);process.exitCode=1});
