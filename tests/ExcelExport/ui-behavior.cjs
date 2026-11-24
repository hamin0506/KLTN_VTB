// Minimal DOM harness for the actual export script. No browser or live data is used.
const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
class Element{
 constructor(value=''){this.value=value;this.disabled=false;this.required=false;this.handlers={};this.options=[];this.parentElement=this;this.textContent='';this.innerHTML='';}
 addEventListener(name,fn){this.handlers[name]=fn;} insertAdjacentHTML(){} setAttribute(){} removeAttribute(){} append(){} remove(){} showModal(){this.open=true;}close(){this.open=false;this.handlers.close?.();} querySelector(key){return this.children?.[key]??null;}
 add(o){this.options.push(o);}replaceChildren(...o){this.options=o;this.value=o[0]?.value??'';}
}
const employee=new Element(),dialog=new Element();let requests=[],passed=0;
const mode=new Element('current'),division=new Element(),department=new Element(),status=new Element(),submit=new Element(),form=new Element();
const message=new Element();submit.children={'.export-message':message};
dialog.children={'form':form,'#export-mode':mode,'#export-division':division,'#export-department':department,'#export-status':status,'.export-submit':submit,'.btn-close':new Element(),'.export-cancel':new Element()};
form.reportValidity=()=>!(division.required&&!division.value||department.required&&!department.value);
const context={console,URLSearchParams,window:{},location:{search:'?q=Test&quality=THIEU_HO_SO&divisionId=1&departmentId=11&page=4&status=TamHoanHDLD'},Option:function(text,value){return {text,value:String(value)};},document:{querySelectorAll:()=>[],querySelector:()=>employee,createElement:(tag)=>tag==='dialog'?dialog:new Element(),body:new Element()},fetch:async url=>{requests.push(String(url));if(String(url).includes('EmployeeOptions'))return{ok:true,json:async()=>({divisions:[{id:1,name:'A'},{id:2,name:'B'}],departments:[{id:11,divisionId:1,name:'A1'},{id:12,divisionId:1,name:'A2'},{id:21,divisionId:2,name:'B1'}]})};return{ok:false,status:404,json:async()=>({message:'Không có dữ liệu phù hợp để xuất.'})};},setTimeout,URL};
employee.dataset={};employee.dataset.employmentLabels=JSON.stringify({DangLamViec:'Đang làm việc',TamHoanHDLD:'Tạm hoãn HĐLĐ',ChamDutHDLD:'Chấm dứt HĐLĐ',NghiHuu:'Nghỉ hưu'});
status.options=['DangLamViec','TamHoanHDLD','ChamDutHDLD','NghiHuu'].map(value=>({value,textContent:''}));
vm.runInNewContext(fs.readFileSync('wwwroot/js/excel-export.js','utf8'),context);
function check(ok,name){assert.ok(ok,name);passed++;console.log('PASS '+name);}
(async()=>{
 await employee.handlers.click();check(dialog.open&&mode.value==='current','modal opens with current filter scope');check(status.options.every(o=>o.textContent===JSON.parse(employee.dataset.employmentLabels)[o.value]),'all four labels from server; raw option values retained');check(status.value==='TamHoanHDLD','status initialized from table');check(division.disabled&&department.disabled,'current scope preserves table organization');
 mode.value='division';mode.onchange();check(division.required&&!division.disabled&&department.disabled,'division required only in division scope');
 mode.value='department';mode.onchange();check(department.required&&!department.disabled,'department scope requires department');
 division.value='1';division.onchange();department.value='11';division.value='2';division.onchange();check(department.value===''&&department.options.length===2&&department.options[1].value==='21','changing division filters options and clears incompatible department');
 let before=requests.length;form.handlers.submit({preventDefault(){}});check(requests.length===before,'missing required department blocks request');
 department.value='21';form.handlers.submit({preventDefault(){}});await new Promise(r=>setTimeout(r,0));let p=new URL(requests.at(-1),'http://test').searchParams;check(p.get('departmentId')==='21'&&p.get('divisionId')==='2'&&!p.has('q')&&!p.has('quality'),'department export has correct scope and no old search');
 check(message.textContent.includes('Không có dữ liệu')&&!submit.disabled,'no-data response visible and loading released');
 mode.value='all';mode.onchange();form.handlers.submit({preventDefault(){}});await new Promise(r=>setTimeout(r,0));p=new URL(requests.at(-1),'http://test').searchParams;check(p.get('mode')==='all'&&!p.has('divisionId')&&!p.has('departmentId'),'all scope excludes organization restrictions');
 mode.value='current';mode.onchange();form.handlers.submit({preventDefault(){}});await new Promise(r=>setTimeout(r,0));p=new URL(requests.at(-1),'http://test').searchParams;check(p.get('q')==='Test'&&p.get('quality')==='THIEU_HO_SO'&&p.get('departmentId')==='11'&&!p.has('page'),'current scope preserves applied search/quality/department without page');
 console.log(`${passed} JavaScript behavior checks passed (simulated DOM, not visual verification).`);
})().catch(e=>{console.error(e);process.exitCode=1;});
